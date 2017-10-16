namespace Web.Extension
{
    using Entity.Project;
    using Entity.ValueType;
    using Model;
    using Model.Chart.Line;
    using Model.Report;
    using Platform.Enum;
    using Platform.Extension;
    using Platform.Util;
    using Service;
    using Service.Project;
    using Service.User;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class ProjectExtension
    {
        public static Dictionary<string, HourItem> BuildHours(this Project project, DateTime startDate, DateTime endDate)
        {
            Dictionary<string, HourItem> result = new Dictionary<string, HourItem>();
            var userService = ServiceFactory.Instance.GetService<UserService>();
            var timeSheetService = ServiceFactory.Instance.GetService<TimeSheetService>(); ;

            var searchHours = timeSheetService.GetProjectHoursByUserId(project.Id, startDate, endDate);

            if (project.IsPublic)
            {
                if (project.ActualHours != null)
                {
                    return project.ActualHours.ToDictionary(pair => userService.Get(pair.Key).Name, pair => new HourItem(0, Math.Round(pair.Value, 2),
                        searchHours.ContainsKey(pair.Key) ? Math.Round(searchHours[pair.Key], 2) : 0));
                }
                else
                {
                    return new Dictionary<string, HourItem>();
                }
            }

            foreach (var item in project.UserIds)
            {
                double actual = 0;
                double plan = 0;
                double search = 0;

                if (project.PlanHours != null && project.PlanHours.ContainsKey(item))
                {
                    plan = project.PlanHours[item];
                }

                if (project.ActualHours != null && project.ActualHours.ContainsKey(item))
                {
                    actual = project.ActualHours[item];
                }

                if (searchHours.ContainsKey(item))
                {
                    search = searchHours[item];
                }

                result.Add(userService.Get(item).Name, new HourItem(Math.Round(plan, 2), Math.Round(actual, 2), Math.Round(search, 2)));
            }

            return result;
        }

        public static Dictionary<string, DateRange> GetActualWorkingRanges(this Project project, DateTime endDate)
        {
            if (project.Tasks.IsEmpty())
            {
                var timeSheetService = ServiceFactory.Instance.GetService<TimeSheetService>();
                return timeSheetService.GetActualWorkingRangesByUserType(project.Id, project.ActualDateRange.StartDate, endDate);
            }
            else
            {
                Dictionary <string, DateRange> result = new Dictionary<string, DateRange>();
                result.Add("DEV", new DateRange(DateTime.MinValue, DateTime.MinValue));
                result.Add("TEST", new DateRange(DateTime.MinValue, DateTime.MinValue));

                var devStartDates = project.Tasks.Where(o => o.Phase != Phase.Test && o.ActualDateRange != null && !o.ActualDateRange.StartDate.IsEmpty()).Select(o => o.ActualDateRange.StartDate).ToList();
                var devEndDates = project.Tasks.Where(o => o.Phase != Phase.Test && o.ActualDateRange != null && !o.ActualDateRange.EndDate.IsEmpty()).Select(o => o.ActualDateRange.EndDate).ToList();
                if(devStartDates.Count > 0)
                {
                    result["DEV"].StartDate = devStartDates.Min();
                }
                if(devEndDates.Count > 0)
                {
                    result["DEV"].EndDate = devEndDates.Max();
                }
                
                var testStartDates = project.Tasks.Where(o => o.Phase == Phase.Test && o.ActualDateRange != null && !o.ActualDateRange.StartDate.IsEmpty()).Select(o => o.ActualDateRange.StartDate).ToList();
                var testEndDates = project.Tasks.Where(o => o.Phase == Phase.Test && o.ActualDateRange != null && !o.ActualDateRange.EndDate.IsEmpty()).Select(o => o.ActualDateRange.EndDate).ToList();
                if (testStartDates.Count > 0)
                {
                    result["TEST"].StartDate = testStartDates.Min();
                }
                if (testEndDates.Count > 0)
                {
                    result["TEST"].EndDate = testEndDates.Max();
                }
                return result;
            }
        }

        public static double GetPlanDevHour(this Project project)
        {
            var deparmentService = ServiceFactory.Instance.GetService<DepartmentService>();

            if (project.PlanHours.IsEmpty())
            {
                return 0;
            }

            if (project.Tasks.IsEmpty())
            {
                return project.PlanHours.Where(o => !deparmentService.IsTester(o.Key)).Sum(o => o.Value);
            }
            else
            {
                return project.Tasks.Where(o => o.Phase < Phase.Test).ToList().Sum(o => o.PlanHour);
            }
        }

        public static double GetPlanTestHour(this Project project)
        {
            var deparmentService = ServiceFactory.Instance.GetService<DepartmentService>();

            if (project.PlanHours.IsEmpty())
            {
                return 0;
            }
            if (project.Tasks.IsEmpty())
            {
                return project.PlanHours.Where(o => deparmentService.IsTester(o.Key)).Sum(o => o.Value);
            }
            else
            {
                return project.Tasks.Where(o => o.Phase >= Phase.Test).ToList().Sum(o => o.PlanHour);
            }
        }

        public static DateTime GetPlanCombinedTestDate(this Project project)
        {
            var deparmentService = ServiceFactory.Instance.GetService<DepartmentService>();

            if (project.PlanHours.IsEmpty())
            {
                return DateTime.MinValue;
            }
            if (project.Tasks.IsEmpty())
            {
                return project.PlanTestDate;
            }
            else
            {
                ProjectTask task = project.Tasks.Where(o => o.Phase == Phase.Test).FirstOrDefault();
                return task != null && task.PlanDateRange != null ? task.PlanDateRange.StartDate : DateTime.MinValue;
            }
        }

        public static DateTime GetActualCombinedTestDate(this Project project)
        {
            var deparmentService = ServiceFactory.Instance.GetService<DepartmentService>();

            if (project.PlanHours.IsEmpty())
            {
                return DateTime.MinValue;
            }
            if (project.Tasks.IsEmpty())
            {
                return project.ActualTestDate;
            }
            else
            {
                ProjectTask task = project.Tasks.Where(o => o.Phase == Phase.Test).FirstOrDefault();
                return task != null && task.ActualDateRange != null ? task.ActualDateRange.StartDate : DateTime.MinValue;
            }
        }

        public static double GetActualDevHour(this Project project)
        {
            var deparmentService = ServiceFactory.Instance.GetService<DepartmentService>();

            if (project.ActualHours.IsEmpty())
            {
                return 0;
            }

            if(project.Tasks.IsEmpty())
            {
                return project.ActualHours.Where(o => !deparmentService.IsTester(o.Key)).Sum(o => o.Value);
            }
            else
            {
                return project.Tasks.Where(o => o.Phase < Phase.Test).ToList().Sum(o => o.ActualHour);
            }
        }

        public static double GetActualTestHour(this Project project)
        {
            var deparmentService = ServiceFactory.Instance.GetService<DepartmentService>();

            if (project.ActualHours.IsEmpty())
            {
                return 0;
            }

            if (project.Tasks.IsEmpty())
            {
                return project.ActualHours.Where(o => deparmentService.IsTester(o.Key)).Sum(o => o.Value);
            }
            else
            {
                return project.Tasks.Where(o => o.Phase >= Phase.Test).ToList().Sum(o=>o.ActualHour);
            }
        }

        public static double GetDevPercentage(this Project project)
        {
            if(project.Status == Status.Done)
            {
                return 1D;
            }
            if (project.Tasks.Any(o => o.Status != Status.Done && o.Phase < Phase.Test))
            {
                double planHours = project.Tasks.Where(o => o.Phase < Phase.Test).ToList().Sum(o => o.PlanHour);
                double actualHours = project.Tasks.Where(o => o.Phase < Phase.Test).ToList().Sum(o => o.ActualHour);
                double percentage = planHours == 0D ? 1 : actualHours / planHours;
                return percentage > 1D ? 1D : percentage;
            }
            return 1D;
        }

        public static double GetTestPercentage(this Project project)
        {
            if (project.Status == Status.Done)
            {
                return 1D;
            }
            if (project.Tasks.Any(o => o.Status != Status.Done && o.Phase >= Phase.Test))
            {
                double planHours = project.Tasks.Where(o => o.Phase >= Phase.Test).ToList().Sum(o => o.PlanHour);
                double actualHours = project.Tasks.Where(o => o.Phase >= Phase.Test).ToList().Sum(o => o.ActualHour);
                double percentage =  planHours == 0D ? 1 : actualHours / planHours;
                return percentage > 1D ? 1D : percentage;
            }
            return 1.0;
        }

        public static LineModel BuildLineModel(this Project project)
        {
            var timeSheetService = ServiceFactory.Instance.GetService<TimeSheetService>();
            var endDate = DateTime.Today < project.GetEndDate() ? DateTime.Today : project.GetEndDate();

            var hours = timeSheetService.GetRemainingProjectHoursByDate(project.Id, project.GetTotalPlanHour(), project.PlanDateRange.StartDate, endDate);

            var names = hours.Keys.Select(o => o.ToString("MM-dd")).ToList();

            List<LineItem> items = new List<LineItem>();
            items.Add(new LineItem(project.Name, hours.Values.ToList()));

            return new LineModel(names, items);
        }

        public static ProjectModel BuildProjectModel(this Project project, string userId)
        {
            var userService = ServiceFactory.Instance.GetService<UserService>();
            var departmentService = ServiceFactory.Instance.GetService<DepartmentService>();

            var isEdit = false;
            var isDelete = false;
            var isClose = false;
            var isPostpone = false;

            var user = userService.Get(userId);

            if(user.UserType == UserType.Admin)
            {
                isEdit = true;
                isDelete = true;
                isClose = true;
            }
            else if(user.UserType != UserType.User)
            {
                if(!project.IsPublic)
                {
                    isEdit = true;
                    isDelete = true;

                    if(project.Status != Status.Done)
                    {
                        isClose = true;
                    }
                }
            }
            else
            {
                if (ServiceFactory.Instance.GetService<DepartmentService>().IsBoss(project.OwnerIds, user.Id))
                {
                    isEdit = true;
                    isClose = true;
                }
            }

            //只有Owner才可以postpone项目
            if (!project.IsPublic
                && !(project.Status == Status.Done && (DateTime.Today - project.ActualDateRange.EndDate).Days > 7)
                && departmentService.IsBoss(user.Id, project.OwnerIds))
            {
                isPostpone = true;
            }

            string departmentNames = "Public";

            if (!project.IsPublic)
            {
                departmentNames = string.Join(", ", departmentService.GetDepartmentsByUserIds(project.OwnerIds).Select(o => o.Id).ToList());
            }

            return new ProjectModel(project, departmentNames, string.Join(",", userService.GetByIds(project.OwnerIds).Select(o => o.Name)), string.Join(",", userService.GetByIds(project.UserIds).Select(o => o.Name)), project.GetTotalPlanHour(), project.GetTotalActualHour(), isEdit, isDelete, isClose, isPostpone);
        }

        //获取某个周期内有贡献的用户Id
        public static List<string> GetContributionUserIds(this Project project, DateTime startDate, DateTime endDate)
        {
            List<string> userIds = new List<string>();
            foreach (var task in project.Tasks)
            {
                if (IsTaskInProcess(task, startDate, endDate))
                {
                    userIds.Add(task.UserId);
                }
            }
            return userIds.Distinct().ToList();
        }

        //满足max(A.start,B.start)<=min(A.end,B,end)，即可判断A，B重叠
        private static bool IsTaskInProcess(ProjectTask task, DateTime startDate, DateTime endDate)
        {
            if (task.Status != Status.Pending)
            {
                DateTime taskEndDate = task.Status == Status.Done ? task.ActualDateRange.EndDate : DateTime.MaxValue;
                return (DateTimeUtil.Max(task.ActualDateRange.StartDate, startDate).CompareTo(DateTimeUtil.Min(taskEndDate, endDate)) <= 0);
            }
            return false;
        }

        public static string GetContributionInfo(this Project project, DateTime startDate, DateTime endDate)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            foreach (var task in project.Tasks)
            {
                if (IsTaskInProcess(task, startDate, endDate))
                {
                    string val = null;
                    if (dic.TryGetValue(task.UserId, out val))
                    {
                        val += ";" + task.Name;
                        dic.Remove(task.UserId);
                        dic.Add(task.UserId, val);
                    }
                    else
                    {
                        dic.Add(task.UserId, task.Name);
                    }
                }
            }
            string ret = string.Empty;
            var userService = ServiceFactory.Instance.GetService<UserService>();
            foreach (var item in dic)
            {
                ret += userService.Get(item.Key).Name + ":" + item.Value + "\r\n";
            }

            return ret;
        }

    }
}
