namespace Web.API
{
    using Common;
    using Entity.Project;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Platform.Enum;
    using Platform.Extension;
    using Platform.Util;
    using Service.Project;
    using Service.User;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Web.Util;

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
                        if (!project.Tasks.IsEmpty())
                        {
                            if (!string.IsNullOrEmpty(project.Id))
                            {
                                if (project.ActualHours.IsEmpty())
                                {
                                    project.ActualHours = new Dictionary<string, double>();
                                }
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
                var userTimeSheetStatusService = this.GetService<UserTimeSheetStatusService>();
                var projectService = this.GetService<ProjectService>();
                var userService = this.GetService<UserService>();
                var departmentService = this.GetService<DepartmentService>();

                foreach (var item in timesheets)
                {
                    projectService.Delete(item.Id);

                    var project = projectService.Get(item.ProjectId);
                    var timeSheet = timeSheetService.Get(item.Id);

                    if (timeSheet == null)
                    {
                        timeSheet = new TimeSheet(item.ProjectId, item.UserId);
                        timeSheetService.Create(timeSheet);
                    }

                    if(timeSheet.WeekTimeSheets == null)
                    {
                        timeSheet.WeekTimeSheets = new Dictionary<string, Dictionary<int, double[]>>();
                    }

                    foreach (var week in item.WeekTimeSheets)
                    {
                        if (timeSheet.WeekTimeSheets.ContainsKey(week.Key))
                        {
                            timeSheet.WeekTimeSheets[week.Key] = week.Value;
                        }
                        else
                        {
                            timeSheet.WeekTimeSheets.Add(week.Key, week.Value);
                        }

                        //userTimeSheetStatusService.UpdateUserTimeSheet(item.UserId, week.Key, Status.Ongoing, 40);
                    }

                    timeSheetService.Update(timeSheet);

                    //更新动态数据
                    projectService.UpdateActualParts(timeSheet);
                }

                var projects = projectService.GetByIds(timesheets.Select(o => o.ProjectId).ToList());
                TimeSheetUtil.RefreshProjectTask(projectService, departmentService, userService, projects);
                TimeSheetUtil.UpdateProject(timeSheetService, projectService, projects);
                var items = timeSheetService.GetByIds(timesheets.Select(o => o.Id).ToList());
                TimeSheetUtil.ResetTimeSheet(projectService, timeSheetService, items);
                var userTimeSheetStatuses = userTimeSheetStatusService.GetByIds(timesheets.Select(o => o.UserId).Distinct().ToList());
                TimeSheetUtil.ResetUserTimeSheet(timeSheetService, userTimeSheetStatusService, userTimeSheetStatuses);
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
