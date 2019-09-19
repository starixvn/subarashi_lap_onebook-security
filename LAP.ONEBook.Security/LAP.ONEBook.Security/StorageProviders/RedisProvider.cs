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
            var db = _connection.GetDatabase();
            return db.StringSetAsync(token, permissionData, TimeSpan.FromMinutes(expiry));
        }

        public string Authenticate(string key)
        {
            var db = _connection.GetDatabase();
            return db.StringGetSet(key, string.Empty);
        }

        public async Task<string> AuthenticateAsync(string key)
        {
            var db = _connection.GetDatabase();
            return await db.StringGetSetAsync(key, string.Empty);
        }
    }
}