namespace DataAccess.Database.Impl.MongoDb
{
    using Base;
    using Config;

    public class MongoDbSetting : IDatabaseSetting
    {
        private string address;
        private string databaseName;
        private string userName;
        private string password;
        private bool isUseSharding;
        private bool isUseReplicaSet;
        private bool isAuth;
        private string replicaSetName;
        

        public string GetAddress()
        {
            return address;
        }

        public string GetDatabaseName()
        {
            return databaseName;
        }

        public string GetUserName()
        {
            return userName;
        }

        public string GetPassword()
        {
            return password;
        }

        public string GetReplicaSetName()
        {
            return replicaSetName;
        }

        public DatabaseType GetDatabaseType()
        {
            return DatabaseType.MongoDB;
        }

        public string GetConnectionString()
        {
            return string.Format("mongodb://{0}:{1}@{2}/{3}?replicaSet={4}", userName, password, address, databaseName, replicaSetName);
        }

        public bool IsUseReplicaSet()
        {
            return isUseReplicaSet;
        }

        public bool IsUseSharding()
        {
            return isUseSharding;
        }

        public bool IsAuth()
        {
            return isAuth;
        }

        public MongoDbSetting(string address, string databaseName, bool isAuth, string userName, string password, 
            bool isUseReplicaSet, bool isUseSharding, string replicaSetName)
        {
            this.address = address;
            this.databaseName = databaseName;
            this.isAuth = isAuth;
            this.userName = userName;
            this.password = password;
            this.isUseReplicaSet = isUseReplicaSet;
            this.isUseSharding = isUseSharding;
            this.replicaSetName = replicaSetName;
        }
    }
}
