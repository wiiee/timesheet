namespace DataAccess.Database.Manager
{
    using Config;
    using Microsoft.Extensions.Configuration;
    using System.Collections.Generic;
    using System;
    using Platform.Util;
    using Platform.Setting;

    public class DatabaseConfigurationManager
    {
        private DatabaseRegistry databaseRegistry;

        public DatabaseRegistry DatabaseRegistry
        {
            get
            {
                return databaseRegistry;
            }
        }

        private DatabaseConfigurationManager()
        {
            databaseRegistry = LoadConfig();
        }

        private static readonly Lazy<DatabaseConfigurationManager> _databaseConfigurationManager = new Lazy<DatabaseConfigurationManager>(() => new DatabaseConfigurationManager());

        public static DatabaseConfigurationManager Instance
        {
            get
            {
                return _databaseConfigurationManager.Value;
            }
        }

        private DatabaseRegistry LoadConfig()
        {
            //var appEnv = CallContextServiceLocator.Locator.ServiceProvider.GetService(typeof(IApplicationEnvironment)) as IApplicationEnvironment;
            //var basePath = appEnv.ApplicationBasePath;

            var path = Setting.Instance.Get("DbSettingFilePath");

            var builder = new ConfigurationBuilder()
                .AddJsonFile(path);

            var configuration = builder.Build();

            var databaseRegistry = new DatabaseRegistry();

            var descriptors = new List<DatabaseDescriptor>();

            foreach (var item in configuration.GetSection("databaseRegistry:databaseDescriptors").GetChildren())
            {
                var descriptor = new DatabaseDescriptor();
                descriptor.Name = item.GetSection("databaseDescriptor").Value;
                descriptor.Address = item.GetSection("address").Value;
                descriptor.DatabaseName = item.GetSection("databaseName").Value;
                descriptor.DatabaseType = EnumUtil.ParseEnum<DatabaseType>(item.GetSection("databaseType").Value);
                descriptor.UserName = item.GetSection("userName").Value;
                descriptor.Password = item.GetSection("password").Value;

                descriptors.Add(descriptor);
            }

            databaseRegistry.DatabaseDescriptors = descriptors;

            return databaseRegistry;
        }
    }
}
