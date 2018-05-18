namespace Web.Controllers
{
    using Common;
    using Extension;
    using Entity.Other;
    using Platform.Util;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Entity.Project;
    using System.Collections.Generic;
    using Service.Project;
    using System.Text;

    [Authorize]
    public class ImportController : BaseController
    {
        [Authorize(Roles = "1")]
        public IActionResult Index(string successMsg, string errorMsg)
        {
            this.BuildHeaderMsg(successMsg, errorMsg);
            ViewData["UserId"] = this.GetUserId();

            return View();
        }

        public IActionResult ImportProject(Import import)
        {
            if (import.Data != null)
            {
                var projectService = this.GetService<ProjectService>();

                List<Project> projects = JsonUtil.FromJson<List<Project>>(import.Data);
                if (projects != null && projects.Count > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var project in projects)
                    {
                        if (project.Id != null)
                        {
                            sb.Append(project.Id).Append(",");
                            projectService.Update(project);
                        }
                        else
                        {
                            string id = projectService.Create(project);
                            sb.Append(id).Append(",");
                        }
                    }
                    ViewData["Result"] = "Update Projects: " + sb.ToString();

                }
                else
                {
                    ViewData["Result"] = "Make sure you input valid Data!";
                }
            }

            return View();
        }

        public IActionResult ImportTimesheet(Import import)
        {
            var data = ViewData["importData"];
            if (data != null)
            {
                var timeSheetService = this.GetService<TimeSheetService>();

                List<TimeSheet> timesheets = JsonUtil.FromJson<List<TimeSheet>>(data.ToString());
                foreach (var obeject in timesheets)
                {
                    if (obeject.Id != null)
                    {
                        timeSheetService.Update(obeject);
                    }
                    else
                    {
                        timeSheetService.Create(obeject);
                    }
                }
            }

            return View();
        }
    }
}
