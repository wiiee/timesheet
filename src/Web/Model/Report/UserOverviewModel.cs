namespace Web.Model.Report
{
    using Chart.Line;
    using System.Collections.Generic;

    public class UserOverviewModel
    {
        //用户Id
        public string Id { get; set; }

        //用户名
        public string Name { get; set; }

        //实际时间和计划时间
        public LineModel PlanActualLine { get; set; }

        //公共项目,KeyValuePair<double, double>为Plan,Actual
        public Dictionary<string, KeyValuePair<double, double>> PublicProjects { get; set; }

        //项目
        public Dictionary<string, KeyValuePair<double, double>> Projects { get; set; }

        //Cr
        public Dictionary<string, KeyValuePair<double, double>> Crs { get; set; }

        public UserOverviewModel(string id, string name, Dictionary<string, KeyValuePair<double, double>> publicProjects,
            Dictionary<string, KeyValuePair<double, double>> projects, Dictionary<string, KeyValuePair<double, double>> crs,
            LineModel planActualLine)
        {
            this.Id = id;
            this.Name = name;
            this.PublicProjects = publicProjects;
            this.Projects = projects;
            this.Crs = crs;
            this.PlanActualLine = planActualLine;
        }
    }
}
