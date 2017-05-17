namespace Web.Model.Report
{
    using Chart.Line;
    using Entity.Project;
    using System.Collections.Generic;

    public class ProjectOverviewModel
    {
        public Project Project { get; set; }
        public double Percentage { get; set; }
        public double DevPercentage { get; set; }
        public double TestPercentage { get; set; }
        public string OwnerNames { get; set; }
        public string UserNames { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedBy { get; set; }
        public string PostponeReasons { get; set; }

        public LineModel Burndown { get; set; }
        public string Status { get; set; }

        //项目时间，计划时间，实际时间
        public Dictionary<string, HourItem> Hours { get; set; }

        public ProjectOverviewModel(Project project, double percentage, double devPercentage, double testPercentage, string ownerNames, string userNames,
            string createdBy, string lastUpdatedBy, string postponeReasons, Dictionary<string, HourItem> hours, LineModel burndown, string status)
        {
            this.Project = project;
            this.Percentage = percentage;
            this.DevPercentage = devPercentage;
            this.TestPercentage = testPercentage;
            this.OwnerNames = ownerNames;
            this.UserNames = userNames;
            this.CreatedBy = createdBy;
            this.LastUpdatedBy = lastUpdatedBy;
            this.PostponeReasons = postponeReasons;
            this.Hours = hours;
            this.Burndown = burndown;
            this.Status = status;
        }
    }
}
