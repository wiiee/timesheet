namespace Web.Model.Report
{
    public class HourItem
    {
        public double Plan { get; set; }
        public double Actual { get; set; }

        //指定时间段
        public double Search { get; set; }

        public HourItem(double plan, double actual, double search)
        {
            this.Plan = plan;
            this.Actual = actual;
            this.Search = search;
        }
    }
}
