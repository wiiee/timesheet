namespace Service.Context
{
    using Platform.Context;

    public class ServiceContext : IContext
    {
        private string userId;
        private string remoteIp;
        private bool isUseCache;

        public ServiceContext(string userId, string remoteIp)
        {
            this.userId = userId;
            this.remoteIp = remoteIp;
        }

        public ServiceContext(string userId, string remoteIp, bool isUseCache)
        {
            this.userId = userId;
            this.remoteIp = remoteIp;
            this.isUseCache = isUseCache;
        }

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
