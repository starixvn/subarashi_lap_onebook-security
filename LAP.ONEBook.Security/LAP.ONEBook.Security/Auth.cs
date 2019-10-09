using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LAP.ONEBook.Security
{
    public class Auth<T> : IAuth<T>
    {
        private const int TOKEN_TIME = 1;

        private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
        };

        private readonly IStorageProvider _storageProvider;

        protected Auth()
        {
        }

        public Auth(IStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public string ApplyToken(T t) => ApplyToken(t, TOKEN_TIME);

        public string ApplyToken(T t, int expiry)
        {
            GenerateTokenInfo(t, out string token, out string data);

            _storageProvider.ApplyToken(token, data, expiry);

            return token;
        }

        private static void GenerateTokenInfo(T t, out string token, out string data)
        {
            var guid = Guid.NewGuid().ToByteArray();
            token = Convert.ToBase64String(guid).Replace("=", "").Replace("+", "").Replace("/", "");
            data = JsonConvert.SerializeObject(t, _jsonSerializerSettings);
        }

        public Task<string> ApplyTokenAsync(T t) => ApplyTokenAsync(t, TOKEN_TIME);

        public async Task<string> ApplyTokenAsync(T t, int expiry)
        {
            GenerateTokenInfo(t, out string token, out string data);

            await _storageProvider.ApplyTokenAsync(token, data, expiry).ConfigureAwait(false);

            return token;
        }

        public T Authenticate(string token)
        {
            var permissionData = _storageProvider.Authenticate(token);
            try
            {
                if (!string.IsNullOrEmpty(permissionData))
                {
                    return JsonConvert.DeserializeObject<T>(permissionData, _jsonSerializerSettings);
                }
            }
            catch (Exception e)
            {
                throw CustomException(token, permissionData, e);
            }

            return default(T);
        }

        public async Task<T> AuthenticateAsync(string token)
        {
            var permissionData = await _storageProvider.AuthenticateAsync(token).ConfigureAwait(false);
            try
            {
                if (!string.IsNullOrEmpty(permissionData))
                {
                    return JsonConvert.DeserializeObject<T>(permissionData, _jsonSerializerSettings);
                }
            }
            catch (Exception e)
            {
                throw CustomException(token, permissionData, e);
            }

            return default(T);
        }

        private static Exception CustomException(string token, string data, Exception e)
        {
            return new Exception($"Authenticate error: {e.Message}\r\nToken={token}\r\nData={data}", e);
        }
    }
}