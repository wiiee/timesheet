namespace Web.Model.Report
{
    public class ReportByUserModel
    {
        public string Id { get; set; }
        public string Name { get; set; }

        //计划花费时间
        public double TotalPlanHour { get; set; }
        //总花费时间
        public double TotalActualHour { get; set; }

        public int ExpiredProjectNumber { get; set; }
        public int OvertimeProjectNumber { get; set; }
        public int TotalProjectNumber { get; set; }

        public ReportByUserModel(string id, string name, double totalPlanHour, double totalActualHour,
            int expiredProjectNumber, int overtimeProjectNumber, int totalProjectNumber)
        {
            this.Id = id;
            this.Name = name;
            this.TotalPlanHour = totalPlanHour;
            this.TotalActualHour = totalActualHour;
            this.ExpiredProjectNumber = expiredProjectNumber;
            this.OvertimeProjectNumber = overtimeProjectNumber;
            this.TotalProjectNumber = totalProjectNumber;
        }
    }
}
