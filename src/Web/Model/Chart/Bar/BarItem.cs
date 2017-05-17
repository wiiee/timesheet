namespace Web.Model.Chart.Bar
{
    using System.Collections.Generic;

    public class BarItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<KeyValuePair<string, double>> Items { get; set; }

        public BarItem(string id, string name, List<KeyValuePair<string, double>> items)
        {
            Id = id;
            Name = name;
            Items = items;
        }
    }
}
