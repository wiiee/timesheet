namespace DataAccess.Database.Impl.MongoDb
{
    using Base;
    using Entity;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public class MongoDbCollection<T> : IDbCollection<T>
        where T : IEntity
    {
        private IMongoCollection<T> mongoCollection;
        private IMongoDatabase mongoDb;

        public MongoDbCollection(IMongoDatabase mongoDb)
        {
            this.mongoDb = mongoDb;
            string collectionName = typeof(T).Name;
            mongoCollection = mongoDb.GetCollection<T>(collectionName);

            /**
            var grid = new MongoGridFS(new MongoServer(new MongoServerSettings { Server = new MongoServerAddress(host, port) }), databaseName, new MongoGridFSSettings());
            grid.Upload(file.InputStream, file.FileName, new MongoGridFSCreateOptions
            {
                Id = imageId,
                ContentType = file.ContentType
            });
            **/
        }

        public string GetCollectionName()
        {
            return mongoCollection.ToString();
        }

        public async Task<string> AddEntityAsync(T entity)
        {
            if(entity.Id == null)
            {
                entity.Id = ObjectId.GenerateNewId().ToString();
            }

            await mongoCollection.InsertOneAsync((T)entity);

            return entity.Id;
        }

        public async Task<List<string>> AddEntitiesAsync(List<T> entities)
        {
            List<string> ids = new List<string>();

            foreach(var entity in entities)
            {
                if(entity.Id == null)
                {
                    entity.Id = ObjectId.GenerateNewId().ToString();
                    ids.Add(entity.Id);
                }
            }

            await mongoCollection.InsertManyAsync(entities);

            return ids;
        }

        public async Task<T> GetEntityByIdAsync(string id)
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            return await mongoCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<T>> GetEntitiesAsync()
        {
            return await mongoCollection.Find(new BsonDocument()).ToListAsync();
        }

        public async Task<IAsyncCursor<T>> FindAsync()
        {
            return await mongoCollection.FindAsync(new BsonDocument());
        }

        public async Task<List<T>> FindEntitiesAsync(Expression<Func<T, bool>> selector)
        {
            return await mongoCollection.Find(selector).ToListAsync();
        }

        public async Task DeleteEntityAsync(string id)
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            await mongoCollection.DeleteOneAsync(filter);
        }

        public async Task DeleteEntityAsync(Expression<Func<T, bool>> selector)
        {
            await mongoCollection.DeleteOneAsync(selector);
        }

        public async Task DeleteEntitiesAsync(List<string> ids)
        {
            var filter = Builders<T>.Filter.In("_id", ids);
            await mongoCollection.DeleteManyAsync(filter);
        }

        public async Task DeleteEntitiesAsync(Expression<Func<T, bool>> selector)
        {
            await mongoCollection.DeleteManyAsync(selector);
        }

        public async Task UpdateEntityAsync(T entity)
        {
            var filter = Builders<T>.Filter.Eq("_id", entity.Id);
            await mongoCollection.ReplaceOneAsync(filter, entity);
        }

        public async Task UpdateEntityAsync<TField>(string id, Expression<Func<T, TField>> set, TField value)
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            var update = Builders<T>.Update.Set(set, value);
            await mongoCollection.UpdateOneAsync(filter, update);
        }

        public async Task UpdateEntityAsync<TField>(string id, string field, TField value)
        {
            var filter = Builders<T>.Filter.Eq("_id", id);
            FieldDefinition<T, TField> fieldDefinition = field;
            var update = Builders<T>.Update.Set(fieldDefinition, value);
            await mongoCollection.UpdateOneAsync(filter, update);
        }

        public async Task UpdateEntitiesAsync<TField>(Expression<Func<T, bool>> selector, Expression<Func<T, TField>> set, TField value)
        {
            var update = Builders<T>.Update.Set(set, value);
            await mongoCollection.UpdateManyAsync(selector, update);
        }

        public async Task UpdateEntitiesAsync(List<T> entities)
        {
            foreach(var entity in entities)
            {
                var filter = Builders<T>.Filter.Eq("_id", entity.Id);
                await mongoCollection.ReplaceOneAsync(filter, entity);
            }
        }

        public async Task SaveEntityAsync(T entity)
        {
            var item = await GetEntityByIdAsync(entity.Id);

            if(item == null)
            {
                await AddEntityAsync(entity);
            }
            else
            {
                await UpdateEntityAsync(entity);
            }
        }

        public async Task<TField> GetField<TField>(string id, Expression<Func<T, TField>> field)
        {
            return await mongoCollection
                .Find(o => o.Id == id)
                .Project(new ProjectionDefinitionBuilder<T>().Expression(field))
                .FirstOrDefaultAsync();
        }

        public async Task<List<TField>> GetFields<TField>(Expression<Func<T, TField>> field)
        {
            return await mongoCollection
                .Find(new BsonDocument())
                .Project(new ProjectionDefinitionBuilder<T>().Expression(field)).ToListAsync();
        }
    }
}
