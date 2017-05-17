namespace Entity.ValueType
{
    using Platform.Enum;
    using System.Collections.Generic;

    public class TaskInfo
    {
        public string Name { get; set; }
        public Phase Phase { get; set; }
        public List<double> Hours { get; set; }
        public string Description { get; set; }
    }
}
