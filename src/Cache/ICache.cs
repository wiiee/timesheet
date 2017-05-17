namespace Cache
{
    using Entity;
    using System.Collections.Generic;

    public interface ICache<T>
        where T : IEntity
    {
        //建缓存
        void BuildWithList(IEnumerable<T> entities);
        void BuildWithDictionary(Dictionary<string, T> entities);

        void Set(T entity);
        void Set(List<T> entities);

        void Remove(string id);
        void Remove(IEnumerable<string> ids);

        //清空缓存
        void Clear();

        //获取单个
        T Get(string id);
        //获取所有
        Dictionary<string, T> GetWithDictionary();
        List<T> GetWithList();

        List<string> GetIds();

        Dictionary<string, T> GetByIdsWithDictionary(IEnumerable<string> ids);
        List<T> GetByIdsWithList(IEnumerable<string> ids);

        bool IsReady();
    }
}
