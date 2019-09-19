namespace LAP.ONEBook.Security.StorageProviders
{
    public interface IStorageProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <param name="Expiry">Expiration time, in minutes</param>
        /// <returns></returns>
        void ApplyToken(string token, string permissionData, int expiry = 1);
        string Authenticate(string key);
    }
}
