namespace Web.Model.Chart.Line
{
    using System.Collections.Generic;

    public class LineItem
    {
        //线的名字
        public string Name { get; set; }
        //线上各点的值
        public List<double> Values { get; set; }

        public LineItem(string name, List<double> values)
        {
            this.Name = name;
            this.Values = values;
        }

        public LineItem()
        {

        }
    }
}
