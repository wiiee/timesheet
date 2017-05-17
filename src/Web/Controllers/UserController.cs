namespace Web.Controllers
{
    using Common;
    using Entity.User;
    using Extension;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Model;
    using Platform.Enum;
    using Service.User;
    using System;
    using System.Collections.Generic;
    using Platform.Extension;
    using System.Linq;

    [Authorize]
    public class UserController : BaseController
    {
        // GET: /<controller>/
        public IActionResult Index(string successMsg, string errorMsg)
        {
            var departmentService = this.GetService<DepartmentService>();
            this.BuildHeaderMsg(successMsg, errorMsg);

            ViewData["Users"] = ConvertUserToUserModel(this.GetService<UserService>().GetByIds(departmentService.GetSubordinatesByUserId(this.GetUserId())));

            return View();
        }

        public IActionResult Profile(string id)
        {
            ViewData["User"] = BuildUserProfileModel(id);
            return View();
        }

        private UserProfileModel BuildUserProfileModel(string id)
        {
            var profile = this.GetService<ProfileService>().Get(id);
            var userService = this.GetService<UserService>();

            var thumbsUpNames = new List<string>();
            var thumbsDownNames = new List<string>();
            var comments = new List<CommentInfo>();

            if(profile != null)
            {
                if (!profile.ThumbsUpIds.IsEmpty())
                {
                    thumbsUpNames.AddRange(userService.GetByIds(profile.ThumbsUpIds).Select(o => o.Name).ToList());
                }

                if (!profile.ThumbsDownIds.IsEmpty())
                {
                    thumbsDownNames.AddRange(userService.GetByIds(profile.ThumbsDownIds).Select(o => o.Name).ToList());
                }

                if (!profile.Comments.IsEmpty())
                {
                    comments = profile.Comments.SelectMany(o => o.Value.Select(p => new CommentInfo(userService.Get(o.Key).Name, p.Key, p.Value.ToLocalTime()))).OrderByDescending(o => o.Time).ToList();
                }
            }

            return new UserProfileModel(id, thumbsUpNames, thumbsDownNames, comments);
        }

        private List<UserModel> ConvertUserToUserModel(List<User> users)
        {
            var result = new List<UserModel>();

            foreach(var item in users)
            {
                bool isEdit = false;
                bool isDelete = false;
                bool isResetPassword = false;

                if(this.GetUserType() == UserType.Admin || this.GetUserType() == UserType.Manager)
                {
                    isEdit = true;
                    isDelete = true;
                    isResetPassword = true;
                }
                else if(this.GetUserType() == UserType.Leader || this.GetUserId() == item.Id)
                {
                    isEdit = true;

                    if(this.GetUserId() == item.Id)
                    {
                        isResetPassword = true;
                    }
                }

                result.Add(new UserModel(item.Id, item.NickName, item.Name, item.MobileNo, item.Gender, item.UserType, item.AccountType, isEdit, isDelete, isResetPassword));
            }

            return result;
        }

        [Authorize(Roles = "0,1,3")]
        public IActionResult AddUser([FromForm]User user)
        {
            if (user != null)
            {
                var userService = this.GetService<UserService>();

                string successMsg = string.Empty;
                string errorMsg = string.Empty;

                try
                {
                    user.Id = user.Id.Trim();
                    user.Name = user.Name.Trim();
                    userService.Create(user);
                    successMsg = string.Format("Add user({0}) successfully!", user.Id);
                }
                catch (Exception ex)
                {
                    errorMsg = ex.Message;
                }

                return RedirectUser(successMsg, errorMsg);
            }
            else
            {
                return View();
            }
        }

        public IActionResult EditUser(string id, User user)
        {
            var departmentService = this.GetService<DepartmentService>();
            var userService = this.GetService<UserService>();

            string successMsg = string.Empty;
            string errorMsg = string.Empty;

            var dbUser = userService.Get(id);

            if (dbUser == null)
            {
                errorMsg = id + "doesn't exist!";
                return RedirectUser(successMsg, errorMsg);
            }

            //没有编辑用户权限
            if (!departmentService.IsBoss(this.GetUserId(), id))
            {
                errorMsg = "You don't have right!";
                return RedirectUser(successMsg, errorMsg);
            }

            if (user == null || string.IsNullOrEmpty(user.Name))
            {
                ViewData["User"] = dbUser;
                return View();
            }
            else
            {
                try
                {
                    user.Id = id;
                    user.Password = dbUser.Password;

                    if (this.GetUserType() != UserType.Admin)
                    {
                        user.UserType = dbUser.UserType;
                    }

                    userService.Update(user);
                    successMsg = string.Format("Edit user({0}) successfully!", id);
                }
                catch (Exception ex)
                {
                    errorMsg = ex.Message;
                }

                return RedirectUser(successMsg, errorMsg);
            }
        }

        [Authorize(Roles = "0,3")]
        public IActionResult DeleteUser(string id)
        {
            var departmentService = this.GetService<DepartmentService>();
            var userService = this.GetService<UserService>();

            string successMsg = string.Empty;
            string errorMsg = string.Empty;

            var dbUser = userService.Get(id);

            if (dbUser == null)
            {
                errorMsg = id + "doesn't exist!";
                return RedirectUser(successMsg, errorMsg);
            }

            if (this.GetUserType() == UserType.Manager && !departmentService.GetSubordinatesByUserId(this.GetUserId()).Contains(id))
            {
                errorMsg = "You don't have right!";
                return RedirectUser(successMsg, errorMsg);
            }

            try
            {
                userService.Delete(id);
                successMsg = string.Format("Delete user({0}) successfully!", id);
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }

            return RedirectUser(successMsg, errorMsg);
        }

        private bool IsEditable(UserType userType, string userId, string id)
        {
            var userService = this.GetService<UserService>();
            var departmentService = this.GetService<DepartmentService>();

            if (userType == UserType.Admin)
            {
                return true;
            }
            else if (userType == UserType.Manager && departmentService.IsBoss(userId, id))
            {
                return true;
            }
            else if (userId == id)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public IActionResult ResetPassword(string id, string oldPassword, string newPassword)
        {
            string successMsg = string.Empty;
            string errorMsg = string.Empty;

            var userService = this.GetService<UserService>();

            var dbUser = userService.Get(id);

            if (dbUser == null)
            {
                errorMsg = id + "doesn't exist!";
                return RedirectUser(successMsg, errorMsg);
            }

            var userType = this.GetUserType();
            var userId = this.GetUserId();
            
            //没有编辑用户权限

            if(!IsEditable(userType, userId, id))
            {
                errorMsg = "You don't have right to reset password.";
                return RedirectUser(successMsg, errorMsg);
            }

            if(string.IsNullOrEmpty(oldPassword))
            {
                ViewData["Id"] = id;
                return View();
            }
            else
            {
                if (userType != UserType.Admin 
                    && userType != UserType.Manager 
                    && dbUser.Password != oldPassword)
                {
                    errorMsg = "Your password is not correct!";
                }
                else
                {
                    try
                    {
                        dbUser.Password = newPassword;
                        userService.Update(dbUser);
                        successMsg = string.Format("Reset password with user({0}) successfully!", id);
                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.Message;
                    }
                }

                return RedirectUser(successMsg, errorMsg);
            }
        }

        private IActionResult RedirectUser(string successMsg, string errorMsg)
        {
            return RedirectToAction("Index", new { successMsg = successMsg, errorMsg = errorMsg });
        }
    }
}
