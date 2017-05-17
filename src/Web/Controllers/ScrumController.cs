namespace Web.Controllers
{
    using Common;
    using Entity.Project.Scrum;
    using Extension;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Model;
    using Platform.Enum;
    using Service.Project;
    using Service.Scrum;
    using Service.User;
    using System;
    using System.Collections.Generic;
    using System.Linq;


    [Authorize]
    public class ScrumController : BaseController
    {
        // GET: /<controller>/
        public IActionResult Index(string groupId, string successMsg, string errorMsg)
        {
            this.BuildHeaderMsg(successMsg, errorMsg);
            var departmentService = this.GetService<DepartmentService>();

            if (string.IsNullOrEmpty(groupId))
            {
                if(this.GetUserType() == UserType.Admin || this.GetUserType() == UserType.Manager)
                {
                    var groups = departmentService.GetUserGroupsByUserId(this.GetUserId());

                    ViewData["GroupPairs"] = groups.Select(o => new KeyValuePair<string, string>(o.Id, o.Name)).ToList();

                    return View("Sprints/Groups");
                }

                groupId = departmentService.GetUserGroupsByUserId(this.GetUserId()).FirstOrDefault().Id;
            }

            ViewData["GroupId"] = groupId;

            return View("Sprints/Group");
        }

        public IActionResult Overview(string sprintId)
        {
            var projectService = this.GetService<ProjectService>();
            var userService = this.GetService<UserService>();
            var sprint = this.GetService<SprintService>().Get(sprintId);

            if(sprint != null)
            {
                var tasks = new List<TaskModel>();
                
                foreach(var item in sprint.TaskIds)
                {
                    var project = projectService.Get(item.Key);
                    var task = project.Tasks.Find(o => o.Name == item.Value);
                    tasks.Add(new TaskModel(project.Id, project.Name, userService.Get(task.UserId).Name, task));
                }

                ViewData["Tasks"] = tasks;

                return View();
            }
            else
            {
                return RedirectScrum("", sprintId + " is invalid, please check it.");
            }
        }

        public IActionResult Sprint(Sprint sprint)
        {
            if (sprint != null && !string.IsNullOrEmpty(sprint.Name))
            {
                sprint.Name = sprint.Name.Trim();

                var sprintService = this.GetService<SprintService>();
                string successMsg = string.Empty;
                string errorMsg = string.Empty;

                //新建
                if (string.IsNullOrEmpty(sprint.Id))
                {
                    sprint.Id = sprint.GroupId + "_" + sprint.Name;

                    try
                    {
                        sprintService.Create(sprint);
                        successMsg = string.Format("Create project({0}) successfully!", sprint.Id);
                    }
                    catch(Exception ex)
                    {
                        errorMsg = ex.Message;
                    }
                }
                else
                {
                    try
                    {
                        sprintService.Update(sprint);
                        successMsg = string.Format("Update project({0}) successfully!", sprint.Id);
                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.Message;
                    }
                }

                return RedirectScrum(successMsg, errorMsg);
            }
            //
            else
            {
                if (!string.IsNullOrEmpty(sprint.Id))
                {
                    ViewData["SprintId"] = sprint.Id;
                }

                ViewData["GroupId"] = this.GetService<DepartmentService>().GetUserGroupsByUserId(this.GetUserId()).FirstOrDefault().Id;
            }

            return View();
        }

        private IActionResult RedirectScrum(string successMsg, string errorMsg)
        {
            return RedirectToAction("Index", new { successMsg = successMsg, errorMsg = errorMsg });
        }
    }
}
