namespace Web.Controllers
{
    using Common;
    using Extension;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Platform.Enum;
    using Platform.Extension;
    using Service.Project;
    using Service.User;
    using System;
    using System.Linq;
    using Model;
    using Entity.User;
    using System.Collections.Generic;
    using Entity.ValueType;
    using Helper;

    public class HomeController : BaseController
    {
        public IActionResult Index(string successMsg, string errorMsg)
        {
            this.BuildHeaderMsg(successMsg, errorMsg);
            var random = new Random();
            var rawUsers = this.GetService<UserService>().Get().FindAll(o => o.AccountType == AccountType.Public);
            var users = rawUsers.Where(o => !o.Pics.IsEmpty()).OrderBy(o => random.Next()).ToList();
            users.AddRange(rawUsers.Where(o => o.Pics.IsEmpty()).OrderBy(o => random.Next()).ToList());
            ViewData["Users"] = BuildUserProfiles(users);
            return View();
        }

        private List<UserHomeModel> BuildUserProfiles(List<User> users)
        {
            var result = new List<UserHomeModel>();

            var profileService = this.GetService<ProfileService>();
            var userId = this.GetUserId();

            foreach(var item in users)
            {
                var index = users.IndexOf(item);

                bool isThumbsUpActive = true;
                bool isThumbsDownActive = true;
                bool isCommentActive = true;

                string logo = string.Empty;
                string thumbnail = string.Empty;

                int thumbsUp = 0;
                int thumbsDown = 0;
                int comment = 0;

                var profile = profileService.Get(item.Id);

                if(profile != null)
                {
                    if(!profile.ThumbsUpIds.IsEmpty())
                    {
                        thumbsUp = profile.ThumbsUpIds.Count;

                        if(profile.ThumbsUpIds.Contains(userId))
                        {
                            isThumbsUpActive = false;
                        }
                    }

                    if (!profile.ThumbsDownIds.IsEmpty())
                    {
                        thumbsDown = profile.ThumbsDownIds.Count;

                        if (profile.ThumbsDownIds.Contains(userId))
                        {
                            isThumbsDownActive = false;
                        }
                    }

                    comment = profile.Comments.IsEmpty() ? 0 : profile.Comments.SelectMany(o => o.Value).Count();
                }

                if (userId == null)
                {
                    isThumbsUpActive = false;
                    isThumbsDownActive = false;
                    isCommentActive = false;
                }

                if (item.Pics.IsEmpty())
                {
                    logo = item.Gender == Gender.Male ? string.Format("~/images/icon/male_{0}.jpg", index % 100) : string.Format("~/images/icon/female_{0}.jpg", index % 56);
                    thumbnail = logo;
                    isThumbsUpActive = false;
                    isThumbsDownActive = false;
                    isCommentActive = false;
                }
                else
                {
                    logo = "~/api/img/" + item.Pics.Last();
                    thumbnail = logo + "?height=200&width=200";
                }

                result.Add(new UserHomeModel(item.Id, item.Name, item.NickName, isThumbsUpActive, isThumbsDownActive, isCommentActive, logo, thumbnail, thumbsUp, thumbsDown, comment));
            }

            return result;
        }

        public IActionResult Error()
        {
            return View();
        }

        [Authorize(Roles = "0")]
        public IActionResult Help()
        {
            return Redirect("~/Home");
        }

        [Authorize(Roles = "0")]
        public IActionResult CleanTimeSheet()
        {
            var timeSheetService = this.GetService<TimeSheetService>();
            var projectService = this.GetService<ProjectService>();
            var timeSheets = timeSheetService.Get();

            foreach(var timeSheet in timeSheets)
            {
                var project = projectService.Get(timeSheet.ProjectId);

                if(!project.IsPublic && !project.Tasks.IsEmpty())
                {
                    var taskIds = project.Tasks.Where(o => o.UserId == timeSheet.UserId).Select(o => o.Id).ToList();

                    if(!timeSheet.WeekTimeSheets.IsEmpty())
                    {
                        foreach (var week in timeSheet.WeekTimeSheets)
                        {
                            var ids = new List<int>(week.Value.Keys);

                            foreach (var id in ids)
                            {
                                if (!taskIds.Contains(id))
                                {
                                    week.Value.Remove(id);
                                }
                            }
                        }

                        timeSheetService.Update(timeSheet);
                    }
                }
            }

            return Redirect("~/Home");
        }

        [Authorize(Roles = "0")]
        public IActionResult ResetUserTimeSheet()
        {
            var timeSheetService = this.GetService<TimeSheetService>();
            var userTimeSheetStatusService = this.GetService<UserTimeSheetStatusService>();
            var timeSheets = timeSheetService.Get();

            var userTimeSheets = userTimeSheetStatusService.Get();

            foreach(var userTimeSheet in userTimeSheets)
            {
                var timeSheet = timeSheets.Where(o => o.UserId == userTimeSheet.Id).ToList();

                var startDate = TimeSheetHelper.MIN_DATE;
                var currentMonday = DateTime.Today.GetMonday();

                while (startDate <= currentMonday)
                {
                    var monday = startDate.GetTimeSheetId();
                    var hour = timeSheet.Where(o => o.WeekTimeSheets.ContainsKey(monday)).Select(o => o.WeekTimeSheets[monday].Sum(p => p.Value.Sum())).Sum();

                    var status = Status.Pending;

                    if(hour >= 40)
                    {
                        status = Status.Done;
                    }
                    else if(hour > 0)
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

            return Redirect("~/Home");
        }

        [Authorize(Roles = "0")]
        public IActionResult UpdateProject()
        {
            var timeSheetService = this.GetService<TimeSheetService>();
            var projectService = this.GetService<ProjectService>();
            var projects = projectService.Get();

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

            return Redirect("~/Home");
        }

        [Authorize(Roles = "0")]
        public string ResetPublicProject()
        {
            var projectService = this.GetService<ProjectService>();
            var userService = this.GetService<UserService>();
            var projects = projectService.Get();
            var userIds = userService.Get().Select(o => o.Id).ToList();

            foreach (var project in projects)
            {
                if (project.IsPublic)
                {
                    project.ActualHours = project.ActualHours.Where(o => userIds.Contains(o.Key)).ToDictionary(o => o.Key, o => o.Value);
                    projectService.Update(project);
                }
            }

            return "done";
        }


        [Authorize(Roles = "0")]
        public IActionResult ResetTimeSheet()
        {
            var projectService = this.GetService<ProjectService>();
            var timeSheetService = this.GetService<TimeSheetService>();

            var projectIds = projectService.GetIds();

            foreach (var item in timeSheetService.Get())
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

            return Redirect("~/Home");
        }

        [Authorize(Roles = "0")]
        public IActionResult RefreshTimeSheet()
        {
            var timeSheetService = this.GetService<TimeSheetService>();

            foreach (var item in timeSheetService.Get())
            {
                timeSheetService.Update(item);
            }
            return Redirect("~/Home");
        }

        [Authorize(Roles = "0")]
        public string RefreshProjectTask()
        {
            var projects = this.GetService<ProjectService>().Get();

            foreach (var project in projects)
            {
                if (!project.Tasks.IsEmpty())
                {
                    foreach(var task in project.Tasks)
                    {
                        if(task.Values == null)
                        {
                            task.Values = new Dictionary<string, int>();

                            var ownerId = this.GetService<DepartmentService>().GetLeaderIdByUserId(task.UserId);

                            if(ownerId == null)
                            {
                                continue;
                            }

                            if (this.GetService<UserService>().Get(task.UserId).UserType == UserType.User)
                            {
                                var group = this.GetService<DepartmentService>().GetUserGroupsByUserId(task.UserId).First();

                                foreach (var userId in group.UserIds)
                                {
                                    task.Values.Add(userId, userId == ownerId ? (int)task.Value : 0);
                                }
                            }
                            else
                            {
                                task.Values.Add(ownerId, (int)task.Value);
                            }
                        }
                    }

                    this.GetService<ProjectService>().Update(project);
                }
            }

            return "Done";
        }

        [Authorize(Roles = "0")]
        public string RefactorTask()
        {
            var projects = this.GetService<ProjectService>().Get();

            foreach (var project in projects)
            {
                this.GetService<ProjectService>().Update(project);
            }

            return "Done";
        }

        [Authorize(Roles = "0")]
        public string AmendTask()
        {
            var projects = this.GetService<ProjectService>().Get();
            int projCount = 0;
            int taskCount = 0;
            foreach (var project in projects)
            {
                Boolean hasInvalidData = false;
                if (project.Tasks.IsEmpty())
                {
                    continue;
                }

                foreach (var task in project.Tasks)
                {
                    if(task.Values == null || task.Values.IsEmpty())
                    {
                        continue;
                    }

                    var result = new Dictionary<string, int>();
                    foreach (var val in task.Values)
                    {
                        if (val.Value == -1)
                        {
                            taskCount++;
                            hasInvalidData = true;
                            var point = this.GetService<UserService>().Get(val.Key).UserType == UserType.User ? 0 : (int)task.Value;
                            result.Add(val.Key, point);
                        }
                        else
                        {
                            result.Add(val.Key, val.Value);
                        }
                    }
                    task.Values = result;
                }

                if(hasInvalidData)
                {
                    projCount++;
                }
            }

            return string.Format("Total {0} projects and {1} tasks has invalid data.", projCount, taskCount);
        }

        private void ResetProfile()
        {
            var profileService = this.GetService<ProfileService>();
            var userService = this.GetService<UserService>();

            var profiles = profileService.Get();

            foreach (var item in profiles)
            {
                if (!item.ThumbsUpIds.IsEmpty())
                {
                    var thumbsUpIds = new HashSet<string>();
                    foreach (var thumbid in item.ThumbsUpIds)
                    {
                        var user = userService.Get(thumbid);
                        if (user != null)
                        {
                            thumbsUpIds.Add(thumbid);
                        }
                    }

                    profileService.Update(item.Id, "ThumbsUpIds", thumbsUpIds);
                }

                if (!item.ThumbsDownIds.IsEmpty())
                {
                    var thumbsDownIds = new HashSet<string>();
                    foreach (var thumbid in item.ThumbsDownIds)
                    {
                        var user = userService.Get(thumbid);
                        if (user != null)
                        {
                            thumbsDownIds.Add(thumbid);
                        }
                    }

                    profileService.Update(item.Id, "ThumbsDownIds", thumbsDownIds);
                }

                if (!item.Comments.IsEmpty())
                {
                    var comments = new Dictionary<string, List<KeyValuePair<string, DateTime>>>();

                    foreach (var entry in item.Comments)
                    {
                        comments.Add(entry.Key, new List<KeyValuePair<string, DateTime>>());

                        foreach (var comment in entry.Value)
                        {
                            if (comment.Key != "undefined")
                            {
                                comments[entry.Key].Add(comment);
                            }
                        }
                    }
                    profileService.Update(item.Id, "Comments", comments);
                }
            }
        }
    }
}
