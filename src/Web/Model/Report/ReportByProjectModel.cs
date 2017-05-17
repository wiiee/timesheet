namespace Web.Model.Report
{
    using Entity.ValueType;

    public class ReportByProjectModel
    {
        public string Id { get; set; }
        public string Name { get; set; }

        //计划时间
        public double PlanDevHour { get; set; }
        public double PlanTestHour { get; set; }
        public double TotalPlanHour { get; set; }

        //实际时间
        public double ActualDevHour { get; set; }
        public double ActualTestHour { get; set; }
        public double TotalActualHour { get; set; }

        //完成百分比
        public double Percentage { get; set; }

        public DateRange PlanDateRange { get; set; }
        public DateRange ActualDateRange { get; set; }
        public string Status { get; set; }

        public string ProjectException { get; set; }

        public ReportByProjectModel(string id, string name, double totalPlanHour, double planDevHour, double planTestHour, double totalActualHour, 
            double actualDevHour, double actualTestHour, DateRange planDateRange, DateRange actualDateRange, string status, string projectException)
        {
            this.Id = id;
            this.Name = name;

            this.TotalPlanHour = totalPlanHour;
            this.PlanDevHour = planDevHour;
            this.PlanTestHour = PlanTestHour;
            this.TotalActualHour = totalActualHour;
            this.ActualDevHour = actualDevHour;
            this.ActualTestHour = actualTestHour;

            if (totalPlanHour != 0)
            {
                this.Percentage = totalActualHour / totalPlanHour;
            }

            this.PlanDateRange = planDateRange;
            this.ActualDateRange = actualDateRange;
            this.Status = status;
            this.ProjectException = projectException;
        }
    }
}
