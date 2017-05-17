namespace Web.Model.Chart.Timeline
{
    using System.Collections.Generic;

    public class TimelineModel
    {
        public List<TimelineItem> Timelines { get; set; }

        public TimelineModel(List<TimelineItem> timelines)
        {
            this.Timelines = timelines;
        }
    }
}
