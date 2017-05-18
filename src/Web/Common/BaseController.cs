namespace Web.Common
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Platform.Context;
    using Platform.Setting;
    using Platform.Util;
    using Service;
    using System.Collections.Generic;

    public abstract class BaseController : Controller
    {
        private Dictionary<string, IService> services = new Dictionary<string, IService>();

        private ISetting setting;

        private static ILogger _logger = LoggerUtil.CreateLogger<BaseController>();

        public BaseController()
        {
            this.setting = Setting.Instance;
        }

        public T GetService<T>()
            where T : IService
        {
            return ServiceFactory.Instance.GetService<T>();
        }

        private string GetRemoteIp()
        {
            var remoteIp = HttpContext.Connection?.RemoteIpAddress?.ToString();

            if(!string.IsNullOrEmpty(remoteIp))
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
                if (HttpContext.Request.Headers.ContainsKey(remoteIpHeader))
                {
                    value = Request.Headers[remoteIpHeader];
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

        public string GetSessionValue(string key)
        {
            byte[] value;
            HttpContext.Session.TryGetValue(key, out value);

            return StringUtil.ByteArrayToStr(value);
        }

        public void SetSessionValue(string key, string value)
        {
            HttpContext.Session.Set(key, StringUtil.StrToByteArray(value));
        }

        public T GetSessionObject<T>(string key)
        {
            var value = HttpContext.Session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }

        public void SetSessionObject(string key, object value)
        {
            HttpContext.Session.SetString(key, JsonConvert.SerializeObject(value));
        }
    }
}
