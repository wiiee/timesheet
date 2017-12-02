namespace Entity.Project
{
    using Platform.Extension;
    using Platform.Enum;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ValueType;
    using Platform.Util;
    using Platform.Setting;
    using Platform.Constant;

    public class Project : BaseEntity
    {
        //Id为[Owner]_[Name]

        public string Name { get; set; }
        public string Description { get; set; }
        public string SerialNumber { get; set; } //cp4 project number
        public string ProjectManagerName { get; set; }

        //计划开始结束日期
        public DateRange PlanDateRange { get; set; }
        //实际开始结束日期
        public DateRange ActualDateRange { get; set; }

        //原始结束日期（发布日期）
        public DateTime PlanEndDate { get; set; }

        //发布日期
        public DateTime PublishDate { get; set; }

        //计划时间
        public Dictionary<string, double> PlanHours { get; set; }
        //实际时间
        public Dictionary<string, double> ActualHours { get; set; }

        public string Comment { get; set; }

        public bool IsPublic { get; set; }

        public Status Status { get; set; }

        public ProjectLevel Level { get; set; }

        //项目是否被Review过
        public bool IsReviewed { get; set; }

        //父项目的Id
        public string GroupId { get; set; }

        //是否bug fix或者Cr
        public bool IsCr { get; set; }

        public List<string> OwnerIds { get; set; }

        //用户
        public List<string> UserIds { get; set; }

        public List<ProjectTask> Tasks { get; set; }

        //推迟项目的原因
        public List<PostponeReason> PostponeReasons { get; set; }

        //public Dictionary<FieldName, object> OtherFields { get; set; }

        //计划送测开始结束日期
        public DateTime PlanTestDate { get; set; }
        //实际测试开始结束日期
        public DateTime ActualTestDate { get; set; }

        //key为时间的ticks, value为<用户Id, 内容>
        public Dictionary<long, KeyValuePair<string, string>> Murmurs { get; set; }

        public Project() {  }

        public double GetTotalPlanHour()
        {
            if (!PlanHours.IsEmpty())
            {
                return PlanHours.Sum(o => o.Value);
            }

            return 0;
        }

        public double GetTotalActualHour()
        {
            if (!ActualHours.IsEmpty())
            {
                return ActualHours.Sum(o => o.Value);
            }

            return 0;
        }
        
        
        public double GetContribution(string userId, DateTime startDate, DateTime endDate)
        {
            double totalValue = 0;
            foreach (var task in Tasks)
            {
                if (task.UserId == userId && task.IsReviewed && task.CalculateValue() > 0)
                {

                    var actualStartDate = task.ActualDateRange.StartDate;
                    var actualEndDate = task.ActualDateRange.EndDate;

                    if (Name.StartsWith(Constant.REWARD_PROJECT_PREFIX) && task.ActualDateRange.StartDate == DateTime.MinValue && task.ActualDateRange.EndDate == DateTime.MinValue)
                    {
                        actualStartDate = task.PlanDateRange.StartDate;
                        actualEndDate = task.PlanDateRange.EndDate;
                    }

                    var calStartDate = actualStartDate < startDate ? startDate : actualStartDate;
                    var calEndDate = actualEndDate < endDate ? actualEndDate : endDate;
                    int actualWorkingDays = DateTimeUtil.GetWorkingDays(actualStartDate, actualEndDate);
                    if (actualWorkingDays == 0) continue;
                    int calWorkingDays = DateTimeUtil.GetWorkingDays(calStartDate, calEndDate);
                    if (calWorkingDays == 0) continue;

                    totalValue += (task.CalculateValue() * calWorkingDays / actualWorkingDays);
                }
            }

            return Math.Round(totalValue);
        }

        public double GetPlanHour(string userId, DateTime startDate, DateTime endDate)
        {
            var planStartDate = PlanDateRange.StartDate;
            var planEndDate = PlanDateRange.EndDate;

            if(Status == Status.Done)
            {
                planEndDate = ActualDateRange.EndDate;
            }

            var calStartDate = planStartDate < startDate ? startDate : planStartDate;
            var calEndDate = planEndDate < endDate ? planEndDate : endDate;

            if (PlanHours.IsEmpty() || !PlanHours.ContainsKey(userId))
            {
                return 0;
            }

            if (DateTimeUtil.GetWorkingDays(planStartDate, planEndDate) == 0)
            {
                return 0;
            }

            if(DateTimeUtil.GetWorkingDays(calStartDate, calEndDate) == 0)
            {
                return 0;
            }

            return Math.Round(PlanHours[userId] * DateTimeUtil.GetWorkingDays(calStartDate, calEndDate) / DateTimeUtil.GetWorkingDays(planStartDate, planEndDate), 2);
        }

        //风险系数，系数越高越危险
        public double GetRiskFactor()
        {
            double factor = 100;

            if (Status != Status.Done && PlanDateRange.EndDate > PlanDateRange.StartDate && DateTime.Today > PlanDateRange.StartDate)
            {
                var planHour = (DateTime.Today - PlanDateRange.StartDate).Days * GetTotalPlanHour() / (PlanDateRange.EndDate - PlanDateRange.StartDate).Days;
                if (GetTotalActualHour() > 0)
                {
                    factor = Math.Round(planHour * 100 / GetTotalActualHour(), 2);
                }
                else
                {
                    factor = 500;
                } 
            }

            return factor;
        }

        public bool IsImportant()
        {
            var hour = int.Parse(Setting.Instance.Get("ImportantHour"));
            return Level != ProjectLevel.Normal || GetTotalPlanHour() >= hour || GetTotalActualHour() >= hour;
        }

        public DateTime GetEndDate()
        {
            if(Status == Status.Done)
            {
                return ActualDateRange.EndDate;
            }
            else
            {
                return PlanDateRange.EndDate;
            }
        }

        public bool IsTaskProject()
        {
            return !Tasks.IsEmpty();
        }

        public void UpdateProjectStatus()
        {
            //如果所有的任务都完成了，项目就完成了
            if (!Tasks.IsEmpty())
            {
                var sum = Tasks.Sum(o => (int)o.Status);

                if (sum == Tasks.Count * 2)
                {
                    Status = Status.Done;
                }
                else if (sum > 0)
                {
                    Status = Status.Ongoing;
                }
                else
                {
                    Status = Status.Pending;
                }
            }
        }

        public void UpdateProjectActualTime()
        {
            //如果所有的任务都完成了，项目就完成了
            if (!Tasks.IsEmpty())
            {
                var startDates = Tasks.Select(o => o.ActualDateRange.StartDate).Where(o => !o.IsEmpty()).ToList();
                var endDates = Tasks.Select(o => o.ActualDateRange.EndDate).Where(o => !o.IsEmpty()).ToList();

                ActualDateRange.StartDate = startDates.Count() > 0 ? startDates.Min() : DateTime.MinValue;
                ActualDateRange.EndDate = endDates.Count() > 0 ? endDates.Max() : DateTime.MinValue;
            }
        }
    }
}
