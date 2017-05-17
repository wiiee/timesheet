namespace Service.Extension
{
    using Entity.Project.Scrum;
    using Entity.ValueType;
    using Platform.Enum;
    using System.Collections.Generic;
    using System.Linq;

    public static class SprintExtension
    {
        public static Status GetStatus(this Sprint sprint)
        {
            var tasks = new List<ProjectTask>();

            var result = tasks.Sum(o => (int)o.Status);

            if(result == 0)
            {
                return Status.Pending;
            }
            else if(result < tasks.Count * 2)
            {
                return Status.Ongoing;
            }
            else
            {
                return Status.Done;
            }
        }
    }
}
