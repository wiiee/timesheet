namespace Web.Model.Chart.Combo
{
    using System.Collections.Generic;

    public class ComboItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string UserType { get; set; }
        public List<KeyValuePair<string, double>> Items { get; set; }

        public ComboItem(string id, string name, string userType, List<KeyValuePair<string, double>> items)
        {
            Id = id;
            Name = name;
            UserType = userType;
            Items = items;
        }
    }
}
