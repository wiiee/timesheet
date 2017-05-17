namespace DataAccess.Database.Manager
{
    using Base;
    using Config;
    using Impl.MongoDb;

    public class DatabaseFactory : IDatabaseFactory
    {
        public IDatabase CreateDatabase(IDatabaseSetting databaseSetting)
        {
            IDatabase database = null;

            switch (databaseSetting.GetDatabaseType())
            {
                case DatabaseType.MongoDB:
                    database = new MongoDbImpl(databaseSetting);
                    break;
                default:
                    break;
            }

            return database;
        }
    }
}
