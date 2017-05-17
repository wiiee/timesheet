namespace DataAccess.Database.Base
{
    using Entity;
    using MongoDB.Driver;
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public interface IDbCollection<T>
        where T : IEntity
    {
        string GetCollectionName();

        Task<string> AddEntityAsync(T entity);
        Task<List<string>> AddEntitiesAsync(List<T> entities);

        Task<T> GetEntityByIdAsync(string id);
        Task<List<T>> GetEntitiesAsync();
        Task<List<T>> FindEntitiesAsync(Expression<Func<T, bool>> selector);

        Task DeleteEntityAsync(string id);
        Task DeleteEntityAsync(Expression<Func<T, bool>> selector);
        Task DeleteEntitiesAsync(List<string> ids);
        Task DeleteEntitiesAsync(Expression<Func<T, bool>> selector);

        Task UpdateEntityAsync(T entity);
        Task UpdateEntityAsync<TField>(string id, Expression<Func<T, TField>> set, TField value);
        Task UpdateEntityAsync<TField>(string id, string field, TField value);
        Task UpdateEntitiesAsync(List<T> entities);
        Task UpdateEntitiesAsync<TField>(Expression<Func<T, bool>> selector, Expression<Func<T, TField>> set, TField value);

        Task SaveEntityAsync(T entity);

        Task<TField> GetField<TField>(string id, Expression<Func<T, TField>> field);
        Task<List<TField>> GetFields<TField>(Expression<Func<T, TField>> field);

        Task<IAsyncCursor<T>> FindAsync();

        /**
        List<T> GetEntities<T>(Expression<Func<T, bool>> selector) where T : IEntity;

        //IQueryable<T> GetQueryable<T>(string collectionName = null) where T : IEntity;

        bool RemoveEntities<T>(Expression<Func<T, bool>> selector) where T : IEntity;

        void UpdateEntities<T, TMember>(Expression<Func<T, bool>> selector, Expression<Func<T, TMember>> set, TMember value) where T : IEntity;

        void UpdateEntities<T>(Expression<Func<T, bool>> selector, Dictionary<Expression<Func<T, object>>, object> sets) where T : IEntity;

        T GetEntity<T>(string id) where T : IEntity;

        //T GetMaxEntity<T>(Expression<Func<T, object>> selector) where T : IEntity;
        //T GetMinEntity<T>(Expression<Func<T, object>> selector) where T : IEntity;
        **/
    }
}
