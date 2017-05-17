namespace Web.Model.Chart.Timeline
{
    using System;

    public class TimelinePart
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public TimelinePart(string name, DateTime startDate, DateTime endDate)
        {
            this.Name = name;
            this.StartDate = startDate;
            this.EndDate = endDate;
        }
    }
}
