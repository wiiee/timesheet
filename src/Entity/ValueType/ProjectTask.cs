namespace Entity.ValueType
{
    using Platform.Enum;
    using System.Collections.Generic;

    public class ProjectTask
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string UserId { get; set; }

        public double PlanHour { get; set; }

        public double ActualHour { get; set; }

        public Phase Phase { get; set; }

        public DateRange PlanDateRange { get; set; }
        public DateRange ActualDateRange { get; set; }

        public Status Status { get; set; }

        public string CodeReview { get; set; }
        public Dictionary<string, double> Values { get; set;}

        //工作的价值
        public double Value { get; set; }

        public bool IsReviewed { get; set; }
    }
}
