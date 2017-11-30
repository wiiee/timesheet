namespace DataAccess.Database.Config
{
    public class DatabaseDescriptor
    {
        public DatabaseType DatabaseType { get; set; }
        public string DatabaseName { get; set; }
        public bool IsAuth { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        public string Name { get; set; }
        public bool IsUseReplicaSet { get; set; }
        public bool IsUseSharding { get; set; }
        public string ReplicaSetName { get; set; }
    }
}
