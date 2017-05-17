namespace DataAccess.Connector.Base
{
    using Config;

    public interface IConnector<Rq, Rs>
    {
        ServiceDescriptor GetServiceDescriptor();
        void SetServiceDescriptor(ServiceDescriptor serviceDescriptor);
        Rs Invoke(Rq request);
    }
}
