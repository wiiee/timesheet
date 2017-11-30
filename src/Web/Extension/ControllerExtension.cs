namespace Web.Extension
{
    using Entity.User;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Platform.Enum;
    using System.Collections.Generic;
    using System.Security.Claims;

    public static class ControllerExtension
    {
        public static string GetUserId(this Controller controller)
        {
            var user = controller.User;

            if (user == null || user.FindFirst(ClaimTypes.NameIdentifier) == null)
            {
                return null;
            }
            else
            {
                return user.FindFirst(ClaimTypes.NameIdentifier).Value;
            }
        }

        public static string GetUserName(this Controller controller)
        {
            var user = controller.User;

            if (user == null || user.FindFirst(ClaimTypes.Name) == null)
            {
                return string.Empty;
            }
            else
            {
                return user.FindFirst(ClaimTypes.Name).Value;
            }
        }

        public static UserType GetUserType(this Controller controller)
        {
            var user = controller.User;

            if (user == null || user.FindFirst(ClaimTypes.Role) == null)
            {
                return UserType.User;
            }
            else
            {
                return (UserType)int.Parse(user.FindFirst(ClaimTypes.Role).Value);
            }
        }

        public static bool IsAuthenticated(this Controller controller)
        {
            var user = controller.User;
            var result = user?.Identity?.IsAuthenticated;

            return result == null ? false : (bool)result;
        }

        public static void BuildHeaderMsg(this Controller controller, string successHeaderMsg, string errorHeaderMsg)
        {
            if (!string.IsNullOrEmpty(successHeaderMsg))
            {
                controller.ViewData["SuccessHeaderMsg"] = successHeaderMsg;
            }

            if (!string.IsNullOrEmpty(errorHeaderMsg))
            {
                controller.ViewData["ErrorHeaderMsg"] = errorHeaderMsg;
            }
        }

        public static bool IsMobileBrowser(this Controller controller)
        {
            return controller.Request.IsMobileBrowser();
        }

        public static void LogInCookie(this Controller controller, User user)
        {
            var claims = new List<Claim> {
                            new Claim(ClaimTypes.NameIdentifier, user.Id),
                            new Claim(ClaimTypes.Role, ((int)user.UserType).ToString())
                        };

            var identity = new ClaimsIdentity(claims, "Password");
            controller.Request.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        }

        public static void LogOffCookie(this Controller controller, string returnUrl)
        {
            controller.Request.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
