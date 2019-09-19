using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace LAP.ONEBook.Security.StorageProviders
{
    public class RedisProvider : IStorageProvider
    {
        private readonly IConnectionMultiplexer _connection;

        public RedisProvider(IConnectionMultiplexer connection)
        {
            _connection = connection;
        }

        public void ApplyToken(string token, string permissionData, int expiry = 1)
        {
            var db = _connection.GetDatabase();
            db.StringSet(token, permissionData, TimeSpan.FromMinutes(expiry));
        }

        public Task ApplyTokenAsync(string token, string permissionData, int expiry = 1)
        {
            throw new NotImplementedException();
        }

        public string Authenticate(string key)
        {
            var db = _connection.GetDatabase();
            return db.StringGetSet(key, string.Empty);
        }

        public Task<string> AuthenticateAsync(string key)
        {
            throw new NotImplementedException();
        }
    }
}