using MongoDB.Driver;

namespace Modix.Data
{
    class MongoDbHandler
    {
        private MongoClient client;
        public IMongoDatabase Database { get; private set; }

        public MongoDbHandler()
        {
            client = new MongoClient("mongodb://localhost:27017");
            Database = client.GetDatabase("MonkDB");
        }
    }
}
