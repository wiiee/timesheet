namespace Web.API
{
    using Common;
    using Entity.User;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Platform.Enum;
    using Platform.Extension;
    using Platform.Util;
    using Service.Model;
    using Service.Project;
    using Service.Result;
    using Service.User;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Extension;

    [Authorize]
    [Route("api/[controller]")]
    public class UserController : BaseController
    {
        private static ILogger _logger = LoggerUtil.CreateLogger<UserController>();

        // GET: api/user
        public JsonResult Get()
        {
            var users = this.GetService<UserService>().Get();
            var entities = users.Select(
                c => new
                {
                    id = c.Id,
                    password = c.Password,
                    name = c.Name,
                    adminTypes = c.UserType.GetEnumsWithName()
                }
            );

            object json = new
            {
                status = Result.Success,
                data = entities
            };

            return Json(json);
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
            string msg = "Action: Edit" + "; UserId: " + user.Id;
            bool status = false;

            UserServiceResult result = null;

            try
            {
                result = this.GetService<UserService>().SignUp(user);
                status = true;
            }
            catch (Exception ex)
            {
                msg += "; Error:" + ex.Message;
            }

            if (result.Status == Result.Failure)
            {
                msg += "; Error:" + result.Message;
            }

            object json = new
            {
                status = status,
                msg = msg
            };

            return Json(json);
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(string id)
        {
            //删除用户
            this.GetService<UserService>().Delete(id);
        }

        [Route("GetOwners")]
        [HttpPost]
        public List<KeyValuePair<string, string>> GetOwners(string projectId)
        {
            try
            {
                var userService = this.GetService<UserService>(); ;
                var departmentService = this.GetService<DepartmentService>();
                var projectService = this.GetService<ProjectService>();

                if (string.IsNullOrEmpty(projectId))
                {
                    var owners = new List<User>();

                    if (this.GetUserType() == UserType.User)
                    {
                        owners.AddRange(userService.GetByIds(departmentService.GetLeaderIdsByUserId(this.GetUserId())));
                    }
                    else
                    {
                        owners.Add(userService.Get(this.GetUserId()));
                    }

                    return departmentService.GetOwnersByUserIds(owners.Select(o => o.Id).ToList()).Select(o => new KeyValuePair<string, string>(o.Id, o.Name)).ToList();
                }

                var project = projectService.Get(projectId);
                var ownerIds = new List<string>(project.OwnerIds);

                if (this.GetUserType() != UserType.User)
                {
                    ownerIds.Add(this.GetUserId());
                }

                return departmentService.GetOwnersByUserIds(ownerIds.Distinct().ToList()).Select(o => new KeyValuePair<string, string>(o.Id, o.Name)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        [Route("GetUsers")]
        [HttpPost]
        public List<KeyValuePair<string, string>> GetUsers(string projectId)
        {
            try
            {
                var userService = this.GetService<UserService>(); ;
                var departmentService = this.GetService<DepartmentService>();
                var projectService = this.GetService<ProjectService>();

                if (string.IsNullOrEmpty(projectId))
                {
                    var owners = new List<User>();

                    if (this.GetUserType() == UserType.User)
                    {
                        owners.AddRange(userService.GetByIds(departmentService.GetLeaderIdsByUserId(this.GetUserId())));
                    }
                    else
                    {
                        owners.Add(userService.Get(this.GetUserId()));
                    }

                    return departmentService.GetUsersByUserIds(owners.Select(o => o.Id).ToList()).Select(o => new KeyValuePair<string, string>(o.Id, o.Name)).ToList();
                }

                var project = projectService.Get(projectId);
                var ownerIds = new List<string>(project.OwnerIds);

                if (this.GetUserType() != UserType.User)
                {
                    ownerIds.Add(this.GetUserId());
                }

                return departmentService.GetUsersByUserIds(ownerIds.Distinct().ToList()).Select(o => new KeyValuePair<string, string>(o.Id, o.Name)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        [Route("GetGroups")]
        [HttpPost]
        public List<GroupModel> GetGroups(string projectId)
        {
            try
            {
                var userService = this.GetService<UserService>(); ;
                var departmentService = this.GetService<DepartmentService>();
                var projectService = this.GetService<ProjectService>();

                if (string.IsNullOrEmpty(projectId))
                {
                    return departmentService.GetUserGroupModelsByUserId(this.GetUserId());
                }

                var project = projectService.Get(projectId);
                var ownerIds = new List<string>(project.OwnerIds);

                if (this.GetUserType() != UserType.User)
                {
                    ownerIds.Add(this.GetUserId());
                }

                return departmentService.GetUserGroupModelsByOwnerIds(ownerIds.Distinct().ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return null;
            }
        }

        [Route("AddComment")]
        [HttpPost]
        public void AddComment(string userId, string comment, string commentUserId)
        {
            if (!string.IsNullOrEmpty(comment) && this.GetService<UserService>().Get(commentUserId) != null)
            {
                this.GetService<ProfileService>().AddComment(userId, comment, commentUserId);
            }
        }

        [Route("ThumbsUp")]
        [HttpPost]
        public void ThumbsUp(string userId, string commentUserId)
        {
            if(this.GetService<UserService>().Get(commentUserId) != null)
            {
                this.GetService<ProfileService>().ThumbsUp(userId, commentUserId);
            }
        }

        [Route("ThumbsDown")]
        [HttpPost]
        public void ThumbsDown(string userId, string commentUserId)
        {
            if (this.GetService<UserService>().Get(commentUserId) != null)
            {
                this.GetService<ProfileService>().ThumbsDown(userId, commentUserId);
            }
        }
    }
}
