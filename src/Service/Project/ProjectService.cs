namespace Service.Project
{
    using Entity.Project;
    using Entity.ValueType;
    using Platform.Context;
    using Platform.Enum;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Platform.Extension;
    using User;
    using Extension;
    public class ProjectService : BaseService<Project>
    {
        public ProjectService(IContextRepository contextRepository) : base(contextRepository)
        {
        }

        public override string Create(Project project)
        {
            project.ActualDateRange = new DateRange();
            project.ActualHours = new Dictionary<string, double>();

            UpdatePlanParts(project);

            return base.Create(project);
        }

        private void UpdatePlanParts(Project project)
        {
            if (project.IsPublic)
            {
                return;
            }

            project.PlanDateRange = new DateRange(project.Tasks.Select(o => o.PlanDateRange.StartDate).Min(), project.Tasks.Select(o => o.PlanDateRange.EndDate).Max());
            //初始发布日期
            project.PlanEndDate = project.PlanEndDate.IsEmpty() ? project.PublishDate : project.PlanEndDate;
            project.PlanHours = new Dictionary<string, double>();
            project.UserIds = new List<string>();

            foreach (var item in project.Tasks)
            {
                project.UserIds.Add(item.UserId);

                if (project.PlanHours.ContainsKey(item.UserId))
                {
                    project.PlanHours[item.UserId] += item.PlanHour;
                }
                else
                {
                    project.PlanHours.Add(item.UserId, item.PlanHour);
                }

                if(item.ActualDateRange == null)
                {
                    item.ActualDateRange = new DateRange();
                }
            }

            project.UserIds = project.UserIds.Distinct().ToList();

            project.IsReviewed = project.Tasks.Sum(o => o.IsReviewed ? 1 : 0) == project.Tasks.Count;
        }

        public override List<string> Create(List<Project> projects)
        {
            var result = new List<string>();

            foreach (var item in projects)
            {
                result.Add(Create(item));
            }

            return result;
        }

        //得到用户相关的项目
        //Admin：所有项目
        //Manager：得到部门成员所有的项目
        //Leader：得到小组成员所有的项目
        //User：得到相关的所有项目
        public List<Project> GetProjectsByUserId(string userId)
        {
            var userIds = (List<string>)ServiceFactory.Instance.GetService<DepartmentService>().GetSubordinatesByUserId(userId);
            return Get().Where(o => o.IsPublic || o.OwnerIds.Intersect(userIds).Count() > 0 || o.UserIds.Intersect(userIds).Count() > 0).ToList();
        }

        public List<Project> GetProjectsByUserIds(List<string> userIds)
        {
            var result = new List<Project>();

            foreach(var item in userIds)
            {
                result.AddRange(GetProjectsByUserId(item));
            }

            return result.Distinct().ToList();
        }

        public List<Project> GetProjectsByOwnerId(string ownerId)
        {
            return Get().Where(o => o.IsPublic || o.OwnerIds.Contains(ownerId)).ToList();
        }

        public List<Project> GetProjectsByOwnerIds(List<string> ownerIds)
        {
            return Get().Where(o => o.IsPublic || o.OwnerIds.Intersect(ownerIds).Count() > 0 ).ToList();
        }

        public void UpdateTask(string projectId, ProjectTask task)
        {
            var project = Get(projectId);
            var index = project.Tasks.FindIndex(o => o.Name == task.Name);

            base.Update(projectId, string.Format("Tasks.{0}", index), task);
        }

        public void UpdateTaskStatus(string projectId, int taskId, Status status)
        {
            var project = Get(projectId);
            var index = project.Tasks.FindIndex(o => o.Id == taskId);

            base.Update(projectId, string.Format("Tasks.{0}.Status", index), status);
        }

        public void UpdateProjectStatus(string projectId, Status status)
        {
            var project = Get(projectId);
            base.Update(projectId, "Status", status);
        }

        public void UpdateStatus(string projectId, int taskId, bool isDone)
        {
            var project = Get(projectId);

            if (taskId != -1)
            {
                var projectTask = project.Tasks.Find(o => o.Id == taskId);

                if (isDone)
                {
                    projectTask.Status = Status.Done;
                }
                else if (projectTask.Status == Status.Done)
                {
                    projectTask.Status = Status.Ongoing;
                }

                UpdateTaskStatus(projectId, taskId, projectTask.Status);
            }
            else
            {
                if (isDone)
                {
                    project.Status = Status.Done;
                }
                else if (project.Status == Status.Done)
                {
                    project.Status = Status.Ongoing;
                }

                UpdateProjectStatus(projectId, project.Status);
            }
        }

        public void CloseProject(string projectId)
        {
            var project = Get(projectId);

            foreach(var task in project.Tasks)
            {
                task.Status = Status.Done;
            }

            project.UpdateProject();

            base.Update(project);
        }

        public void CloseProject(string projectId, DateTime endDate)
        {
            var project = Get(projectId);

            //更新状态和实际结束日期
            project.Status = Status.Done;

            if (project.ActualDateRange == null)
            {
                project.ActualDateRange = new DateRange();
            }

            project.ActualDateRange.EndDate = endDate;

            base.Update(project);
        }

        public void EditForUser(string id, string name, string comment, string description, string codeReview)
        {
            if (!string.IsNullOrEmpty(name))
            {
                Update(id, "Name", name);
            }

            if (!string.IsNullOrEmpty(comment))
            {
                Update(id, "Comment", comment);
            }

            if (!string.IsNullOrEmpty(description))
            {
                Update(id, "Description", description);
            }

            if (!string.IsNullOrEmpty(codeReview))
            {
                Update(id, "CodeReview", codeReview);
            }
        }

        public void PostponeProject(string projectId, PostponeReason postponeReason, string comment, DateTime endDate)
        {
            if (endDate.IsEmpty())
            {
                endDate = DateTime.Today;
            }

            var project = Get(projectId);

            if (project.PostponeReasons == null)
            {
                var reasons = new List<PostponeReason>();
                reasons.Add(postponeReason);
                project.PostponeReasons = reasons;
            }
            else
            {
                project.PostponeReasons.Add(postponeReason);
            }

            if(project.Status == Status.Done)
            {
                Update(project.Id, "Status", Status.Ongoing);
            }

            Update(projectId, "PostponeReasons", project.PostponeReasons);
            Update(projectId, "Comment", comment);
            Update(projectId, "PublishDate", endDate);
            Update(projectId, "ActualDateRange.EndDate", DateTime.MinValue);
        }

        override
        public void Update(Project project)
        {
            //如果删除Task,删除对应的TimeSheet
            var dbProject = Get(project.Id);

            if(!project.Tasks.IsEmpty())
            {
                var taskIds = dbProject.Tasks.Select(o => o.Id).Except(project.Tasks.Select(o => o.Id)).ToList();

                if (taskIds.Count > 0)
                {
                    ServiceFactory.Instance.GetService<TimeSheetService>().DeleteTasks(project.Id, dbProject.Tasks.FindAll(o => taskIds.Contains(o.Id)));
                }

                UpdatePlanParts(project);

                //不存在的用户删除实际时间
                var additionalIds = project.ActualHours.Keys.Except(project.UserIds).ToList();

                foreach(var item in additionalIds)
                {
                    project.ActualHours.Remove(item);
                }
            }

            base.Update(project);
        }

        //获取用户在该段时间有计划时间的项目,减掉实际结束的项目
        public List<Project> GetProjectsByUserId(string userId, DateTime startDate, DateTime endDate)
        {
            return Get().Where(o => o.UserIds.Contains(userId) 
                && !o.PlanHours.IsEmpty() && o.PlanHours.ContainsKey(userId) && o.PlanHours[userId] > 0
                && o.PlanDateRange.EndDate >= startDate && o.PlanDateRange.StartDate <= endDate
                && !(o.Status == Status.Done && o.ActualDateRange.EndDate < startDate)).ToList(); 
        }

        //获取用户在该段时间有计划时间的项目,减掉实际结束的项目
        public List<Project> GetProjectsByUserIds(IEnumerable<string> userIds, DateTime startDate, DateTime endDate)
        {
            var result = new List<Project>();

            foreach(var item in userIds)
            {
                result.AddRange(GetProjectsByUserId(item, startDate, endDate));
            }

            return result.Distinct().ToList();
        }

        public List<Project> GetProjectsByOwnerId(string ownerId, DateTime startDate, DateTime endDate)
        {
            return Get().Where(o => o.OwnerIds.Contains(ownerId)
            && !o.PlanHours.IsEmpty()
            && o.PlanDateRange.EndDate >= startDate && o.PlanDateRange.StartDate <= endDate
            && !(o.Status == Status.Done && o.ActualDateRange.EndDate < startDate)).ToList();
        }

        //获取用户在该段时间有计划时间的项目,减掉实际结束的项目
        //key为ProjectId
        public Dictionary<string, double> GetPlanHoursByProject(string userId, DateTime startDate, DateTime endDate)
        {
            var projects = Get().Where(o => o.UserIds.Contains(userId)
                && !o.PlanHours.IsEmpty() && o.PlanHours.ContainsKey(userId) && o.PlanHours[userId] > 0
                && o.PlanDateRange.EndDate >= startDate && o.PlanDateRange.StartDate <= endDate
                && !(o.Status == Status.Done && o.ActualDateRange.EndDate < startDate)).ToList();

            return projects.ToDictionary(o => o.Id, o => o.GetPlanHour(userId, startDate, endDate));
        }

        //获取用户在该段时间有计划时间的项目,减掉实际结束的项目
        //key为ProjectId
        public Dictionary<DateTime, double> GetPlanHoursByDate(string userId, DateTime startDate, DateTime endDate)
        {
            var result = new Dictionary<DateTime, double>();

            var projects = Get().Where(o => o.UserIds.Contains(userId)
                && !o.PlanHours.IsEmpty() && o.PlanHours.ContainsKey(userId) && o.PlanHours[userId] > 0
                && o.PlanDateRange.EndDate >= startDate && o.PlanDateRange.StartDate <= endDate
                && !(o.Status == Status.Done && o.ActualDateRange.EndDate < startDate)).ToList();

            var firstDate = new DateTime(startDate.Ticks);

            while (firstDate <= endDate)
            {
                result.Add(firstDate, projects.Sum(o => o.GetPlanHour(userId, firstDate, firstDate)));
                firstDate = firstDate.AddDays(1);
            }

            return result;
        }

        public long Murmur(string projectId, long tick, string userId, string content)
        {
            var project = Get(projectId);

            long currentTick = DateTime.Now.Ticks;

            if (project != null)
            {
                if(project.Murmurs == null)
                {
                    project.Murmurs = new Dictionary<long, KeyValuePair<string, string>>();
                }

                if (tick > 0 && project.Murmurs.ContainsKey(tick))
                {
                    project.Murmurs[tick] = new KeyValuePair<string, string>(userId, content);
                }
                else
                {
                    project.Murmurs.Add(currentTick, new KeyValuePair<string, string>(userId, content));
                }

                Update(projectId, "Murmurs", project.Murmurs);
            }

            return tick > 0 ? tick : currentTick;
        }

        public void DeleteMurmur(string projectId, long tick)
        {
            var project = Get(projectId);

            long currentTick = DateTime.Now.Ticks;

            if (project != null)
            {
                if (project.Murmurs == null)
                {
                    project.Murmurs = new Dictionary<long, KeyValuePair<string, string>>();
                }

                if (project.Murmurs.ContainsKey(tick))
                {
                    project.Murmurs.Remove(tick);
                }

                Update(projectId, "Murmurs", project.Murmurs);
            }
        }

        //提交TimeSheet后，项目数据有更新
        public void UpdateActualParts(TimeSheet timeSheet)
        {
            var project = Get(timeSheet.ProjectId);

            if (project == null)
            {
                return;
            }

            if (timeSheet == null)
            {
                return;
            }

            if (project.ActualHours == null)
            {
                project.ActualHours = new Dictionary<string, double>();
            }

            //更新用户时长
            if (project.Tasks.IsEmpty())
            {
                project.ActualDateRange.StartDate = timeSheet.GetEarliestDate();

                if (project.ActualHours.ContainsKey(timeSheet.UserId))
                {
                    project.ActualHours[timeSheet.UserId] = timeSheet.GetTotalHours();
                }
                else
                {
                    project.ActualHours.Add(timeSheet.UserId, timeSheet.GetTotalHours());
                }
            }
            //新建的带Task的项目
            else
            {
                foreach (var task in project.Tasks)
                {
                    var weekTimeSheet = timeSheet.WeekTimeSheets.SelectMany(o => o.Value).Where(o => o.Key == task.Id);
                    if(weekTimeSheet.IsEmpty())
                    {
                        continue;
                    }
                    //先更新任务时长
                    task.ActualHour = weekTimeSheet.Sum(o => o.Value.Sum());

                    if(task.ActualDateRange == null)
                    {
                        task.ActualDateRange = new DateRange();
                    }

                    //更新起始日期
                    task.ActualDateRange.StartDate = timeSheet.GetTaskStartDate(task.Id);

                    //更新任务状态
                    if (task.Status == Status.Done)
                    {
                        //更新任务的结束日期
                        task.ActualDateRange.EndDate = timeSheet.GetTaskEndDate(task.Id);
                    }
                    else
                    {
                        //更新任务的结束日期
                        task.ActualDateRange.EndDate = DateTime.MinValue;

                        if (task.ActualHour > 0)
                        {
                            task.Status = Status.Ongoing;
                        }
                        else
                        {
                            task.Status = Status.Pending;
                        }
                    }
                }

                //如果所有的任务都完成了，项目就完成了
                if (!project.Tasks.IsEmpty())
                {
                    if(project.ActualDateRange.StartDate.IsEmpty() || timeSheet.GetEarliestDate() < project.ActualDateRange.StartDate)
                    {
                        project.ActualDateRange.StartDate = timeSheet.GetEarliestDate();
                    }

                    var sum = project.Tasks.Sum(o => (int)o.Status);

                    if(sum == project.Tasks.Count * 2)
                    {
                        project.Status = Status.Done;
                        if (project.ActualDateRange.EndDate.IsEmpty() || timeSheet.GetLatestDate() > project.ActualDateRange.EndDate)
                        {
                            project.ActualDateRange.EndDate = timeSheet.GetLatestDate();
                        }
                    }
                    else if(sum > 0)
                    {
                        project.Status = Status.Ongoing;
                    }
                    else
                    {
                        project.Status = Status.Pending;
                    }
                }

                var hours = project.Tasks.GroupBy(o => o.UserId, o => o.ActualHour, (key, g) => new {
                    Id = key,
                    Hours = g.Sum()
                }).ToList();

                foreach (var item in hours)
                {
                    if (project.ActualHours.ContainsKey(item.Id))
                    {
                        project.ActualHours[item.Id] = item.Hours;
                    }
                    else
                    {
                        project.ActualHours.Add(item.Id, item.Hours);
                    }
                }
            }

            base.Update(project);
        }
    }
}
