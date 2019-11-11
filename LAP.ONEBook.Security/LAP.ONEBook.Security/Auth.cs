using System;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace LAP.ONEBook.Security
{
    public class Auth<T> : IAuth<T>
    {
        private const int TOKEN_TIME = 1;
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
            token = Convert.ToBase64String(guid).Replace("=", "").Replace("+", "");
            data = ToJson(t);
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
            return DeserializeToken(token, permissionData);
        }

        private static T DeserializeToken(string token, string permissionData)
        {
            try
            {
                if (!string.IsNullOrEmpty(permissionData))
                {
                    return FromJson(permissionData);
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Authenticate error: {e.Message}\r\nToken={token}\r\nData={permissionData}", e);
            }

            return default;
        }

        public async Task<T> AuthenticateAsync(string token)
        {
            var permissionData = await _storageProvider.AuthenticateAsync(token).ConfigureAwait(false);
            return DeserializeToken(token, permissionData);
        }
        private static string ToJson(T t)
        {
            return JsonConvert.SerializeObject(t);
        }

        private static T FromJson(string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        private static byte[] StringToUtf8ByteArray(string value)
        {
            var encoding = new UTF8Encoding();
            var byteArray = encoding.GetBytes(value);
            return byteArray;
        }
    }
}