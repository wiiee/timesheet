namespace Web.API
{
    using Common;
    using Entity.ValueType;
    using Extension;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Platform.Extension;
    using Platform.Util;
    using Service.Project;
    using Service.User;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [Authorize]
    [Route("api/[controller]")]
    public class UserPerformanceController : BaseController
    {
        private static ILogger _logger = LoggerUtil.CreateLogger<UserPerformanceController>();

        // PUT api/values
        [Route("Save")]
        [HttpPost]
        public JsonResult Save([FromBody]PerformanceItem item)
        {
            try {
                var userGroup = this.GetService<DepartmentService>().GetUserGroupsByOwnerId(this.GetUserId()).FirstOrDefault();
                this.GetService<UserPerformanceService>().Update(userGroup.Id, item);
                return Json(new { successMsg = string.Format("Save performance successfully!")});
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(new { errorMsg = ex.Message});
            }
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(string id)
        {
            //删除用户
            this.GetService<UserService>().Delete(id);
        }

        [Route("GetItems")]
        [HttpPost]
        public List<PerformanceItem> GetItems()
        {
            var userGroup = this.GetService<DepartmentService>().GetUserGroupsByOwnerId(this.GetUserId()).FirstOrDefault();

            if(userGroup != null)
            {
                var item = this.GetService<UserPerformanceService>().Get(userGroup.Id);

                if(item != null)
                {
                    return item.Items;
                }
            }

            return new List<PerformanceItem>();
        }

        [Route("GetSample")]
        [HttpPost]
        public PerformanceItem GetSample()
        {
            var result = new PerformanceItem();

            var userGroup = this.GetService<DepartmentService>().GetUserGroupsByOwnerId(this.GetUserId()).FirstOrDefault();

            if (userGroup != null)
            {
                var levels = this.GetService<UserService>().GetByIds(userGroup.UserIds.Where(o => o != this.GetUserId()).ToList())
                 .ToDictionary(o => o.Id, o => o.Level);

                foreach(var level in levels){
                    result.Values.Add(level.Key, new Score(level.Value));
                }
            }

            return result;
        }

        [Route("Pull")]
        [HttpPost]
        public PerformanceItem Pull([FromBody]PerformanceItem item)
        {
            var userGroup = this.GetService<DepartmentService>().GetUserGroupsByOwnerId(this.GetUserId()).FirstOrDefault();

            if(userGroup != null)
            {
                foreach (var userId in userGroup.UserIds.Where(o => o != this.GetUserId()).ToList())
                {
                    item.Values[userId].TimeSheetValue = this.GetService<TimeSheetService>().
                        GetContribution(userId, item.DateRange.StartDate, item.DateRange.EndDate);
                }   
            }

            return item;
        }

        [Route("Calculate")]
        [HttpPost]
        public PerformanceItem Calculate([FromBody]PerformanceItem item)
        {
            item.Calculate();
            return item;
        }
    }
}
