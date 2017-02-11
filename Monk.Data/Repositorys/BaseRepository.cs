using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Monk.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Monk.Data.Repositorys
{
    public class BaseRepository<T> where T : BaseModel
    {
        protected readonly IMongoDatabase database;
        private readonly string collectionName;

        public BaseRepository()
        {
            database = new MongoDbHandler().Database;
            collectionName = Activator.CreateInstance<T>().CollectionName;
        }

        public Task<IAsyncCursor<T>> GetAllAsync()
        {
            return database.GetCollection<T>(collectionName).FindAsync(new BsonDocument());
        }

        public T GetOne(Expression<Func<T, bool>> filter)
        {
            return database.GetCollection<T>(collectionName).Find(filter).FirstOrDefault();
        }

        public Task InsertAsync(T entity)
        {
            return database.GetCollection<T>(collectionName).InsertOneAsync(entity);
        }

        public Task<ReplaceOneResult> Update(ObjectId id, T entity)
        {
            return database.GetCollection<T>(collectionName).ReplaceOneAsync((x => x.Id == id), entity);
        }

        public Task<DeleteResult> Remove(ObjectId id)
        {
            return database.GetCollection<T>(collectionName).DeleteOneAsync(x => x.Id == id);
        }
    }
}
