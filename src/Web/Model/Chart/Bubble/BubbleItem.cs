namespace Web.Model.Chart.Bubble
{
    public class BubbleItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double H { get; set; }
        public double V { get; set; }
        public string Category { get; set; }
        public double Size { get; set; }

        public BubbleItem(string id, string name, double h, double v, string category, double size)
        {
            this.Id = id;
            this.Name = name;
            this.H = h;
            this.V = v;
            this.Category = category;
            this.Size = size;
        }
    }
}
