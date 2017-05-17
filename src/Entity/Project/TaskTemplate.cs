namespace Entity.Project
{
    using System.Collections.Generic;
    using ValueType;

    public class TaskTemplate : BaseEntity
    {
        //Id为GroupId

        public List<TaskInfo> Tasks { get; set; }

        public TaskTemplate() { }

        public TaskTemplate(string groupId, List<TaskInfo> tasks)
        {
            this.Id = groupId;
            this.Tasks = tasks;
        }
    }
}
