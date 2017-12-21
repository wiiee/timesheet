namespace Web.API
{
    using Common;
    using Entity.User;
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
    public class UserPerformanceController : BaseController
    {
        private static ILogger _logger = LoggerUtil.CreateLogger<UserPerformanceController>();

        // GET: api/user
        [Route("GetUserNames")]
        [HttpPost]
        public Dictionary<string, string> GetUserNames()
        {
            var users = this.GetService<UserService>().Get();
            var entities = users.Select(
                c => new
                {
                    id = c.Id,
                    //password = c.Password,
                    name = c.Name,
                    //adminTypes = c.UserType.GetEnumsWithName()
                }
            ).ToDictionary(key => key.id, value => value.name);

            return entities;
        }

        // POST api/values
        [HttpPost]
        public JsonResult Post([FromBody]User user)
        {
            string msg = "Action: Edit" + "; UserId: " + user.Id;

            try
            {
                this.GetService<UserService>().Update(user);
            }
            catch (Exception ex)
            {
                msg += "; Error:" + ex.Message;
            }


            object json = new
            {
                status = Result.Success,
                msg = msg
            };

            return Json(json);
        }

        // PUT api/values
        [HttpPut]
        public JsonResult Put([FromBody]User user)
        {
            try { 
                return Json(new { successMsg = string.Format("Edit project({0}:{1}) successfully!")});
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
            return new PerformanceItem();
        }

        [Route("Pull")]
        [HttpPost]
        public PerformanceItem Pull(DateRange dateRange)
        {
            var userGroup = this.GetService<DepartmentService>().GetUserGroupsByOwnerId(this.GetUserId()).FirstOrDefault();

            if (userGroup != null)
            {
                var item = this.GetService<UserPerformanceService>().Get(userGroup.Id);

                if (item != null)
                {
                    return null;
                }
            }

            return new List<PerformanceItem>();
        }
    }
}
