namespace Service
{
    using DataAccess.Database.Base;
    using DataAccess.Database.Manager;
    using Entity;
    using Microsoft.Extensions.Logging;
    using Platform.Constant;
    using System;
    using Platform.Util;

    public class Repository<T>
        where T : IEntity
    {
        private IDatabase db;
        private IDbCollection<T> collection;

        private static ILogger _logger = LoggerUtil.CreateLogger<Repository<T>>();

        private Repository()
        {
            db = DatabaseManager.Instance.GetDatabase(GetDbName(typeof(T).FullName));
            collection = db.GetCollection<T>();
        }

        private static readonly Lazy<Repository<T>> lazy = new Lazy<Repository<T>>(() => new Repository<T>());

        public static Repository<T> Instance { get { return lazy.Value; } }

        public IDbCollection<T> GetCollection()
        {
            return collection;
        }

        public void RenameCollection(string oldName, string newName)
        {
            db.RenameCollection(oldName, newName);
        }

        public void DropCollection()
        {
            db.DropCollection<T>();
        }

        private string GetDbName(string type)
        {
            return DbName.TIMESHEET;
        }
    }
}
