namespace Web.Model.Report
{
    using System;
    using System.Collections.Generic;

    public class HotelWeeklyReportModel
    {
        public string UserGroupName { get; set; }
        public List<KeyValuePair<string, string>> Projects { get; set; }
        public double DevPercentage { get; set; }
        public double TestPercentage { get; set; }
        public DateTime EndDate { get; set; }
        public string ProgressText { get; set; }
        public string NextWeekPlan { get; set; }
        public string Comment { get; set; }

        public HotelWeeklyReportModel(string userGroupName, List<KeyValuePair<string, string>> projects, double devPercentage, double testPercentage,
            DateTime endDate, string progressText, string nextWeekPlan, string comment)
        {
            this.UserGroupName = userGroupName;
            this.Projects = projects;
            this.DevPercentage = devPercentage;
            this.TestPercentage = testPercentage;
            this.EndDate = endDate;
            this.ProgressText = progressText;
            this.NextWeekPlan = nextWeekPlan;
            this.Comment = comment;
        }
    }
}
