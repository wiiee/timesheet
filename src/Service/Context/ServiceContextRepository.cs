namespace Service.Context
{
    using Platform.Context;
    using System;

    public class ServiceContextRepository : IContextRepository
    {
        private static readonly Lazy<ServiceContextRepository> lazy = new Lazy<ServiceContextRepository>(() => new ServiceContextRepository());

        private ServiceContextRepository()
        {
        }

        public static ServiceContextRepository Instance { get { return lazy.Value; } }

        public IContext GetCurrent()
        {
            return new ServiceContext("Service", "127.0.0.1");
        }

        public void SetContext(IContext context)
        {
            throw new NotImplementedException();
        }
    }
}
