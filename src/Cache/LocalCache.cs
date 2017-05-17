namespace Cache
{
    using Entity;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class LocalCache<T> : ICache<T>
        where T : IEntity
    {
        private static readonly Lazy<LocalCache<T>> lazy = new Lazy<LocalCache<T>>(() => new LocalCache<T>());

        public static LocalCache<T> Instance { get { return lazy.Value; } }

        private LocalCache() { }

        private Dictionary<string, T> caches;
        private bool isReady;

        private object lockObj = new object();

        public void BuildWithList(IEnumerable<T> entities) {
            lock (lockObj)
            {
                caches = entities.ToDictionary(o => o.Id);
                isReady = true;
            }
        }
        public void BuildWithDictionary(Dictionary<string, T> entities) {
            lock(lockObj)
            {
                caches = entities;
                isReady = true;
            } 
        }

        public void Set(T entity) {
            if(!isReady)
            {
                return;
            }

            lock(lockObj)
            {
                if (caches.ContainsKey(entity.Id))
                {
                    caches[entity.Id] = entity;
                }
                else
                {
                    caches.Add(entity.Id, entity);
                }
            }
        }

        public void Set(List<T> entities)
        {
            if (!isReady)
            {
                return;
            }

            foreach (var item in entities)
            {
                Set(item);
            }
        }

        public void Clear() {
            if (!isReady)
            {
                return;
            }

            lock (lockObj)
            {
                caches = null;
                isReady = false;
            }
        }

        public T Get(string id)
        {
            if (!isReady)
            {
                return default(T);
            }

            if (caches.ContainsKey(id))
            {
                return caches[id];
            }

            return default(T);
        }

        public void Remove(string id)
        {
            if (!isReady)
            {
                return;
            }

            lock (lockObj)
            {
                if (caches.ContainsKey(id))
                {
                    caches.Remove(id);
                }
            }
        }

        public void Remove(IEnumerable<string> ids)
        {
            if (!isReady)
            {
                return;
            }

            lock (lockObj)
            {
                foreach(var item in ids)
                {
                    caches.Remove(item);
                }
            }
        }

        public Dictionary<string, T> GetWithDictionary()
        {
            return caches;
        }

        public List<T> GetWithList()
        {
            return caches.Select(o => o.Value).ToList(); ;
        }

        public List<string> GetIds()
        {
            return caches.Keys.ToList();
        }

        public Dictionary<string, T> GetByIdsWithDictionary(IEnumerable<string> ids)
        {
            return caches.Where(o => ids.Contains(o.Key)).ToDictionary(pair => pair.Key, pair => pair.Value);
        }

        public List<T> GetByIdsWithList(IEnumerable<string> ids)
        {
            return caches.Where(o => ids.Contains(o.Key)).Select(o => o.Value).ToList();
        }

        public bool IsReady()
        {
            return isReady;
        }
    }
}
