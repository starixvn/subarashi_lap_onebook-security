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
            return ApplyToken(t, 1);
        }

        public string ApplyToken(T t, int expiry = 1)
        {
            var g = Guid.NewGuid();
            var GuidString = Convert.ToBase64String(g.ToByteArray());
            GuidString = GuidString.Replace("=", "");
            GuidString = GuidString.Replace("+", "");

            _storageProvider.ApplyToken(GuidString, ToJson(t), expiry);

            return GuidString;
        }

        public T Authenticate(string sToken)
        {
            var sPermissionData = "";
            sPermissionData = _storageProvider.Authenticate(sToken);
            return string.IsNullOrEmpty(sPermissionData) ? default(T) : FromJson(sPermissionData);
        }

        public bool Authenticate(string sToken, ref T t)
        {
            var sPermissionData = "";

            sPermissionData = _storageProvider.Authenticate(sToken);

            if (string.IsNullOrEmpty(sPermissionData))
            {
                t = default(T);
                return false;
            }
            else
            {
                t = FromJson(sPermissionData);
                return true;
            }
        }

        private static string ToJson(T t)
        {
            var stream = new MemoryStream();
            var dcs = new DataContractJsonSerializer(t.GetType());
            dcs.WriteObject(stream, t);

            stream.Position = 0;
            using (var streamReader = new StreamReader(stream))
            {
                var json = streamReader.ReadToEnd();

                return json.Replace("\\/", "/");
            }
        }

        private static T FromJson(string jsonString)
        {
            var stream = new MemoryStream();
            var dcs = new DataContractJsonSerializer(typeof(T));
            var bytes = StringToUtf8ByteArray(jsonString);
            stream.Write(bytes, 0, bytes.Length);
            stream.Position = 0;

            return (T)dcs.ReadObject(stream);
        }

        private static Byte[] StringToUtf8ByteArray(string value)
        {
            Debug.Assert(value != null);

            var encoding = new UTF8Encoding();
            var byteArray = encoding.GetBytes(value);
            return byteArray;
        }
    }
}