namespace Service.Context
{
    using Platform.Context;

    public class ServiceContextUtil
    {
        public static IContext SERVICE_CONTEXT = new ServiceContext("Service", "127.0.0.1");
    }
}
