namespace Web.Util
{
    using Entity.Project;
    using Entity.User;
    using Entity.ValueType;
    using Platform.Enum;
    using Platform.Extension;
    using Service.Project;
    using Service.User;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Web.Helper;

    public static class TimeSheetUtil
    {
        public static void ResetTimeSheet(ProjectService projectService, TimeSheetService timeSheetService, List<TimeSheet> timeSheets = null)
        {
            var projectIds = projectService.GetIds();

            if (timeSheets.IsEmpty())
            {
                timeSheets = timeSheetService.Get();
            }

            foreach (var item in timeSheets)
            {
                if (!projectIds.Contains(item.ProjectId))
                {
                    timeSheetService.Delete(item.Id);
                }
                else
                {
                    var project = projectService.Get(item.ProjectId);

                    if (!project.IsPublic)
                    {
                        foreach (var week in item.WeekTimeSheets)
                        {
                            var taskIds = new List<int>(week.Value.Keys);
                            foreach (var taskId in taskIds)
                            {
                                if (!project.Tasks.Select(o => o.Id).Contains(taskId))
                                {
                                    week.Value.Remove(taskId);
                                }
                            }
                        }

                        timeSheetService.Update(item);
                    }
                }
            }
        }

        public static string RefreshProjectTask(ProjectService projectService, DepartmentService departmentService, UserService userService, List<Project> projects = null)
        {
            if (projects.IsEmpty())
            {
                projects = projectService.Get();
            }

            foreach (var project in projects)
            {
                if (!project.Tasks.IsEmpty())
                {
                    foreach (var task in project.Tasks)
                    {
                        if (task.Values == null)
                        {
                            task.Values = new Dictionary<string, int>();

                            var ownerId = departmentService.GetLeaderIdByUserId(task.UserId);

                            if (ownerId == null)
                            {
                                continue;
                            }

                            if (userService.Get(task.UserId).UserType == UserType.User)
                            {
                                var group = departmentService.GetUserGroupsByUserId(task.UserId).First();

                                foreach (var userId in group.UserIds)
                                {
                                    task.Values.Add(userId, userId == ownerId ? (int)task.CalculateValue() : 0);
                                }
                            }
                            else
                            {
                                task.Values.Add(ownerId, (int)task.CalculateValue());
                            }
                        }
                    }

                    projectService.Update(project);
                }
            }

            return "Done";
        }

        public static void UpdateProject(TimeSheetService timeSheetService, ProjectService projectService, List<Project> projects = null)
        {
            if (projects.IsEmpty())
            {
                projects = projectService.Get();
            }

            foreach (var project in projects)
            {
                if (project.IsPublic)
                {
                    continue;
                }

                var timeSheets = timeSheetService.Get(o => o.ProjectId == project.Id);

                project.ActualHours = new Dictionary<string, double>();

                //更新总时间
                foreach (var item in timeSheets)
                {
                    project.ActualHours.Add(item.UserId, item.GetTotalHours());
                }

                //更新Task
                foreach (var task in project.Tasks)
                {
                    var timeSheet = timeSheets.Where(o => o.UserId == task.UserId).FirstOrDefault();

                    //为空，则任务为初始状态
                    if (timeSheet == null)
                    {
                        task.Status = Status.Pending;
                        task.ActualHour = 0;
                        task.ActualDateRange = new DateRange();
                    }
                    else
                    {
                        task.ActualHour = timeSheet.GetTaskHours(task.Id);

                        if (task.Status == Status.Done)
                        {
                            task.ActualDateRange.StartDate = timeSheet.GetTaskStartDate(task.Id);
                            task.ActualDateRange.EndDate = timeSheet.GetTaskEndDate(task.Id);
                        }
                        else
                        {
                            if (task.ActualHour > 0)
                            {
                                task.Status = Status.Ongoing;
                                task.ActualDateRange.StartDate = timeSheet.GetTaskStartDate(task.Id);
                                task.ActualDateRange.EndDate = DateTime.MinValue;
                            }
                            else
                            {
                                task.Status = Status.Pending;
                                task.ActualDateRange.StartDate = DateTime.MinValue;
                                task.ActualDateRange.EndDate = DateTime.MinValue;
                            }
                        }
                    }
                }

                project.UpdateProjectStatus();
                project.UpdateProjectActualTime();
                projectService.Update(project);
            }
        }

        public static void ResetUserTimeSheet(TimeSheetService timeSheetService, UserTimeSheetStatusService userTimeSheetStatusService, List<UserTimeSheetStatus> userTimeSheetStatuses = null)
        {
            var timeSheets = timeSheetService.Get();

            if (userTimeSheetStatuses.IsEmpty())
            {
                userTimeSheetStatuses = userTimeSheetStatusService.Get();
            }

            foreach (var userTimeSheet in userTimeSheetStatuses)
            {
                var timeSheet = timeSheets.Where(o => o.UserId == userTimeSheet.Id).ToList();

                var startDate = TimeSheetHelper.MIN_DATE;
                var currentMonday = DateTime.Today.GetMonday();

                while (startDate <= currentMonday)
                {
                    var monday = startDate.GetTimeSheetId();
                    var hour = timeSheet.Where(o => o.WeekTimeSheets.ContainsKey(monday)).Select(o => o.WeekTimeSheets[monday].Sum(p => p.Value.Sum())).Sum();

                    var status = Status.Pending;

                    if (hour >= 40)
                    {
                        status = Status.Done;
                    }
                    else if (hour > 0)
                    {
                        status = Status.Ongoing;
                    }

                    KeyValuePair<Status, double> value = new KeyValuePair<Status, double>(status, hour);

                    if (userTimeSheet.Weeks.ContainsKey(monday))
                    {
                        userTimeSheet.Weeks[monday] = value;
                    }
                    else
                    {
                        userTimeSheet.Weeks.Add(monday, value);
                    }

                    startDate = startDate.AddDays(7);
                }

                userTimeSheetStatusService.Update(userTimeSheet);
            }
        }
    }
}
