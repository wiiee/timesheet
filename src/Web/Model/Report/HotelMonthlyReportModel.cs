namespace Web.Model.Report
{
    using System;
    using System.Collections.Generic;

    public class HotelMonthlyReportModel : MonthlyReportModel
    {
        public HotelMonthlyReportModel(string projectId, string projectName, string groupName, int level, bool isDone, DateTime finishedDate) 
                    : base(projectId, projectName, groupName, level, isDone, finishedDate)
        {
            
        }
    }
}
