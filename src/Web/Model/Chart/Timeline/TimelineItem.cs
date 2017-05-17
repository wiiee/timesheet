namespace Web.Model.Chart.Timeline
{
    using System.Collections.Generic;

    public class TimelineItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SerialNumber { get; set; }

        public List<TimelinePart> Parts { get; set; }

        public TimelineItem(string id, string name, string serialNumber, List<TimelinePart> parts)
        {
            this.Id = id;
            this.Name = name;
            this.SerialNumber = serialNumber;
            this.Parts = parts;
        }
    }
}
