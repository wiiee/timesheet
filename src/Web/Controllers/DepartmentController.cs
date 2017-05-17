namespace Web.Controllers
{
    using Common;
    using Entity.User;
    using Entity.ValueType;
    using Extension;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Model.Department;
    using Platform.Enum;
    using Platform.Extension;
    using Service.User;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    [Authorize]
    public class DepartmentController : BaseController
    {
        // GET: /<controller>/
        public IActionResult Index(string successMsg, string errorMsg)
        {
            BuildDepartmentModel(successMsg, errorMsg);
            return View();
        }

        private void BuildDepartmentModel(string successMsg, string errorMsg)
        {
            this.BuildHeaderMsg(successMsg, errorMsg);
            var departments = this.GetService<DepartmentService>().GetDepartmentsByUserId(this.GetUserId());
            var user = this.GetService<UserService>().Get(this.GetUserId());

            var result = new List<DepartmentModel>();

            foreach (var item in departments)
            {
                result.Add(item.Convert(user));
            }

            ViewData["Departments"] = result;
        }

        [Authorize(Roles = "0")]
        public IActionResult AddDepartment(Department department)
        {
            if (department != null && !string.IsNullOrEmpty(department.Id))
            {
                string successMsg = string.Empty;
                string errorMsg = string.Empty;

                try
                {
                    department.Id = department.Id.Trim();
                    department.UserGroups = new Dictionary<string, UserGroup>();
                    this.GetService<DepartmentService>().Create(department);
                    successMsg = string.Format("Add department({0}) successfully!", department.Id);
                }
                catch (Exception ex)
                {
                    errorMsg = ex.Message;
                }

                return RedirectDepartment(successMsg, errorMsg);
            }
            else
            {
                ViewData["Owners"] = this.GetService<UserService>().Get(o => o.UserType == UserType.Manager);
                ViewData["UserGroups"] = this.GetService<DepartmentService>().GetUserGroupsWithList();
                return View();
            }
        }

        [Authorize(Roles = "0,3")]
        public IActionResult EditDepartment(string id, HashSet<string> OwnerIds, HashSet<string> userGroupIds)
        {
            string successMsg = string.Empty;
            string errorMsg = string.Empty;


            var dbDepartment = this.GetService<DepartmentService>().Get(id);

            if (dbDepartment == null)
            {
                errorMsg = id + "doesn't exist!";
                return RedirectDepartment(successMsg, errorMsg);
            }

            if (!this.GetService<DepartmentService>().IsBoss(this.GetUserId(), dbDepartment.OwnerIds))
            {
                errorMsg = "You don't have right!";
                return RedirectDepartment(successMsg, errorMsg);
            }

            if (OwnerIds.IsEmpty())
            {
                ViewData["Department"] = dbDepartment;
                ViewData["Owners"] = this.GetService<UserService>().Get(o => o.UserType == UserType.Manager);
                ViewData["UserGroups"] = this.GetService<DepartmentService>().GetUserGroupsByOwnerId(this.GetUserId());
                return View();
            }
            else
            {
                try
                {
                    dbDepartment.OwnerIds = OwnerIds;
                    var removeUserGroupIds = dbDepartment.UserGroups.Keys.Except(userGroupIds).ToList();

                    foreach(var item in removeUserGroupIds)
                    {
                        dbDepartment.UserGroups.Remove(item);
                    }

                    this.GetService<DepartmentService>().Update(dbDepartment);
                    successMsg = string.Format("Edit department({0}) successfully!", id);
                }
                catch (Exception ex)
                {
                    errorMsg = ex.Message;
                }

                return RedirectDepartment(successMsg, errorMsg);
            }
        }

        [Authorize(Roles = "0,3")]
        public IActionResult DeleteDepartment(string id)
        {
            string successMsg = string.Empty;
            string errorMsg = string.Empty;

            var dbDepartment = this.GetService<DepartmentService>().Get(id);

            if (dbDepartment == null)
            {
                errorMsg = id + "doesn't exist!";
                return RedirectDepartment(successMsg, errorMsg);
            }

            if (!this.GetService<DepartmentService>().IsBoss(this.GetUserId(), dbDepartment.OwnerIds))
            {
                errorMsg = "You don't have right!";
                return RedirectDepartment(successMsg, errorMsg);
            }

            try
            {
                this.GetService<DepartmentService>().Delete(id);
                successMsg = string.Format("Delete department({0}) successfully!", id);
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }

            return RedirectDepartment(successMsg, errorMsg);
        }

        [Authorize(Roles = "0,3")]
        public IActionResult AddGroup(UserGroup userGroup)
        {
            var userService = this.GetService<UserService>();
            var departmentService = this.GetService<DepartmentService>();

            if (userGroup != null && !string.IsNullOrEmpty(userGroup.Name))
            {
                string successMsg = string.Empty;
                string errorMsg = string.Empty;

                try
                {
                    departmentService.AddUserGroup(userGroup);
                    successMsg = string.Format("Add group({0}) successfully!", userGroup.Id);
                }
                catch (Exception ex)
                {
                    errorMsg = ex.Message;
                }

                return RedirectDepartment(successMsg, errorMsg);
            }
            else
            {
                if(this.GetUserType() == UserType.Admin)
                {
                    ViewData["DepartmentIds"] = departmentService.Get().Select(o => o.Id).ToList();
                }
                else
                {
                    ViewData["DepartmentIds"] = departmentService.Get(o => o.OwnerIds.Contains(this.GetUserId())).Select(o => o.Id).ToList();
                }

                ViewData["Owners"] = userService.Get(o => o.UserType == UserType.Leader || o.UserType == UserType.Manager);
                ViewData["Users"] = userService.Get(o => o.UserType != UserType.Admin);
                return View();
            }
        }

        [Authorize(Roles = "0,1,3")]
        public IActionResult EditGroup(string id, UserGroup userGroup)
        {
            string successMsg = string.Empty;
            string errorMsg = string.Empty;

            var userService = this.GetService<UserService>();
            var departmentService = this.GetService<DepartmentService>();

            var dbUserGroup = departmentService.GetUserGroupById(id);

            if (dbUserGroup == null)
            {
                errorMsg = id + "doesn't exist!";
                return RedirectDepartment(successMsg, errorMsg);
            }

            if (!departmentService.IsBoss(this.GetUserId(), dbUserGroup.OwnerIds))
            {
                errorMsg = "You don't have right!";
                return RedirectDepartment(successMsg, errorMsg);
            }

            if (userGroup == null || string.IsNullOrEmpty(userGroup.Name))
            {
                ViewData["UserGroup"] = dbUserGroup;
                ViewData["Owners"] = userService.Get(o => o.UserType == UserType.Leader || o.UserType == UserType.Manager);
                ViewData["Users"] = userService.Get(o => o.UserType != UserType.Admin);
                return View();
            }
            else
            {
                try
                {
                    departmentService.UpdateUserGroup(userGroup);
                    successMsg = string.Format("Edit group({0}) successfully!", id);
                }
                catch (Exception ex)
                {
                    errorMsg = ex.Message;
                }

                return RedirectDepartment(successMsg, errorMsg);
            }
        }

        [Authorize(Roles = "0,3")]
        public IActionResult DeleteGroup(string id)
        {
            var userService = this.GetService<UserService>();
            var departmentService = this.GetService<DepartmentService>();

            string successMsg = string.Empty;
            string errorMsg = string.Empty;

            var dbUserGroup = departmentService.GetUserGroupById(id);

            if (dbUserGroup == null)
            {
                errorMsg = id + "doesn't exist!";
                return RedirectDepartment(successMsg, errorMsg);
            }

            if (!departmentService.IsBoss(this.GetUserId(), dbUserGroup.OwnerIds))
            {
                errorMsg = "You don't have right!";
                return RedirectDepartment(successMsg, errorMsg);
            }

            try
            {
                departmentService.DeleteUserGroup(id);
                successMsg = string.Format("Delete group({0}) successfully!", id);
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }

            return RedirectDepartment(successMsg, errorMsg);
        }

        private IActionResult RedirectDepartment(string successMsg, string errorMsg)
        {
            return RedirectToAction("Index", new { successMsg = successMsg, errorMsg = errorMsg });
        }
    }
}
