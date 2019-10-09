using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

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
            using (var stream = new MemoryStream())
            {
                var dcs = new DataContractJsonSerializer(t.GetType());
                dcs.WriteObject(stream, t);
                stream.Position = 0;
                using (var streamReader = new StreamReader(stream))
                {
                    var json = streamReader.ReadToEnd();

                    return json.Replace("\\/", "/");
                }
            }
        }

        private static T FromJson(string jsonString)
        {
            using (var stream = new MemoryStream())
            {
                var dcs = new DataContractJsonSerializer(typeof(T));
                var bytes = StringToUtf8ByteArray(jsonString);
                stream.Write(bytes, 0, bytes.Length);
                stream.Position = 0;

                return (T)dcs.ReadObject(stream);
            }
        }

        private static byte[] StringToUtf8ByteArray(string value)
        {
            var encoding = new UTF8Encoding();
            var byteArray = encoding.GetBytes(value);
            return byteArray;
        }
    }
}