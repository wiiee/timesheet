namespace Web.API
{
    using Common;
    using Entity.Project.Scrum;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Platform.Util;
    using Service.Extension;
    using Service.Scrum;
    using System;
    using System.Linq;

    [Authorize]
    [Route("api/[controller]")]
    public class ScrumController : BaseController
    {
        private static ILogger _logger = LoggerUtil.CreateLogger<ProjectController>();

        [Route("GetSprint")]
        [HttpPost]
        public Sprint GetSprint(string sprintId)
        {
            try
            {
                return this.GetService<SprintService>().Get(sprintId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        [Route("GetSprints")]
        [HttpPost]
        public JsonResult GetSprints(string groupId)
        {
            try
            {
                var sprints = this.GetService<SprintService>().Get(o => o.GroupId == groupId);

                var result = sprints.Select(o => new {
                    Id = o.Id,
                    Name = o.Name,
                    DateRange = o.DateRange,
                    Status = o.GetStatus().ToString()
                });

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        [Route("DeleteSprint")]
        [HttpPost]
        public JsonResult DeleteSprint(string sprintId)
        {
            try
            {
                this.GetService<SprintService>().Delete(sprintId);
                return Json(new { successMsg = string.Format("Delete sprint({0}) successfully!", sprintId) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(new { errorMsg = ex.Message });
            }
        }
    }
}
