namespace Entity.ValueType
{
    using Platform.Enum;
    using Platform.Extension;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ProjectTask
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string UserId { get; set; }

        public double PlanHour { get; set; }

        public double ActualHour { get; set; }

        public Phase Phase { get; set; }

        public DateRange PlanDateRange { get; set; }
        public DateRange ActualDateRange { get; set; }

        public Status Status { get; set; }

        public string CodeReview { get; set; }
        public Dictionary<string, double> Values { get; set;}

        //工作的价值
        public double Value { get; set; }

        public bool IsReviewed { get; set; }

        public int CalculateValue()
        {
            if (Values.IsEmpty())
            {
                return 0;
            }
            else
            {
                var vaildValues = Values.Where(o => o.Value > 0).ToList();

                var total = vaildValues.Sum(o => o.Value);
                var min = vaildValues.Min(o => o.Value);
                var max = vaildValues.Max(o => o.Value);
                var count = vaildValues.Count;

                if(count < 2)
                {
                    return (int)total;
                }
                else if(count == 2)
                {
                    return (int)min;
                }
                else
                {
                    var result = (total - min - max) / (count - 2);
                    return (int)result;
                }
            }
        }

        public void EncryptValues(string userId, bool isLeader = false)
        {
            var result = new Dictionary<string, double>();
            if (!isLeader)
            {
                foreach(var value in Values)
                {
                    if(value.Key != userId)
                    {
                        result.Add(value.Key, -1);
                    }
                    else
                    {
                        result.Add(value.Key, value.Value);
                    }
                }
            }
        }
    }
}
