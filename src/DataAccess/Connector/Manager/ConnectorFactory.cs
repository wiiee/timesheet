namespace DataAccess.Connector.Base
{
    using Config;
    using Impl;
    using System;

    public class ConnectorFactory : IConnectorFactory
    {
        private static ServiceRegistry serviceRegistry = ConnectorConfigurationManager.LoadConfig();

        private ServiceDescriptor GetConfigDescriptor(Type rq, Type rs)
        {
            String requestType = rq.FullName;
            String responseType = rs.FullName;
            ServiceDescriptor desc = GetDescriptorFor(requestType, responseType);

            if (desc == null)
            {
                //log error
            }

            return desc;
        }

        private ServiceDescriptor GetDescriptorFor(String requestType, String responseType)
        {
            return serviceRegistry.GetDescriptor(requestType, responseType);
        }

        public IConnector<Rq, Rs> GetConnector<Rq, Rs>()
        {
            ServiceDescriptor serviceDescriptor = GetConfigDescriptor(typeof(Rq), typeof(Rs));

            IConnector<Rq, Rs> connector = null;

            switch (serviceDescriptor.EndpointEntity.GetEndPointType())
            {
                case EndpointType.HttpEndpoint:
                    connector = new HttpConnectorImpl<Rq, Rs>();
                    break;
                default:
                    //log error
                    break;
            }

            connector.SetServiceDescriptor(serviceDescriptor);

            return connector;
        }

    }
}