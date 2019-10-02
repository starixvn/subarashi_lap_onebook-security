using System;
using System.Data.SqlClient;
using LAP.ONEBook.Security.StorageProviders;
using Newtonsoft.Json;

namespace LAP.ONEBook.Security
{
    public class Auth<T>
    {
        private const int TOKEN_TIME = 1;

        private readonly SqlConnectionStringBuilder _sqlClientDBConnectionSB;
        private readonly IStorageProvider _storageProvider;

        private static JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
        };

        public bool IsTest { set; get; }

        public Auth(IStorageProvider StorageProvider)
        {
            _storageProvider = StorageProvider;
        }

        public Auth(string sqlClientDBConnectionString)
        {
            _sqlClientDBConnectionSB = new SqlConnectionStringBuilder(sqlClientDBConnectionString);
            _storageProvider = new DefaultProvider(sqlClientDBConnectionString);
        }

        public string ApplyToken(T t)
        {
            return ApplyToken(t, TOKEN_TIME);
        }

        public string ApplyToken(T t, int expiry)
        {
            var guid = Guid.NewGuid().ToByteArray();
            var token = Convert.ToBase64String(guid).Replace("=", "").Replace("+", "").Replace("/", "");
            var data = JsonConvert.SerializeObject(t, _jsonSerializerSettings);

            _storageProvider.ApplyToken(token, data, expiry);

            return token;
        }

        public T Authenticate(string token)
        {
            var permissionData = _storageProvider.Authenticate(token);
            return string.IsNullOrEmpty(permissionData) ? default(T) : FromJson(permissionData);
        }

        public bool Authenticate(string token, ref T permission)
        {
            string permissionData = _storageProvider.Authenticate(token);
            if (string.IsNullOrEmpty(permissionData))
            {
                permission = default(T);
                return false;
            }
            else
            {
                permission = FromJson(permissionData);
                return true;
            }
        }

        private static T FromJson(string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString, _jsonSerializerSettings);
        }
    }
}