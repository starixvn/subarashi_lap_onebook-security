using LAP.ONEBook.Security.StorageProviders;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace LAP.ONEBook.Security
{
    public class Auth<T>
    {
        private const int TOKEN_TIME = 1;

        private readonly SqlConnectionStringBuilder _sqlClientDBConnectionSB;
        private readonly IStorageProvider _storageProvider;

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
            var guid = Guid.NewGuid();
            var guidString = Convert.ToBase64String(guid.ToByteArray()).Replace("=", "").Replace("+", "");

            _storageProvider.ApplyToken(guidString, ToJson(t), expiry);

            return guidString;
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

        private static string ToJson(T t)
        {
            using (var stream = new MemoryStream())
            {
                var dcs = new DataContractJsonSerializer(typeof(T));
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
            Debug.Assert(value != null);

            var encoding = new UTF8Encoding();
            var byteArray = encoding.GetBytes(value);
            return byteArray;
        }
    }
}