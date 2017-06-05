namespace Web.Controllers
{
    using Common;
    using Entity.Project;
    using Entity.User;
    using Extension;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Model;
    using Platform.Enum;
    using Platform.Extension;
    using Platform.Util;
    using Service.Project;
    using Service.User;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [Authorize]
    public class ProjectController : BaseController
    {
        private static ILogger _logger = LoggerUtil.CreateLogger<ProjectController>();

        public IActionResult Index(string successMsg, string errorMsg)
        {
            this.BuildHeaderMsg(successMsg, errorMsg);
            //BuildProject(successMsg, errorMsg);
            return View();
        }

        public IActionResult TaskTemplate(TaskTemplate taskTemplate)
        {
            var taskTemplateService = this.GetService<TaskTemplateService>();
            var groupId = this.GetService<DepartmentService>().GetUserGroupsByUserId(this.GetUserId()).FirstOrDefault().Id;

            if (taskTemplate.Tasks.IsEmpty() && string.IsNullOrEmpty(taskTemplate.Id))
            {
                var dbTaskTemplate = taskTemplateService.Get(groupId);

                ViewData["TaskTemplate"] = dbTaskTemplate;
                return View();
            }
            else
            {
                if (string.IsNullOrEmpty(taskTemplate.Id))
                {
                    taskTemplate.Id = groupId;
                    taskTemplateService.Create(taskTemplate);
                }
                else
                {
                    taskTemplateService.Update(taskTemplate.Id, "Tasks", taskTemplate.Tasks);
                }
                
                var successMsg = "Save template successfully!";

                ViewData["TaskTemplate"] = taskTemplate;

                this.BuildHeaderMsg(successMsg, string.Empty);

                return View();
            }
        }

        private List<string> getUserIds()
        {
            List<string> userIds = null;

            if (this.GetUserType() == UserType.User)
            {
                userIds = this.GetService<DepartmentService>().GetLeaderIdsByUserId(this.GetUserId());
            }
            else
            {
                userIds = new List<string>(new string[] { this.GetUserId() });
            }
            return userIds;
        }

        private void BuildProject(string successMsg, string errorMsg)
        {
            var projectService = this.GetService<ProjectService>();

            this.BuildHeaderMsg(successMsg, errorMsg);

            var projects = projectService.GetProjectsByUserIds(getUserIds());

            //更新项目状态，到期自动关闭项目
            foreach (var project in projects)
            {
                if (project.Tasks.IsEmpty())
                {
                    if (project.Status != Status.Done && project.PlanDateRange.EndDate < DateTime.Today)
                    {
                        projectService.CloseProject(project.Id, project.PlanDateRange.EndDate);
                    }
                }
            }

            ViewData["Projects"] = BuildProjectModels(projects);
        }

        private List<ProjectModel> BuildProjectModels(List<Project> projects)
        {
            List<ProjectModel> models = new List<ProjectModel>();

            projects = projects.OrderBy(o => o.IsPublic).ThenByDescending(o => o.UserIds.Contains(this.GetUserId())).ThenByDescending(o => o.PlanDateRange.EndDate).ToList();

            foreach (var item in projects)
            {
                models.Add(item.BuildProjectModel(this.GetUserId()));
            }

            return models;
        }

        public IActionResult Project(string projectId)
        {
            ViewData["ProjectId"] = projectId;
            return View();
        }

        [Authorize(Roles = "0,1,2,3")]
        public IActionResult PostponeProject(string id, PostponeReason postponeReason, string comment, DateTime endDate)
        {
            var projectService = this.GetService<ProjectService>();
            var departmentService = this.GetService<DepartmentService>();

            string successMsg = string.Empty;
            string errorMsg = string.Empty;

            var dbProject = projectService.Get(id);

            if (dbProject == null)
            {
                errorMsg = id + "doesn't exist!";
                return RedirectProject(successMsg, errorMsg);
            }

            //没有权限
            if (!departmentService.IsBoss(this.GetUserId(), dbProject.OwnerIds))
            {
                errorMsg = "You don't have right!";
                return RedirectProject(successMsg, errorMsg);
            }

            if (!endDate.IsEmpty())
            {
                try
                {
                    projectService.PostponeProject(id, postponeReason, comment, endDate);
                    successMsg = string.Format("Open project({0}) successfully!", id);
                }
                catch (Exception ex)
                {
                    errorMsg = ex.Message;
                }

                return RedirectProject(successMsg, errorMsg);
            }
            else
            {
                ViewData["Project"] = dbProject;
                return View();
            }
        }

        private void BuildOwnersAndUsers(List<User> projectOwners)
        {
            var userService = this.GetService<UserService>(); ;
            var departmentService = this.GetService<DepartmentService>();

            if (projectOwners == null)
            {
                if(this.GetUserType() == UserType.User)
                {
                    projectOwners = new List<User>();
                    projectOwners.AddRange(userService.GetByIds(departmentService.GetLeaderIdsByUserId(this.GetUserId())));
                }
                else
                {
                    projectOwners = new List<User>(new User[] { userService.Get(this.GetUserId()) });
                }
            }

            ViewData["Owners"] =  this.GetService<DepartmentService>().GetOwnersByUserIds(projectOwners.Select(o => o.Id).ToList());
            ViewData["Users"] = this.GetService<DepartmentService>().GetUsersByUserIds(projectOwners.Select(o => o.Id).ToList());
            ViewData["UserGroups"] = this.GetService<DepartmentService>().GetUserGroupModelsByUserId(projectOwners.Select(o => o.Id).ToList().First());
        }

        [Authorize(Roles = "0,1,3")]
        public IActionResult DeleteProject(string id)
        {
            var departmentService = this.GetService<DepartmentService>();
            var projectService = this.GetService<ProjectService>();
            var timeSheetService = this.GetService<TimeSheetService>();

            string successMsg = string.Empty;
            string errorMsg = string.Empty;

            var dbProject = projectService.Get(id);
            if (dbProject == null)
            {
                errorMsg = id + "doesn't exist!";
                return RedirectProject(successMsg, errorMsg);
            }

            //没有权限
            if (!departmentService.IsBoss(this.GetUserId(), dbProject.OwnerIds))
            {
                errorMsg = "You don't have right!";
                return RedirectProject(successMsg, errorMsg);
            }

            try
            {
                projectService.Delete(id);
                timeSheetService.Delete(o => o.ProjectId == id);
                successMsg = string.Format("Delete project({0}) successfully!", id);
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }

            return RedirectProject(successMsg, errorMsg);
        }

        private IActionResult RedirectProject(string successMsg, string errorMsg)
        {
            return RedirectToAction("Index", new { successMsg = successMsg, errorMsg = errorMsg });
        }
    }
}
