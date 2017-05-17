namespace Web.Model.Chart.Bar
{
    using System.Collections.Generic;

    public class BarModel
    {
        public List<BarItem> Bars { get; set; }

        public BarModel(List<BarItem> bars)
        {
            Bars = bars;
        }
    }
}
