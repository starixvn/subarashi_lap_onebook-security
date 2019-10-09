using System;
using Newtonsoft.Json;

namespace LAP.ONEBook.Security
{
    public class Auth<T>
    {
        private const int TOKEN_TIME = 1;

        private readonly IStorageProvider _storageProvider;

        private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
        };

        public Auth(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
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
            try
            {
                var permissionData = _storageProvider.Authenticate(token);
                if (!string.IsNullOrEmpty(permissionData))
                {
                    return JsonConvert.DeserializeObject<T>(permissionData, _jsonSerializerSettings);
                }
            }
            catch (Exception)
            {
                throw;
            }

            return default(T);
        }
    }
}