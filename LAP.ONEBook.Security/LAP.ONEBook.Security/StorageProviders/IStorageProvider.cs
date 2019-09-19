using System.Threading.Tasks;

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
        Task ApplyTokenAsync(string token, string permissionData, int expiry = 1);

        void ApplyToken(string token, string permissionData, int expiry = 1);

        Task<string> AuthenticateAsync(string key);

        string Authenticate(string key);
    }
}