namespace Web.Controllers
{
    using Common;
    using Entity.User;
    using Microsoft.AspNetCore.Mvc;
    using Platform.Enum;
    using Platform.Util;
    using Service.User;
    using System.Collections.Generic;
    using System.Net;
    using System.Security.Claims;

    public class AccountController : BaseController
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return new RedirectResult("~/Home/Index?errorMsg=" + WebUtility.UrlEncode("You don't have access to the page previously!"));
        }

        public IActionResult SignUp(User user, string returnUrl)
        {
            if (user != null && !string.IsNullOrEmpty(user.Id))
            {
                user.Id = user.Id.Trim();

                if (ModelState.IsValid)
                {
                    var result = this.GetService<UserService>().SignUp(user);

                    if (result.Status == Result.Success)
                    {
                        LogIn(result.User);

                        //如果是注册或者登陆页面，跳转到首页
                        if (returnUrl != null &&
                            (returnUrl.ToLower().Contains("account/login") ||
                            returnUrl.ToLower().Contains("account/signup")))
                        {
                            returnUrl = null;
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMsg = result.Message;
                        ViewBag.ReturnUrl = returnUrl;
                        return View();
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(returnUrl) || returnUrl.Equals("/"))
                {
                    returnUrl = HttpContext.Request.PathBase;
                }
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            return Redirect(UrlUtil.GetRelativeUrl(returnUrl));
        }

        public IActionResult LogIn(string id, string password, string returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                id = id.Trim();

                if (ModelState.IsValid)
                {
                    var result = this.GetService<UserService>().LogIn(id, password);

                    if (result.Status == Result.Success)
                    {
                        LogIn(result.User);

                        // 如果是注册或者登陆页面，跳转到首页
                        if (returnUrl != null &&
                            (returnUrl.ToLower().Contains("account/login") || 
                            returnUrl.ToLower().Contains("account/signup")))
                        {
                            returnUrl = null;
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMsg = result.Message;
                        ViewBag.ReturnUrl = returnUrl;

                        return View();
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(returnUrl) || returnUrl.Equals("/"))
                {
                    returnUrl = HttpContext.Request.PathBase;
                }
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            return Redirect(UrlUtil.GetRelativeUrl(returnUrl));
        }

        public IActionResult LogOff(string returnUrl)
        {
            HttpContext.Authentication.SignOutAsync("Cookies");
            return Redirect(UrlUtil.GetRelativeUrl(returnUrl));
        }

        private void LogIn(User user)
        {
            var claims = new List<Claim> {
                            new Claim(ClaimTypes.NameIdentifier, user.Id),
                            new Claim(ClaimTypes.Role, ((int)user.UserType).ToString()),
                            new Claim(ClaimTypes.Name, user.Name)
                        };

            var identity = new ClaimsIdentity(claims, "Password");
            HttpContext.Authentication.SignInAsync("Cookies", new ClaimsPrincipal(identity));
        }
    }
}
