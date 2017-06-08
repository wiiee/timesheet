namespace Web.Model.Report
{
    using System;
    using Extension;
    using System.Collections.Generic;
    using Platform.Extension;
    using Entity.Project;
    using Entity.ValueType;
    using System.Linq;
    using Platform.Enum;

    public class FlightWeeklyReportModel
    {
        public string ProjectID { get; set; }
        public string ProjectName { get; set; }
        public string UserGroupName { get; set; }
        public string ProductManagerName { get; set; }
        public string SerialNumber { get; set; }
        public DateTime PlanCombinedTestDate { get; set; } //预计联调日期
        public DateTime ActualCombinedTestDate { get; set; } //实际联调日期
        public DateTime PlanEndDate { get; set; } // 最初要求上线日期
        public DateTime ActualEndDate { get; set; } // 调整后（计划）上线日期
        public string ProgressText { get; set; } //整体进度：开发；测试；已完成 等
        public string PercentageCompletion { get; set; } //完成百分比
        public string Status { get; set; } //状态：准时；延期；提前
        public string Comment { get; set; }
        public bool IsImportant { get; set; } //是否重要项目
        public string AllParticipants{ get; set;}
        public Project Project { get; set; }

        public WeeklyReportExtrasInfo ExtrasInfo { get; set; }

        public FlightWeeklyReportModel(Project project, string userGroupName, string percentageCompletion, string progressText,
            string status, List<string> devManagers, List<string> testManagers, List<string> devNames, List<string> testNames)
        {
            this.Project = project;
            this.ExtrasInfo = new WeeklyReportExtrasInfo(project, devManagers, testManagers, devNames, testNames);
            this.UserGroupName = userGroupName;
            this.ProjectName = project.Name;
            this.ProjectID = project.Id;
            this.ProductManagerName = project.ProjectManagerName;
            this.SerialNumber = project.SerialNumber;
            this.PlanEndDate = project.PlanEndDate; 
            this.ActualEndDate = project.GetEndDate();
            this.ProgressText = progressText;
            ProjectTask task = project.Tasks.Where(o => o.Phase == Phase.Publish).FirstOrDefault();
            this.PlanCombinedTestDate = task != null && task.PlanDateRange != null ? task.PlanDateRange.StartDate : DateTime.MinValue;
            this.ActualCombinedTestDate = task != null && task.ActualDateRange != null ? task.ActualDateRange.StartDate : DateTime.MinValue;

            this.Comment = project.Comment;
            this.IsImportant = project.Level == Platform.Enum.ProjectLevel.High;
            this.PercentageCompletion = percentageCompletion;
            this.AllParticipants = getAllParticipants(devNames, testNames);
            this.Status = status;
        }

        private string getAllParticipants(List<string> devNames, List<string> testNames)
        {
            if (devNames.IsEmpty())
            {
                return string.Format("测试:{0}", string.Join("\\", testNames));
            }
            if (testNames.IsEmpty())
            {
                return string.Format("开发:{0}", string.Join("\\", devNames));
            }
            return string.Format("开发:{0} 测试:{1}", string.Join("\\", devNames), string.Join("\\", testNames));
        }

        //导出到Excel表专用
        public override string ToString()
        {
            IList<string> strings = new List<string>();
            strings.Add(ProjectName);
            strings.Add(SerialNumber);
            strings.Add(UserGroupName);
            strings.Add(ProductManagerName);
            strings.Add(PercentageCompletion);
            strings.Add(ProgressText);
            strings.Add(Status);
            strings.Add(PlanCombinedTestDate.ToSimpleString());
            strings.Add(ActualCombinedTestDate.IsEmpty() ? PlanCombinedTestDate.ToSimpleString() : ActualCombinedTestDate.ToSimpleString());
            strings.Add(PlanEndDate.ToSimpleString());
            strings.Add(ActualEndDate.ToSimpleString());
            strings.Add(AllParticipants);
            strings.Add(Comment);
            //Extras information
            strings.Add(ExtrasInfo.DevManager);
            strings.Add(ExtrasInfo.DevName);
            strings.Add(ExtrasInfo.DevPlanStartDate.ToSimpleString());
            strings.Add(ExtrasInfo.DevActualStartDate.ToSimpleString());
            strings.Add(ExtrasInfo.DevPlanEndDate.ToSimpleString());
            strings.Add(ExtrasInfo.DevActualEndDate.ToSimpleString());
            strings.Add(ExtrasInfo.DevPlanHour.ToString());
            strings.Add(ExtrasInfo.DevActualHour.ToString());

            strings.Add(ExtrasInfo.TestManager);
            strings.Add(ExtrasInfo.TestName);
            strings.Add(ExtrasInfo.TestPlanStartDate.ToSimpleString());
            strings.Add(ExtrasInfo.TestActualStartDate.ToSimpleString());
            strings.Add(ExtrasInfo.TestPlanEndDate.ToSimpleString());
            strings.Add(ExtrasInfo.TestActualEndDate.ToSimpleString());
            strings.Add(ExtrasInfo.TestPlanHour.ToString());
            strings.Add(ExtrasInfo.TestActualHour.ToString());

            strings.Add(Project.Level.ToString());
            strings.Add(Project.IsCr ? "True" : "False");
            return string.Join("$", strings);
        }
    }

    public class WeeklyReportExtrasInfo
    {
        public Project Project { get; set; }
        public string DevManager { set; get; }
        public string TestManager { set; get; }
        public string DevName { set; get; }
        public string TestName { set; get; }

        public DateTime DevPlanStartDate { get; set; } // 计划编码开始日期
        public DateTime DevPlanEndDate { get; set; } // 计划编码结束日期
        public DateTime TestPlanStartDate { get; set; } // 计划测试开始日期
        public DateTime TestPlanEndDate { get; set; } // 计划测试结束日期

        public DateTime DevActualStartDate { get; set; } // 实际编码开始日期
        public DateTime DevActualEndDate { get; set; } // 实际编码结束日期
        public DateTime TestActualStartDate { get; set; } // 实际测试开始日期
        public DateTime TestActualEndDate { get; set; } // 实际测试结束日期

        public double DevPlanHour { get; set; } // 计划编码时长
        public double DevActualHour { get; set; } // 实际编码时长
        public double TestPlanHour { get; set; } // 计划测试时长
        public double TestActualHour { get; set; } // 实际测试时长

        public WeeklyReportExtrasInfo(Project project, List<string> devManagers, List<string> testManagers, List<string> devNames, 
            List<string> testNames)
        {
            this.DevManager = string.Join("\\", devManagers);
            this.TestManager = string.Join("\\", testManagers);
            this.DevName = string.Join("\\", devNames);
            this.TestName = string.Join("\\", testNames);
            
            //plan and actual hours
            this.DevPlanHour = project.Tasks.Where(o=>o.Phase < Phase.Test).Sum(o=>o.PlanHour);
            this.DevActualHour = project.Tasks.Where(o => o.Phase < Phase.Test).Sum(o => o.ActualHour);
            this.TestPlanHour = project.Tasks.Where(o => o.Phase >= Phase.Test).Sum(o => o.PlanHour);
            this.TestActualHour = project.Tasks.Where(o => o.Phase >= Phase.Test).Sum(o => o.ActualHour);

            //dev plan
            var devPlanStartDates = project.Tasks.Where(o => o.Phase < Phase.Test && o.PlanDateRange != null && !o.PlanDateRange.StartDate.IsEmpty()).Select(o => o.PlanDateRange.StartDate).ToList();
            var devPlanEndDates = project.Tasks.Where(o => o.Phase < Phase.Test && o.PlanDateRange != null && !o.PlanDateRange.EndDate.IsEmpty()).Select(o => o.PlanDateRange.EndDate).ToList();
            this.DevPlanStartDate = devPlanStartDates.Count > 0 ? devPlanStartDates.Min() : DateTime.MinValue;
            this.DevPlanEndDate = devPlanEndDates.Count > 0 ? devPlanEndDates.Max() : DateTime.MinValue;

            //dev actual
            var devActualStartDates = project.Tasks.Where(o => o.Phase < Phase.Test && o.ActualDateRange != null && !o.ActualDateRange.StartDate.IsEmpty()).Select(o => o.ActualDateRange.StartDate).ToList();
            var devActualEndDates = project.Tasks.Where(o => o.Phase < Phase.Test && o.ActualDateRange != null && !o.ActualDateRange.EndDate.IsEmpty()).Select(o => o.ActualDateRange.EndDate).ToList();
            this.DevActualStartDate = devActualStartDates.Count > 0? devActualStartDates.Min(): DateTime.MinValue;
            this.DevActualEndDate = devActualEndDates.Count > 0 ? devActualEndDates.Max() : DateTime.MinValue;

            //test plan
            var testPlanStartDates = project.Tasks.Where(o => o.Phase >= Phase.Test && o.PlanDateRange != null && !o.PlanDateRange.StartDate.IsEmpty()).Select(o => o.PlanDateRange.StartDate).ToList();
            var testPlanEndDates = project.Tasks.Where(o => o.Phase >= Phase.Test && o.PlanDateRange != null && !o.PlanDateRange.EndDate.IsEmpty()).Select(o => o.PlanDateRange.EndDate).ToList();
            this.TestPlanStartDate = testPlanStartDates.Count > 0 ? testPlanStartDates.Min() : DateTime.MinValue;
            this.TestPlanEndDate = testPlanEndDates.Count > 0 ? testPlanEndDates.Max() : DateTime.MinValue;

            //test actual
            var testActualStartDates = project.Tasks.Where(o => o.Phase >= Phase.Test && o.ActualDateRange != null && !o.ActualDateRange.StartDate.IsEmpty()).Select(o => o.ActualDateRange.StartDate).ToList();
            var testActualEndDates = project.Tasks.Where(o => o.Phase >= Phase.Test && o.ActualDateRange != null && !o.ActualDateRange.EndDate.IsEmpty()).Select(o => o.ActualDateRange.EndDate).ToList();
            this.TestActualStartDate = testActualStartDates.Count > 0 ? testActualStartDates.Min() : DateTime.MinValue;
            this.TestActualEndDate = testActualEndDates.Count > 0 ? testActualEndDates.Max() : DateTime.MinValue;

        }
    }
}
