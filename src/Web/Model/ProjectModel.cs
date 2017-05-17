namespace Web.Model
{
    using Entity.Project;

    public class ProjectModel
    {
        public Project Project { get; set; }

        public string DepartmentNames { get; set; }

        public string UserNames { get; set; }
        public string OwnerNames { get; set; }

        public double TotalPlanHour { get; set; }
        public double TotalActualHour { get; set; }

        public bool IsEdit { get; set; }
        public bool IsDelete { get; set; }
        public bool IsClose { get; set; }
        public bool IsPostpone { get; set; }

        public ProjectModel(Project project, string departmentNames, string ownerNames, string userNames, double totalPlanHour, double totalActualHour, bool isEdit, bool isDelete, bool isClose, bool isPostpone)
        {
            this.Project = project;
            this.IsEdit = isEdit;
            this.IsDelete = isDelete;
            this.IsClose = isClose;
            this.IsPostpone = isPostpone;
            this.UserNames = userNames;
            this.OwnerNames = ownerNames;
            this.TotalActualHour = totalActualHour;
            this.TotalPlanHour = totalPlanHour;
            this.DepartmentNames = departmentNames;
        }

        public object ToRow() {
            return new {
                Id = Project.Id,
                Description = Project.Description,
                Name = Project.Name,
                UserNames = UserNames,
                OwnerNames = OwnerNames,
                StartDate = Project.PlanDateRange.StartDate,
                EndDate = Project.PlanDateRange.EndDate,
                TotalPlanHour = Project.GetTotalPlanHour(),
                TotalActualHour = Project.GetTotalActualHour(),
                Status = Project.Status.ToString(),
                Level = Project.Level.ToString(),
                IsReviewed = Project.IsReviewed,
                DepartmentNames = DepartmentNames,
                Tasks = Project.Tasks,
                IsCr = Project.IsCr,
                IsEdit = IsEdit,
                IsClose = IsClose,
                IsPostpone = IsPostpone,
                IsDelete = IsDelete,
                IsShow = true
            };
        }
    }
}
