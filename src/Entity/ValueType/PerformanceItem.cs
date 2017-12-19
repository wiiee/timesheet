namespace Entity.ValueType
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class PerformanceItem
    {
        public int Id { get; set; }
        public DateRange DateRange { get; set; }

        public double Factor { get; set; }

        public int TimeSheetPercentage;
        public int ManagerPercentage;

        public Dictionary<string, Score> Values { get; set; }

        public PerformanceItem(){
            Values = new Dictionary<string, Score>();
        }

        public void Calculate(Dictionary<string, int> levels){
            foreach(var item in Values){
                var level = Math.Max(1, levels[item.Key]);
                item.Value.StandardValue = Convert.ToInt32(item.Value.TimeSheetValue / Math.Pow(level, Factor));
            }

            var averageValue = Values.Average(o => o.Value.StandardValue);

            var referenceValue = Math.Max(averageValue / 0.75, Values.Max(o => o.Value.StandardValue));

            foreach(var item in Values){
                item.Value.Outcome = Convert.ToInt32(100 * item.Value.Outcome / referenceValue);
                item.Value.Result = Convert.ToInt32((item.Value.Outcome * TimeSheetPercentage + item.Value.ManagerValue * ManagerPercentage) / 100);
            }
        }
    }
}
