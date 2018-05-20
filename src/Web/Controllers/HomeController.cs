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
    using Entity.Project;
    using Web.Util;

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
            TimeSheetUtil.ResetUserTimeSheet(
                this.GetService<TimeSheetService>(),
                this.GetService<UserTimeSheetStatusService>());

            return Redirect("~/Home");
        }

        [Authorize(Roles = "0")]
        public IActionResult UpdateProject()
        {
            TimeSheetUtil.UpdateProject(
                this.GetService<TimeSheetService>(),
                this.GetService<ProjectService>());

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
            TimeSheetUtil.ResetTimeSheet(this.GetService<ProjectService>(), this.GetService<TimeSheetService>());

            return Redirect("~/Home");
        }

        [Authorize(Roles = "0")]
        public string RefreshProjectTask()
        {
            return TimeSheetUtil.RefreshProjectTask(
                this.GetService<ProjectService>(),
                this.GetService<DepartmentService>(),
                this.GetService<UserService>());
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
        public string AddSilverProjects()
        {
            var devOwnerId = "G10432";
            var group = this.GetService<DepartmentService>().GetUserGroupsByUserId(devOwnerId).First();
            List<string> ownerIds = new List<string>(new string[] { devOwnerId, "G10477" });
            List<string> userIds = new List<string>(new string[] { devOwnerId });
            DateTime startDate = new DateTime(2018, 12, 11);
            DateTime endDate = startDate.AddDays(7);
            string projectIds = string.Empty;
            string projExits = string.Empty;
            for (int id=61; id<69; id++)
            {
                string sn = "Silver-Sprint" + id.ToString();
                var proj = this.GetService<ProjectService>().Get().Where(o => string.Equals(o.SerialNumber,sn, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (proj != null)
                {
                    projExits += proj.Id + "\r\n";
                    continue;
                }

                Project project = new Project();
                project.Name = "Direct Booking Silver Sprint" + id.ToString();
                project.Description = project.Name;
                project.SerialNumber = sn;
                project.OwnerIds = ownerIds;
                project.UserIds = userIds;
                project.Status = Status.Pending;
                project.ProjectManagerName = "乐文焱";

                //init task data
                ProjectTask task = new ProjectTask();
                task.Id = 0;
                task.Name = "review";
                task.Phase = Phase.Development;
                task.PlanDateRange = new DateRange(startDate, endDate);
                task.Status = Status.Pending;
                task.UserId = "G10432";
                task.PlanHour = 20;
                task.Values = new Dictionary<string, int>() { };
                foreach (var userId in group.UserIds)
                {
                    task.Values.Add(userId, userId == devOwnerId ? 20 : 0);
                }

                project.Tasks = new List<ProjectTask>();
                project.Tasks.Add(task);

                project.PlanDateRange = task.PlanDateRange;
                project.PublishDate = endDate;

                projectIds += this.GetService<ProjectService>().Create(project) + "\r\n";

                startDate = startDate.AddDays(7);
                endDate = endDate.AddDays(7);
            }


            return (projectIds.IsEmpty() ? "Create project failed." : "Create project(" + projectIds +")") + 
                (projExits.IsEmpty()?"": "Projects already exists (" + projExits + ")");
        }

        [Authorize(Roles = "0")]
        public string AddAquaProjects()
        {
            var devOwnerId = "G10488";
            var group = this.GetService<DepartmentService>().GetUserGroupsByUserId(devOwnerId).First();
            List<string> ownerIds = new List<string>(new string[] { devOwnerId, "G10402","G10601" });
            List<string> userIds = new List<string>(new string[] { devOwnerId });
            DateTime startDate = new DateTime(2018, 11, 19);
            DateTime endDate = startDate.AddDays(4);
            string projectIds = string.Empty;
            string projExits = string.Empty;
            for (int id = 61; id < 71; id++)
            {
                string sn = "Aqua-Sprint" + id.ToString();
                var proj = this.GetService<ProjectService>().Get().Where(o => string.Equals(o.SerialNumber, sn, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (proj != null)
                {
                    projExits += proj.Id + "\r\n";
                    continue;
                }

                Project project = new Project();
                project.Name = "Direct Booking Aqua Sprint " + id.ToString();
                project.Description = project.Name;
                project.SerialNumber = sn;
                project.OwnerIds = ownerIds;
                project.UserIds = userIds;
                project.Status = Status.Pending;
                project.Level = ProjectLevel.Medium;
                project.ProjectManagerName = "冯冰";

                //init task data
                ProjectTask task = new ProjectTask();
                task.Id = 0;
                task.Name = "Code Review";
                task.Phase = Phase.Development;
                task.PlanDateRange = new DateRange(startDate, endDate);
                task.Status = Status.Pending;
                task.UserId = devOwnerId;
                task.PlanHour = 20;
                task.Values = new Dictionary<string, int>() { };
                foreach (var userId in group.UserIds)
                {
                    task.Values.Add(userId, userId == devOwnerId ? 20 : 0);
                }

                project.Tasks = new List<ProjectTask>();
                project.Tasks.Add(task);

                project.PlanDateRange = task.PlanDateRange;
                project.PublishDate = endDate;

                projectIds += this.GetService<ProjectService>().Create(project) + "\r\n";

                startDate = startDate.AddDays(7);
                endDate = endDate.AddDays(7);
            }


            return (projectIds.IsEmpty() ? "Create project failed." : "Create project(" + projectIds + ")") +
                (projExits.IsEmpty() ? "" : "Projects already exists (" + projExits + ")");
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
                            var point = this.GetService<UserService>().Get(val.Key).UserType == UserType.User ? 0 : (int)task.CalculateValue();
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
                    this.GetService<ProjectService>().Update(project);
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
