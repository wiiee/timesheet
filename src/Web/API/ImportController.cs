namespace Web.API
{
    using Common;
    using Entity.Project;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Platform.Extension;
    using Platform.Util;
    using Service.Project;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [Authorize]
    [Route("api/[controller]")]
    public class ImportController : BaseController
    {
        private static ILogger _logger = LoggerUtil.CreateLogger<ImportController>();

        [Route("ImportProjects")]
        [HttpPost]
        public List<string> ImportProjects([FromBody]List<Project> projects)
        {
            List<string> result = new List<string>();

            try
            {
                var projectService = this.GetService<ProjectService>();

                if (!projects.IsEmpty())
                {
                    foreach (var project in projects)
                    {
                        if (project.Id != null)
                        {
                            projectService.Update(project);
                            result.Add(project.Id);
                        }
                        else
                        {
                            var projectId = projectService.Create(project);
                            result.Add(projectId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            return result;
        }

        [Route("ImportTimeSheets")]
        [HttpPost]
        public string ImportTimeSheets([FromBody]List<TimeSheet> timesheets)
        {
            List<string> result = new List<string>();

            try
            {
                var timeSheetService = this.GetService<TimeSheetService>();

                foreach (var item in timesheets)
                {
                    if (item.Id != null)
                    {
                        timeSheetService.Update(item);
                    }
                    else
                    {
                        timeSheetService.Create(item);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return ex.Message;
            }

            return "Done";
        }
    }
}
