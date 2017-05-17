namespace DataAccess.Database.Base
{
    public interface IDatabaseFactory
    {
        IDatabase CreateDatabase(IDatabaseSetting databaseSetting);
    }
}
