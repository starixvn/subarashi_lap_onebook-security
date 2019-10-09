using System;
using StackExchange.Redis;

namespace LAP.ONEBook.Security.StorageProvider.Redis
{
    public class RedisProvider : IStorageProvider
    {
        private readonly ConfigurationOptions _configOptions;

        public RedisProvider(string configString)
        {
            if (string.IsNullOrEmpty(configString))
            {
                throw new ArgumentNullException(nameof(configString));
            }

            _configOptions = ConfigurationOptions.Parse(configString);
        }

        public RedisProvider(ConfigurationOptions configOptions)
        {
            _configOptions = configOptions ?? throw new ArgumentNullException(nameof(configOptions));
        }

        public void ApplyToken(string token, string permissionData, int expiry = 1)
        {
            using (var conn = ConnectionMultiplexer.Connect(_configOptions))
            {
                var db = conn.GetDatabase();
                db.StringSet(token, permissionData, TimeSpan.FromMinutes(expiry));
            }
        }

        public string Authenticate(string token)
        {
            using (var conn = ConnectionMultiplexer.Connect(_configOptions))
            {
                var db = conn.GetDatabase();
                var data = db.StringGet(token);

                if (data.HasValue)
                {
                    db.KeyDelete(token);
                }

                return data;
            }
        }
    }
}