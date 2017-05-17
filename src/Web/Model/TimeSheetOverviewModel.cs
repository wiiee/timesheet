namespace Web.Model
{
    using System;

    public class TimeSheetOverviewModel
    {
        public DateTime Monday { get; set; }
        public DateTime Sunday { get; set; }
        public string Status { get; set; }
        public string Text { get; set; }
        public double WeekHours { get; set; }
        public bool IsReturn { get; set; }
        public bool IsShow { get; set; }

        public TimeSheetOverviewModel(DateTime monday, DateTime sunday, string status, string text, double WeekHours, 
            bool isReturn, bool isShow)
        {
            this.Monday = monday;
            this.Sunday = sunday;
            this.Text = text;
            this.Status = status;
            this.WeekHours = WeekHours;
            this.IsReturn = isReturn;
            this.IsShow = isShow;
        }

        public TimeSheetOverviewModel()
        {
        }
    }
}
