namespace Web.Model.Chart.Line
{
    using System.Collections.Generic;
    using System.Linq;

    public class LineModel
    {
        //X坐标轴上各点的名字
        public List<string> Names { get; set; }
        public List<LineItem> Items { get; set; }

        public LineModel(List<string> names, List<LineItem> items)
        {
            this.Names = names;
            this.Items = items;
        }

        public LineModel()
        {
        }

        public string BuildLineData()
        {
            string result = "[";

            result += string.Format("[\"{0}\",\"{1}\"]", "Week", string.Join("\",\"", Items.Select(o => o.Name).ToList()));

            foreach(var item in Names)
            {
                var index = Names.IndexOf(item);
                result += string.Format(", [\"{0}\",{1}]", Names[index], string.Join(",", Items.Select(o => o.Values[index])));
            }

            result.TrimEnd();
            result.TrimEnd(',');

            result += "]";

            return result;
        }
    }
}
