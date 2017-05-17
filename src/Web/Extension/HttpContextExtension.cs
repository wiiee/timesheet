namespace Web.Extension
{
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using Platform.Util;
    using System.Security.Claims;

    public static class HttpContextExtension
    {
        public static string GetUserId(this HttpContext context)
        {
            var user = context.User;

            if (user == null || user.FindFirst(ClaimTypes.NameIdentifier) == null)
            {
                return null;
            }
            else
            {
                return user.FindFirst(ClaimTypes.NameIdentifier).Value;
            }
        }

        public static string GetRemoteIp(this HttpContext context)
        {
            var remoteIp = context.Connection?.RemoteIpAddress?.ToString();

            if (!string.IsNullOrEmpty(remoteIp))
            {
                return remoteIp;
            }

            string[] remoteIpHeaders =
            {
                "X-FORWARDED-FOR",
                "REMOTE_ADDR",
                "HTTP_X_FORWARDED_FOR",
                "HTTP_CLIENT_IP",
                "HTTP_X_FORWARDED",
                "HTTP_X_CLUSTER_CLIENT_IP",
                "HTTP_FORWARDED_FOR",
                "HTTP_FORWARDED",
                "X_FORWARDED_FOR",
                "CLIENT_IP",
                "X_FORWARDED",
                "X_CLUSTER_CLIENT_IP",
                "FORWARDED_FOR",
                "FORWARDED"
            };

            string value;
            foreach (string remoteIpHeader in remoteIpHeaders)
            {
                if (context.Request.Headers.ContainsKey(remoteIpHeader))
                {
                    value = context.Request.Headers[remoteIpHeader];
                    if (!string.IsNullOrEmpty(value))
                    {
                        value = value.Split(',')[0].Split(';')[0];
                        if (value.Contains("="))
                        {
                            value = value.Split('=')[1];
                        }
                        value = value.Trim('"');
                        if (value.Contains(":"))
                        {
                            value = value.Substring(0, value.LastIndexOf(':'));
                        }
                        return value.TrimStart('[').TrimEnd(']');
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return "no header found or empty value found";
        }

        public static string GetSessionValue(this HttpContext context, string key)
        {
            byte[] value;
            context.Session.TryGetValue(key, out value);

            return StringUtil.ByteArrayToStr(value);
        }

        public static void ClearSession(this HttpContext context)
        {
            context.Session.Clear();
        }

        public static void RemoveSession(this HttpContext context, string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                context.Session.Remove(key);
            }
        }

        public static void SetSessionValue(this HttpContext context, string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                context.Session.SetString(key, value);
            }
        }

        public static T GetSessionObject<T>(this HttpContext context, string key)
        {
            var value = context.Session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }

        public static void SetSessionObject(this HttpContext context, string key, object value)
        {
            if (value != null)
            {
                context.Session.SetString(key, JsonConvert.SerializeObject(value));
            }
        }
    }
}
