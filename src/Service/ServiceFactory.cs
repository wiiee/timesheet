namespace Service
{
    using Context;
    using Microsoft.Extensions.Logging;
    using Platform.Context;
    using Platform.Util;
    using System;
    using System.Collections.Generic;
    using Platform.Extension;

    //Resolve Service之间互相引用导致死循环的问题
    public class ServiceFactory
    {
        private static ILogger _logger = LoggerUtil.CreateLogger<ServiceFactory>();

        private Dictionary<string, IService> services;
        private bool isInited;

        private ServiceFactory()
        {

        }

        private static readonly Lazy<ServiceFactory> lazy = new Lazy<ServiceFactory>(() => new ServiceFactory());

        public static ServiceFactory Instance { get { return lazy.Value; } }

        public void Init(Dictionary<string, IService> services)
        {
            if (!isInited)
            {
                this.services = new Dictionary<string, IService>(services);
                isInited = true;
            }                      
        }

        public T GetService<T>()
            where T : IService
        {
            var key = typeof(T).FullName;

            if (!services.IsEmpty() && services.ContainsKey(key))
            {
                return (T)services[key];
            }
            else
            {
                return default(T);
            }
        }
    }
}
