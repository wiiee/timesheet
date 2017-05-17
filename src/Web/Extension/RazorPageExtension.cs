namespace Web.Extension
{
    using System.Security.Claims;
    using Microsoft.AspNetCore.Mvc.Razor;
    using Platform.Enum;
    using Platform.Util;
    using Service;

    public static class RazorPageExtension
    {
        public static bool IsLogIn(this RazorPage page)
        {
            var isAuthenticated = page.User?.Identity?.IsAuthenticated;
            return isAuthenticated == null ? false : (bool)isAuthenticated;
        }

        public static bool IsAdmin(this RazorPage page)
        {
            if (page.IsLogIn())
            {
                return page.User.FindFirst(ClaimTypes.Role).Value == ((int)UserType.Admin).ToString();
            }

            return false;
        }

        public static bool IsUser(this RazorPage page)
        {
            if (page.IsLogIn())
            {
                return page.User.FindFirst(ClaimTypes.Role).Value == ((int)UserType.User).ToString();
            }

            return false;
        }

        public static bool IsManager(this RazorPage page)
        {
            if (page.IsLogIn())
            {
                return page.User.FindFirst(ClaimTypes.Role).Value == ((int)UserType.Manager).ToString();
            }

            return false;
        }

        public static bool IsLeader(this RazorPage page)
        {
            if (page.IsLogIn())
            {
                return page.User.FindFirst(ClaimTypes.Role).Value == ((int)UserType.Leader).ToString();
            }

            return false;
        }

        //Greater than or equal to
        public static bool IsGeLeader(this RazorPage page)
        {
            if (page.IsLogIn())
            {
                var role = page.User.FindFirst(ClaimTypes.Role).Value;
                return role != ((int)UserType.User).ToString();
            }

            return false;
        }

        public static string GetUserId(this RazorPage page)
        {
            if (page.IsLogIn())
            {
                return page.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            }

            return string.Empty;
        }

        public static string GetUserName(this RazorPage page)
        {
            if (page.IsLogIn())
            {
                if (page.User.FindFirst(ClaimTypes.Name) == null)
                {
                    return string.Empty;
                }
                else
                {
                    return page.User.FindFirst(ClaimTypes.Name).Value;
                }
            }

            return string.Empty;
        }

        public static UserType GetUserType(this RazorPage page)
        {
            if (page.IsLogIn())
            {
                return (UserType)int.Parse(page.User.FindFirst(ClaimTypes.Role).Value);
            }

            return UserType.User;
        }

        public static string GetSessionValue(this RazorPage page, string key)
        {
            byte[] value;

            page.Context.Session.TryGetValue(key, out value);

            return StringUtil.ByteArrayToStr(value);
        }

        public static T GetService<T>(this RazorPage page)
            where T : IService
        {
            return ServiceFactory.Instance.GetService<T>();
        }
    }
}
