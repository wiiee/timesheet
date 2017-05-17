namespace Web.Model
{
    using Entity.Project;
    using System.Collections.Generic;
    using System.Linq;

    public class TimeSheetModel
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public string TaskName { get; set; }
        public int TaskId { get; set; }
        //项目是否是Public
        public bool IsPublic { get; set; }
        public double[] Week { get; set; }

        public bool IsSelected { get; set; }

        public bool IsDone { get; set; }

        public Dictionary<long, KeyValuePair<string, string>> Murmurs { get; set; }

        public TimeSheetModel()
        {

        }

        public TimeSheetModel(Project project, int taskId, string taskName, bool isDone, double[] week)
        {
            this.ProjectId = project.Id;
            this.ProjectName = project.Name;
            this.ProjectDescription = project.Description;
            this.TaskName = taskName;
            this.TaskId = taskId;
            this.IsPublic = project.IsPublic;
            this.Murmurs = project.Murmurs;
            this.Week = week;
            this.IsSelected = week.Sum() > 0;
            this.IsDone = isDone;
        }
    }
}
