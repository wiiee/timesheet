namespace DataAccess.Connector.Impl
{
    using System.IO;
    using System.Text;
    using Base;
    using Config;

    public sealed class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding { get { return Encoding.UTF8; } }
    }

    public class HttpConnectorImpl<Rq, Rs> : IConnector<Rq, Rs>
    {
        private ServiceDescriptor serviceDescriptor;

        public ServiceDescriptor GetServiceDescriptor()
        {
            return serviceDescriptor;
        }

        public void SetServiceDescriptor(ServiceDescriptor serviceDescriptor)
        {
            this.serviceDescriptor = serviceDescriptor;
        }

        public Rs Invoke(Rq request)
        {
            return default(Rs);
        }
    }
}