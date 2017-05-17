namespace Web.Model
{
    using Entity.ValueType;

    public class TaskModel
    {
        public ProjectTask ProjectTask { get; set; }

        public string ProjectId { get; set; }

        public string projectName { get; set; }

        public string UserName { get; set; }

        public TaskModel(string projectId, string projectName, string userName, ProjectTask projectTask)
        {
            this.ProjectId = projectId;
            this.projectName = projectName;
            this.UserName = userName;
            this.ProjectTask = projectTask;
        }
    }
}
