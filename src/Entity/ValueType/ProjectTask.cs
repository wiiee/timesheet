namespace Entity.ValueType
{
    using Platform.Enum;
    using Platform.Extension;
    using Platform.Util;
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
        public Dictionary<string, int> Values { get; set;}

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

                if(vaildValues.Count == 0)
                {
                    return 0;
                }

                var total = vaildValues.Sum(o => o.Value);
                var min = vaildValues.Min(o => o.Value);
                var max = vaildValues.Max(o => o.Value);
                var count = vaildValues.Count;

                if(count < 2)
                {
                    return total;
                }
                else if(count == 2)
                {
                    return min;
                }
                else
                {
                    return (total - min - max) / (count - 2);
                }
            }
        }

        public void EncryptValues(string userId, bool isLeader = false)
        {
            if (!isLeader)
            {
                var result = new Dictionary<string, int>();

                foreach (var value in Values)
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

                Values = result;
            }
        }

        public ProjectTask Merge(ProjectTask other)
        {
            var a = JsonUtil.FromJson<ProjectTask>(JsonUtil.ToJson(this));
            var b = JsonUtil.FromJson<ProjectTask>(JsonUtil.ToJson(other));

            a.Values = null;
            b.Values = null;

            if(JsonUtil.ToJson(a) == JsonUtil.ToJson(b) && JsonUtil.ToJson(this.Values.Keys) == JsonUtil.ToJson(other.Values.Keys))
            {
                var values = new Dictionary<string, int>();

                foreach(var item in this.Values)
                {
                    //如果两个数据一样，或者其中有一个是0，或者有一个是-1的，表示没有冲突
                    if(item.Value == other.Values[item.Key] || item.Value * other.Values[item.Key] <= 0)
                    {
                        values.Add(item.Key, Math.Max(item.Value, other.Values[item.Key]));
                    }
                    else
                    {
                        return null;
                    }
                }

                this.Values = values;
                return this;
            }

            return null;
        }
    }
}
