namespace Service.Project
{
    using Entity.Project;
    using Entity.ValueType;
    using Microsoft.Extensions.Logging;
    using Platform.Context;
    using Platform.Extension;
    using Platform.Util;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using User;

    public class TimeSheetService : BaseService<TimeSheet>
    {
        private static ILogger _logger = LoggerUtil.CreateLogger<TimeSheetService>();

        public TimeSheetService(IContextRepository contextRepository) : base(contextRepository) { }

        public void DeleteTasks(string projectId, List<ProjectTask> tasks)
        {
            foreach(var task in tasks)
            {
                var timeSheet = Get(projectId + "_" + task.UserId);
                
                if(timeSheet != null && !timeSheet.WeekTimeSheets.IsEmpty())
                {
                    foreach(var item in timeSheet.WeekTimeSheets)
                    {
                        if (item.Value.ContainsKey(task.Id))
                        {
                            item.Value.Remove(task.Id);
                        }
                    }
                    base.Update(timeSheet);
                }
            }
        }

        //key为projectId
        public Dictionary<string, Dictionary<int, double[]>> GetWeekTimeSheetByUserId(string userId, string weekId)
        {
            var result = new Dictionary<string, Dictionary<int, double[]>>();
            var timeSheets = Get(o => o.UserId == userId);

            foreach(var item in timeSheets)
            {
                var project = ServiceFactory.Instance.GetService<ProjectService>().Get(item.ProjectId);

                if(project == null)
                {
                    _logger.LogWarning(string.Format("timeSheetId:{0} have some problem, please check it", item.Id));
                    continue;
                }

                if (item.WeekTimeSheets != null)
                {
                    if (item.WeekTimeSheets.ContainsKey(weekId))
                    {
                        result.Add(item.ProjectId, item.WeekTimeSheets[weekId]);
                    }
                }
            }

            return result;
        }

        //key:ProjectId value:总时间数
        //获取用户在某一时间段内花费在项目上的总时间数，只返回有数据的项目
        public Dictionary<string, double> GetUserHoursByProjectId(string userId, DateTime startDate, DateTime endDate)
        {
            Dictionary<string, double> result = new Dictionary<string, double>();

            var timeSheets = Get(o => o.UserId == userId);

            foreach(var item in timeSheets)
            {
                var hours = item.GetHours(startDate, endDate);
                if (hours > 0)
                {
                    result.Add(item.ProjectId, hours);
                }
            }

            return result;
        }

        //key为日期,value为key上所花费的时间
        //获取用户在某个时间段内实际花费的时间总和
        public Dictionary<DateTime, double> GetUserHoursByDate(string userId, DateTime startDate, DateTime endDate)
        {
            Dictionary<DateTime, double> result = new Dictionary<DateTime, double>();

            var firstDate = new DateTime(startDate.Ticks);

            var timeSheets = Get(o => o.UserId == userId);

            while (firstDate <= endDate)
            {
                result.Add(firstDate, timeSheets.Sum(o => o.GetHour(firstDate)));
                firstDate = firstDate.AddDays(1);
            }

            return result;
        }

        //key为日期,value为key上所花费的时间
        //获取用户在某个时间段内实际花费的时间总和，除去公共项目
        public Dictionary<DateTime, double> GetUserHoursWithoutPublicByDate(string userId, DateTime startDate, DateTime endDate)
        {
            Dictionary<DateTime, double> result = new Dictionary<DateTime, double>();

            var firstDate = new DateTime(startDate.Ticks);

            var timeSheets = Get(o => o.UserId == userId).Where(o => !IsPublic(o.ProjectId)).ToList();

            while (firstDate <= endDate)
            {
                result.Add(firstDate, timeSheets.Sum(o => o.GetHour(firstDate)));
                firstDate = firstDate.AddDays(1);
            }

            return result;
        }

        private bool IsPublic(string projectId)
        {
            var project = ServiceFactory.Instance.GetService<ProjectService>().Get(projectId);
            return project.IsPublic;
        }

        //获取用户组在某个时间段内工作的项目
        public List<string> GetWorkingProjectIds(IEnumerable<string> userIds, DateTime startDate, DateTime endDate)
        {
            List<string> result = new List<string>();

            foreach(var item in userIds)
            {
                result.AddRange(GetUserHoursByProjectId(item, startDate, endDate).Keys);
            }

            return result.Distinct().ToList();
        }

        //key为UserId
        //获取用户在某个项目上的时间分配，只返回有数据的用户
        public Dictionary<string, double> GetProjectHoursByUserId(string projectId)
        {
            Dictionary<string, double> result = new Dictionary<string, double>();

            var timeSheets = Get(o => o.ProjectId == projectId);

            foreach (var item in timeSheets)
            {
                if(item.GetTotalHours() > 0)
                {
                    result.Add(item.UserId, item.GetTotalHours());
                }
            }

            return result;
        }

        //key为UserId
        //获取用户在某个时间段在某个项目上的时间分配，只返回有数据的用户
        public Dictionary<string, double> GetProjectHoursByUserId(string projectId, DateTime startDate, DateTime endDate)
        {
            Dictionary<string, double> result = new Dictionary<string, double>();

            var timeSheets = Get(o => o.ProjectId == projectId);

            foreach (var item in timeSheets)
            {
                if(item.GetHours(startDate, endDate) > 0)
                {
                    result.Add(item.UserId, item.GetHours(startDate, endDate));
                }
            }

            return result;
        }

        //遍历项目timesheet，找到对应task周期内实际值
        public double GetTaskActualHour(string projectId, int taskId, DateTime startDate, DateTime endDate)
        {
            double ret = 0;
            var timeSheets = Get(o => o.ProjectId == projectId);
            
            foreach (var item in timeSheets)
            {
                var weeks = item.WeekTimeSheets.Where(o => o.Value.ContainsKey(taskId) && o.Value[taskId].Sum() > 0).OrderBy(o => o.Key).ToList();
                if(weeks.Count > 0)
                {
                    foreach(var week in weeks)
                    {
                        var monday = DateTime.Parse(week.Key);
                        var sunday = monday.AddDays(6);
                        if(week.Value.ContainsKey(taskId) && DateTimeUtil.Max(monday, startDate).CompareTo(DateTimeUtil.Min(sunday, endDate)) <= 0)
                        {
                            var start = (int)DateTimeUtil.Max(startDate, monday).Subtract(monday).TotalDays;
                            var end = (int)DateTimeUtil.Min(endDate, sunday).Subtract(monday).TotalDays;
                            for(var i=start; i<end; i++)
                            {
                                ret += week.Value[taskId][i];
                            }
                        }
                    }
                }
            }
            return ret;
        }

        //key为[DEV],[TEST]
        //获取用户在某个时间段在某个项目上的时间分配，只返回有数据的用户
        public Dictionary<string, DateRange> GetActualWorkingRangesByUserType(string projectId, DateTime startDate, DateTime endDate)
        {
            var deparmentService = ServiceFactory.Instance.GetService<DepartmentService>();
            Dictionary<string, DateRange> result = new Dictionary<string, DateRange>();
            DateRange devDateRange = new DateRange(DateTime.MinValue, DateTime.MinValue);
            DateRange testDateRange = new DateRange(DateTime.MinValue, DateTime.MinValue);
            var timeSheets = Get(o => o.ProjectId == projectId);
            foreach (var item in timeSheets)
            {
                DateTime earliestDate = item.GetEarliestDate();
                DateTime lastestDate = item.GetLatestDate();
                if (deparmentService.IsTester(item.UserId))
                {
                    if (testDateRange.StartDate.IsEmpty() || (!earliestDate.IsEmpty() && testDateRange.StartDate > earliestDate))
                    {
                        testDateRange.StartDate = earliestDate;
                    }
                    if (testDateRange.EndDate.IsEmpty() || testDateRange.EndDate < lastestDate)
                    {
                        testDateRange.EndDate = lastestDate;
                    }
                }
                else
                {
                    if (devDateRange.StartDate.IsEmpty() || (!earliestDate.IsEmpty() && devDateRange.StartDate > earliestDate))
                    {
                        devDateRange.StartDate = earliestDate;
                    }
                    if (devDateRange.EndDate.IsEmpty() || devDateRange.EndDate < lastestDate)
                    {
                        devDateRange.EndDate = lastestDate;
                    }
                }
            }
            result.Add("DEV", devDateRange);
            result.Add("TEST", testDateRange);

            return result;
        }

        //key为日期,value为key上所花费的时间
        public Dictionary<DateTime, double> GetProjectHoursByDate(string projectId, DateTime startDate, DateTime endDate)
        {
            Dictionary<DateTime, double> result = new Dictionary<DateTime, double>();

            var timeSheets = Get(o => o.ProjectId == projectId);

            var firstDate = new DateTime(startDate.Ticks);

            while (firstDate <= endDate)
            {
                result.Add(firstDate, timeSheets.Select(o => o.GetHour(firstDate)).Sum());
                firstDate = firstDate.AddDays(1);
            }

            return result;
        }

        //key为日期,value为key上所花费的时间
        public Dictionary<DateTime, double> GetRemainingProjectHoursByDate(string projectId, double total, DateTime startDate, DateTime endDate)
        {
            Dictionary<DateTime, double> result = new Dictionary<DateTime, double>();

            var timeSheets = Get(o => o.ProjectId == projectId);

            var firstDate = new DateTime(startDate.Ticks);

            while (firstDate <= endDate)
            {
                total -= timeSheets.Select(o => o.GetHour(firstDate)).Sum();
                result.Add(firstDate, total);
                firstDate = firstDate.AddDays(1);
            }

            return result;
        }
    }
}
