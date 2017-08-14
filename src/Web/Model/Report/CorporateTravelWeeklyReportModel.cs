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

    public class CorporateTravelWeeklyReportModel
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

        public CorporateTravelWeeklyReportModel(Project project, string userGroupName, string percentageCompletion, string progressText,
            string status, List<string> devManagers, List<string> testManagers, List<string> devNames, List<string> testNames)
        {
            this.Project = project;
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
            strings.Add(Project.Level.ToString());
            strings.Add(Project.IsCr ? "True" : "False");
            return string.Join("$", strings);
        }
    }

}
