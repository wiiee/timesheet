namespace Web.Model.Report
{
    using System;
    using System.Collections.Generic;

    public class FlightMonthlyReportModel : MonthlyReportModel
    {
        
        public FlightMonthlyReportModel(string projectId, string projectName, string groupName, int level, bool isDone, DateTime finishedDate) 
            : base(projectId, projectName, groupName, level, isDone, finishedDate)
        {

        }
    }
}
