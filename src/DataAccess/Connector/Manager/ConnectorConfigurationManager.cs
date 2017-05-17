namespace DataAccess.Connector.Base
{
    using Microsoft.Extensions.Configuration;
    using System.Collections.Generic;
    using Config;

    public static class ConnectorConfigurationManager
    {
        public static ServiceRegistry LoadConfig()
        {
            var builder = new ConfigurationBuilder()
            .AddJsonFile("connectorRegistry.json");

            var configuration = builder.Build();

            var serviceRegistry = new ServiceRegistry();

            var descriptors = new List<ServiceDescriptor>();

            foreach (var item in configuration.GetChildren())
            {

                var descriptor = new ServiceDescriptor();

                //descriptor.Name = section.Value["databaseDescriptor"];

                descriptors.Add(descriptor);
            }

            serviceRegistry.ServiceDescriptors = descriptors;

            return serviceRegistry;
        }
    }
}