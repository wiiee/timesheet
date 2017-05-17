using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web.Model.Report
{
    public class MonthlyReportModel
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string GroupName { get; set; }
        public int Level { get; set; }
        public bool IsDone { get; set; }
        public DateTime FinishedDate { get; set; } 

        public MonthlyReportModel(string projectId, string projectName, string groupName, int level, bool isDone, DateTime finishedDate)
        {
            this.ProjectId = projectId;
            this.ProjectName = projectName;
            this.GroupName = groupName;
            this.Level = level;
            this.IsDone = isDone;
            this.FinishedDate = finishedDate;
        }
    }
}
