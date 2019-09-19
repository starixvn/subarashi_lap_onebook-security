using System;
using System.Collections.Generic;
using System.Text;

namespace LAP.ONEBook.Security
{
    public class RedisProvider : IStorageProvider
    {
        public void ApplyToken(string token, string permissionData, int expiry = 1)
        {
            throw new NotImplementedException();
        }

        public string Authenticate(string key)
        {
            throw new NotImplementedException();
        }
    }
}
