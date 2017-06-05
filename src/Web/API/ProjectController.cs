namespace Web.API
{
    using Common;
    using Entity.Project;
    using Entity.ValueType;
    using Extension;
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

    [Authorize]
    [Route("api/[controller]")]
    public class ProjectController : BaseController
    {
        private static ILogger _logger = LoggerUtil.CreateLogger<ProjectController>();

        // GET: api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public Project Get(string id)
        {
            return this.GetService<ProjectService>().Get(id);
        }

        // POST api/values
        [HttpPost]
        public JsonResult Post([FromBody]Project project)
        {
            var projectService = this.GetService<ProjectService>();
            var userService = this.GetService<UserService>();

            if (project == null || string.IsNullOrEmpty(project.Id))
            {
                return Json(new  { errorMsg = "The data have some problem, please check it." });
            }

            var dbProject = projectService.Get(project.Id);

            if (project == null || string.IsNullOrEmpty(project.Id))
            {
                return Json(new { errorMsg = string.Format("Project({0}) doesn't exist", project.Id) });
            }

            //没有权限
            //if (!this.GetService<DepartmentService>().IsBoss(this.GetUserId(), dbProject.OwnerIds)
            //    && !dbProject.UserIds.Contains(this.GetUserId()))
            //{
            //    return Json(new { errorMsg = "You don't have right!" });
            //}

            try
            {
                var deleteIds = dbProject.Tasks.Select(o => o.Id).Except(project.Tasks.Select(o => o.Id).ToList()).ToList();

                if (!deleteIds.IsEmpty())
                {
                    var timeSheetService = this.GetService<TimeSheetService>();
                    var timeSheets = timeSheetService.Get(o => o.ProjectId == project.Id).ToList();
                    
                    foreach(var timeSheet in timeSheets)
                    {
                        timeSheet.DeleteTasks(deleteIds);
                        timeSheetService.Update(timeSheet);
                    }
                }

                project.UpdateProjectStatus();
                if(project.Status == Status.Done)//update actual date range when mark project done (all tasks completed)
                {
                    project.UpdateProjectActualTime();
                }
                projectService.Update(project);

                return Json(new { successMsg = string.Format("Edit project({0}) successfully!", project.Id) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(new { errorMsg = ex.Message });
            }
        }

        // PUT api/values/5
        [HttpPut]
        public void Put([FromBody]Project project)
        {
            project.Name = project.Name.Trim();
            if (project.IsPublic)
            {
                project.OwnerIds = new List<string>(new string[] { this.GetUserId() });
                project.UserIds = new List<string>(new string[] { this.GetUserId() });
            }

            this.GetService<ProjectService>().Create(project);
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public JsonResult Delete(string id)
        {
            try
            {
                this.GetService<ProjectService>().Delete(id);
                this.GetService<TimeSheetService>().Delete(o => o.ProjectId == id);
                return Json(new { successMsg = string.Format("Delete project({0}) successfully!", id) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(new { errorMsg = ex.Message });
            }
        }

        [Route("IsCp4Valid")]
        [HttpPost]
        public JsonResult IsCp4Valid(string serialNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(serialNumber))
                {
                    return Json(new {
                        valid = false,
                        message = "You don't input anything, please re-enter it"
                    });
                }

                var project = this.GetService<ProjectService>().Get().Where(o => o.SerialNumber == serialNumber).FirstOrDefault();
                return Json(new {
                    valid = project == null,
                    message = project == null ? string.Format("{0} is valid", serialNumber) : 
                    string.Format("{0} exist already, see <a target='_blank' href='/Report/ProjectOverview?projectId={1}'>Detail</a>", serialNumber, project.Id)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(new
                {
                    valid = false,
                    message = ex.Message
                });
            }
        }

        [Route("IsNameExist")]
        [HttpPost]
        public JsonResult IsNameExist(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return Json(new
                    {
                        valid = false,
                        message = "You don't input anything, please re-enter it"
                    });
                }

                name = name.Replace(" ", "");
                var project = this.GetService<ProjectService>().Get().Where(o => o.Name.Replace(" ", "") == name).FirstOrDefault();
                return Json(new
                {
                    valid = project == null,
                    message = project == null ? string.Format("{0} is valid", name) :
                    string.Format("{0} exist already, see <a target='_blank' href='/Report/ProjectOverview?projectId={1}'>Detail</a>", name, project.Id)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(new
                {
                    valid = false,
                    message = ex.Message
                });
            }
        }

        [Route("GetTasks")]
        [HttpPost]
        public JsonResult GetTasks(string groupId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var group = this.GetService<DepartmentService>().GetUserGroupById(groupId);
                var projects = this.GetService<ProjectService>().Get(o => 
                    !o.Tasks.IsEmpty() && 
                    group.OwnerIds.Intersect(o.OwnerIds).Count() > 0 &&
                    o.Status != Status.Done &&
                    o.PlanDateRange.StartDate < endDate &&
                    o.PlanDateRange.EndDate > startDate);

                var result = projects.Select(o => new {
                    ProjectId = o.Id,
                    ProjectName = o.Name,
                    ProjectDescription = o.Description,
                    Tasks = o.Tasks
                }).ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        [Route("GetTaskTemplates")]
        [HttpPost]
        public List<TaskInfo> GetTaskTemplates()
        {
            try
            {
                var groupId = this.GetService<DepartmentService>().GetUserGroupsByUserId(this.GetUserId()).FirstOrDefault().Id;
                var taskTemplate = this.GetService<TaskTemplateService>().Get(groupId);
                return taskTemplate == null ? new List<TaskInfo>() : taskTemplate.Tasks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        [Route("EditForUser")]
        [HttpPost]
        public JsonResult EditForUser([FromBody]Project project)
        {
            try
            {
                this.GetService<ProjectService>().EditForUser(project.Id, project.Name, project.Comment, project.Description, project.CodeReview);
                return Json(new { successMsg = string.Format("Edit project({0}) successfully!", project.Id) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(new { errorMsg = ex.Message });
            }
        }

        [Route("CloseProject")]
        [HttpPost]
        public JsonResult CloseProject(string projectId)
        {
            try
            {
                this.GetService<ProjectService>().CloseProject(projectId);
                return Json(new { successMsg = string.Format("Close project({0}) successfully!", projectId)});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(new { errorMsg = ex.Message });
            }
        }

        [Route("PostponeProject")]
        [HttpPost]
        public JsonResult PostponeProject(string projectId, PostponeReason postponeReason, string comment, DateTime endDate)
        {
            try
            {
                this.GetService<ProjectService>().PostponeProject(projectId, postponeReason, comment, endDate);
                return Json(new { successMsg = string.Format("PostponeProject project({0}) successfully!", projectId) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(new { errorMsg = ex.Message });
            }
        }

        [Route("GetProject")]
        [HttpPost]
        public Project GetProject(string projectId)
        {
            return this.GetService<ProjectService>().Get(projectId);
        }

        [Route("GetProjectModel")]
        [HttpPost]
        public object GetProjectModel(string projectId)
        {
            try
            {
                var project = this.GetService<ProjectService>().Get(projectId);
                return project.BuildProjectModel(this.GetUserId()).ToRow();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        [Route("GetProjectModels")]
        [HttpPost]
        public List<object> GetProjectModels(string statusText)
        {
            try
            {
                var leaderIds = this.GetService<DepartmentService>().GetLeaderIdsByUserId(this.GetUserId());
                var projects = this.GetService<ProjectService>().Get().Where(o => o.IsPublic || o.OwnerIds.Intersect(leaderIds).Count() > 0);
                projects = projects.OrderBy(o => o.IsPublic).ThenByDescending(o => o.GetEndDate()).ThenByDescending(o => o.UserIds.Contains(this.GetUserId())).ThenByDescending(o => o.Created).ToList();

                if(!string.IsNullOrEmpty(statusText))
                {
                    try
                    {
                        Status status = EnumUtil.ParseEnum<Status>(statusText);
                        projects = projects.Where(o => o.Status == status).ToList();
                    }
                    catch(Exception)
                    {

                    }
                }

                return projects.Select(o => o.BuildProjectModel(this.GetUserId()).ToRow()).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        [Route("Murmur")]
        [HttpPost]
        public JsonResult Murmur(string projectId, long tick, string userId, string content)
        {
            try
            {
                var result = this.GetService<ProjectService>().Murmur(projectId, tick, userId, content);
                return Json(new {
                    Id = result.ToString(),
                    Time = new DateTime(result)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(new { errorMsg = ex.Message });
            }
        }

        
        [Route("DeleteMurmur")]
        [HttpPost]
        public void DeleteMurmur(string projectId, long tick, string userId, string content)
        {
            try
            {
                this.GetService<ProjectService>().DeleteMurmur(projectId, tick);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        [Route("GetWorkingHour")]
        [HttpPost]
        public double GetWorkingHour(string userId, DateTime startDate, DateTime endDate)
        {
            try
            {
                return Math.Round(this.GetService<TimeSheetService>().GetUserHoursByProjectId(userId, startDate, endDate).Sum(o => o.Value));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return 0;
            }
        }

        [Route("GetPlanHour")]
        [HttpPost]
        public int GetPlanHour(string userId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var hours = this.GetService<ProjectService>().GetPlanHoursByProject(userId, startDate, endDate);
                return (int)Math.Round(hours.Sum(o => o.Value));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return 0;
            }
        }
    }
}
