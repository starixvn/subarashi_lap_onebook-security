using System.Threading.Tasks;

namespace LAP.ONEBook.Security
{
    public interface IAuth<T>
    {
        string ApplyToken(T t);
        string ApplyToken(T t, int expiry);
        Task<string> ApplyTokenAsync(T t);
        Task<string> ApplyTokenAsync(T t, int expiry);
        T Authenticate(string token);
        Task<T> AuthenticateAsync(string token);
    }
}