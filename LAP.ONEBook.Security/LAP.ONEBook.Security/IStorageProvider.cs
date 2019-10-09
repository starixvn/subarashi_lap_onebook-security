using System.Threading.Tasks;

namespace LAP.ONEBook.Security
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

        Task<string> AuthenticateAsync(string token);

        string Authenticate(string token);
    }
}