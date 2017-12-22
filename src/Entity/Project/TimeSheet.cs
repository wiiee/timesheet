namespace Entity.Project
{
    using Platform.Extension;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class TimeSheet : BaseEntity
    {
        //Id为[ProjectId]_[UserId]

        public string ProjectId { get; set; }
        public string UserId { get; set; }

        //string为周一的日期字符串,值为每个Task的时间
        public Dictionary<string, Dictionary<int, double[]>> WeekTimeSheets { get; set; }

        public TimeSheet(string projectId, string userId)
        {
            this.Id = projectId + "_" + userId;
            this.ProjectId = projectId;
            this.UserId = userId;
        }

        public void AddWeek(string monday, string projectId, string userId, int taskId, double[] week)
        {
            if (WeekTimeSheets == null)
            {
                WeekTimeSheets = new Dictionary<string, Dictionary<int, double[]>>();
            }

            if (WeekTimeSheets.ContainsKey(monday))
            {
                if (WeekTimeSheets[monday].ContainsKey(taskId))
                {
                    WeekTimeSheets[monday][taskId] = week;
                }
                else
                {
                    WeekTimeSheets[monday].Add(taskId, week);
                }
            }
            else
            {
                Dictionary<int, double[]> weeks = new Dictionary<int, double[]>
                {
                    { taskId, week }
                };
                WeekTimeSheets.Add(monday, weeks);
            }
        }

        public double GetTotalHours()
        {
            if (!WeekTimeSheets.IsEmpty())
            {
                return WeekTimeSheets.Select(o => o.Value.Select(p => p.Value.Sum()).Sum()).Sum();
            }

            return 0;            
        }

        //用户在该段期间上所花的时间总数
        public double GetHours(DateTime startDate, DateTime endDate)
        {
            if(WeekTimeSheets.IsEmpty())
            {
                return 0;
            }
            double result = 0;
            var firstMonday = startDate.GetMonday();
            var lastMonday = endDate.GetMonday();
            var monday = new DateTime(firstMonday.AddDays(7).Ticks);

            while (monday < lastMonday)
            {
                if (WeekTimeSheets.ContainsKey(monday.GetTimeSheetId()))
                {
                    Dictionary<int, double[]> tasks = WeekTimeSheets[monday.GetTimeSheetId()];
                    result += tasks.Sum(o => o.Value.Sum());
                }
                
                monday = monday.AddDays(7);
            }

            //获取起始星期的数据
            if (WeekTimeSheets.ContainsKey(firstMonday.GetTimeSheetId()))
            {
                var dayOfWeek = (int)startDate.DayOfWeek;
                var beginning = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
                Dictionary<int, double[]> tasks = WeekTimeSheets[firstMonday.GetTimeSheetId()];
                foreach (var task in tasks)
                {
                    for (int i = beginning; i < 7; i++)
                    {
                        result += task.Value[i];
                    }
                }
            }

            //获取最后一个星期的数据，如果起始星期和最后一个星期一样，就不计算
            if (firstMonday < lastMonday && WeekTimeSheets.ContainsKey(lastMonday.GetTimeSheetId()))
            {
                var dayOfWeek = (int)startDate.DayOfWeek;
                var end = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
                Dictionary<int, double[]> tasks = WeekTimeSheets[lastMonday.GetTimeSheetId()];
                foreach (var task in tasks)
                {
                    for (int i = 0; i <= end; i++)
                    {
                        result += task.Value[i];
                    }
                }
            }
            

            return result;
        }

        public double GetHour(DateTime date)
        {
            var monday = date.GetMonday().GetTimeSheetId();
            var index = date.GetDayIndex();

            if (WeekTimeSheets.ContainsKey(monday))
            {
                double result = 0;
                Dictionary<int, double[]> tasks = WeekTimeSheets[monday];
                foreach(var task in tasks)
                {
                    result += task.Value[index];
                }
                return result;
            }

            
            return 0;
        }

        public double GetTaskHours(int taskId)
        {
            return WeekTimeSheets.Where(o => o.Value.ContainsKey(taskId) && o.Value[taskId].Sum() > 0).SelectMany(o => o.Value).
                Where(o => o.Key == taskId).Sum(o => o.Value.Sum());
        }

        public DateTime GetTaskStartDate(int taskId)
        {
            var weeks = WeekTimeSheets.ToList().Where(o => o.Value.ContainsKey(taskId) && o.Value[taskId].Sum() > 0).OrderBy(o => o.Key).ToList();

            if (weeks.Count == 0)
            {
                return DateTime.MinValue;
            }
            else
            {
                var week = weeks.First();
                var monday = Convert.ToDateTime(week.Key);
                return monday.AddDays(Array.FindIndex(week.Value[taskId], o => o > 0));
            }
        }

        public DateTime GetTaskEndDate(int taskId)
        {
            var weeks = WeekTimeSheets.ToList().Where(o => o.Value.ContainsKey(taskId) && o.Value[taskId].Sum() > 0).OrderByDescending(o => o.Key).ToList();

            if (weeks.Count == 0)
            {
                return DateTime.MinValue;
            }
            else
            {
                var week = weeks.First();
                var monday = Convert.ToDateTime(week.Key);
                return monday.AddDays(6 - Array.FindIndex(week.Value[taskId].Reverse().ToArray(), o => o > 0));
            }
        }

        public DateTime GetEarliestDate()
        {
            if (!WeekTimeSheets.IsEmpty())
            {
                var monday = WeekTimeSheets.Select(o => new { Key = o.Key, Value = o.Value.Sum(oo => oo.Value.Sum()) }).Where(o => o.Value > 0).Select(o => o.Key).Min();

                if (!monday.IsEmpty())
                {
                    Dictionary<int, double[]> tasks = WeekTimeSheets[monday];
                    foreach( var task in tasks)
                    {
                        for (int i = 0; i < 7; i++)
                        {
                            if (task.Value[i] > 0)
                            {
                                return Convert.ToDateTime(monday).AddDays(i);
                            }
                        }
                    }   
                }
            }

            return DateTime.MinValue;
        }

        public DateTime GetLatestDate()
        {
            if(WeekTimeSheets.IsEmpty())
            {
                return DateTime.MinValue;
            }

            var dates = WeekTimeSheets.Select(o => new { Key = o.Key, Value = o.Value.Sum(p => p.Value.Sum()) }).Where(o => o.Value > 0).Select(o => Convert.ToDateTime(o.Key)).ToList();
            if (dates.IsEmpty())
            {
                return DateTime.MinValue;
            }

            var monday = dates.Max();
            Dictionary<int, double[]> tasks = WeekTimeSheets[monday.GetTimeSheetId()];
            foreach (var task in tasks)
            {
                for (int i = 6; i >= 0; i--)
                {
                    if (task.Value[i] > 0)
                    {
                        return monday.AddDays(i);
                    }
                }
            }

            return DateTime.MinValue;
        }

        public void DeleteTask(int taskId)
        {
            foreach (var week in WeekTimeSheets)
            {
                foreach (var id in week.Value.Keys)
                {
                    if (id == taskId)
                    {
                        week.Value.Remove(taskId);
                    }
                }
            }
        }

        public void DeleteTasks(List<int> taskIds)
        {
            foreach (var week in WeekTimeSheets)
            {
                var ids = new List<int>(week.Value.Keys);
                foreach (var id in ids)
                {
                    if (taskIds.Contains(id))
                    {
                        week.Value.Remove(id);
                    }
                }
            }
        }
    }
}
