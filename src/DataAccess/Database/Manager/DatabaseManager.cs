namespace DataAccess.Database.Manager
{
    using Base;
    using Config;
    using Impl.MongoDb;
    using Microsoft.Extensions.Logging;
    using MongoDB.Bson.Serialization;
    using MongoDB.Bson.Serialization.Conventions;
    using MongoDB.Bson.Serialization.Options;
    using MongoDB.Bson.Serialization.Serializers;
    using Platform.Util;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class DatabaseManager
    {
        private Dictionary<string, IDatabase> databases;
        private static readonly Lazy<DatabaseManager> _databaseManager = new Lazy<DatabaseManager>(() => new DatabaseManager());
        private DatabaseRegistry databaseRegistry;
        private IDatabaseFactory databaseFactory;

        private static ILogger _logger = LoggerUtil.CreateLogger<DatabaseManager>();

        public IDatabase GetDatabase(string databaseName)
        {
            if(databases.ContainsKey(databaseName))
            {
                return databases[databaseName];
            }

            return null;
        }

        private void CreateDatebases()
        {
            foreach (var item in databaseRegistry.DatabaseDescriptors)
            {
                IDatabaseSetting databaseSetting = null;

                switch(item.DatabaseType)
                {
                    case DatabaseType.MongoDB:
                        databaseSetting = new MongoDbSetting(item.Address, item.Name, item.UserName, item.Password, false, false, string.Empty);
                        break;
                    default:
                        break;
                }

                databases.Add(item.DatabaseName, databaseFactory.CreateDatabase(databaseSetting));
            }
        }

        public static void RegisterDateTimeSerializer()
        {           
            if (BsonSerializer.LookupSerializer(typeof(DateTime)) == null)
            {
                BsonSerializer.RegisterSerializer(typeof(DateTime), new DateTimeSerializer(DateTimeKind.Local));
            }
        }

        private void RegisterConvention()
        {
            ConventionRegistry.Register(
            "DictionaryRepresentationConvention",
            new ConventionPack { new DictionaryRepresentationConvention(DictionaryRepresentation.ArrayOfArrays) },
            _ => true);
        }

        private void RegisterTypes()
        {
            var types = Assembly.Load(new AssemblyName("Entity")).GetTypes();

            foreach (var type in types)
            {
                if (type.GetTypeInfo().IsClass)
                {
                    //BsonClassMap.LookupClassMap(type);
                    try
                    {
                        RegisterType(type);
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError(ex.Message);
                    }
                }
            }
        }

        //Register type
        private void RegisterType(Type type)
        {
            if (!BsonClassMap.IsClassMapRegistered(type))
            {
                string typeName = type.Name;
                if(typeName.Contains("Extension") || typeName.Contains("Util"))
                {
                    return;
                }

                var bsonClassMap = new BsonClassMap(type);
                bsonClassMap.AutoMap();
                bsonClassMap.SetIgnoreExtraElements(true);

                //Set date time with local
                var names = TypeUtil.GetNamesWithDateTime(type);

                if ( names.Count > 0)
                {
                    var dateTimeSerializer = new DateTimeSerializer(DateTimeKind.Local);
                    foreach(var name in names)
                    {
                        if(bsonClassMap.GetMemberMap(name) != null)
                        {
                            bsonClassMap.GetMemberMap(name).SetSerializer(dateTimeSerializer);
                        }
                    }
                }
                
                BsonClassMap.RegisterClassMap(bsonClassMap);
                _logger.LogInformation(type.ToString());
            }
        }

        private void SetupMongoDbEnvironment()
        {
            //RegisterDateTimeSerializer();
            RegisterConvention();
            RegisterTypes();
        }

        private DatabaseManager()
        {
            SetupMongoDbEnvironment();
            databases = new Dictionary<string, IDatabase>();
            databaseRegistry = DatabaseConfigurationManager.Instance.DatabaseRegistry;
            databaseFactory = new DatabaseFactory();
            CreateDatebases();
        }

        public static DatabaseManager Instance
        {
            get
            {
                return _databaseManager.Value;
            }
        }
    }
}
