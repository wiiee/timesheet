namespace Web.Context
{
    using Extension;
    using Microsoft.AspNetCore.Http;
    using Platform.Context;

    public class WebContext : IContext
    {
        private string userId;
        private string remoteIp;
        private bool isUseCache;

        public WebContext(HttpContext httpContext)
        {
            this.userId = httpContext.GetUserId();
            this.remoteIp = httpContext.GetRemoteIp();
            this.isUseCache = true;
        }

        //public WebContext(IHttpContextAccessor httpContextAccessor)
        //{
        //    this.userId = httpContextAccessor.HttpContext.GetUserId();
        //    this.remoteIp = httpContextAccessor.HttpContext.GetRemoteIp();
        //    this.isUseCache = false;
        //}

        public string UserId
        {
            get
            {
                return userId;
            }
        }

        public string RemoteIp
        {
            get
            {
                return remoteIp;
            }
        }

        public bool IsUseCache
        {
            get
            {
                return isUseCache;
            }
        }
    }
}
