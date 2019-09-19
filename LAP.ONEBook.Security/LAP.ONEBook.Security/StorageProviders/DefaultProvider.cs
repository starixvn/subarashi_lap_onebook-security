using System.Data.SqlClient;

namespace LAP.ONEBook.Security.StorageProviders
{
    internal class DefaultProvider : IStorageProvider
    {
        private readonly string _connectionString;

        public DefaultProvider(string ConnectionString)
        {
            _connectionString = ConnectionString;
        }
        public void ApplyToken(string token, string permissionData, int expiry = 1)
        {
            using (SqlConnection SqlCon = new SqlConnection(_connectionString))
            {
                var sSPname = "dbo.Dot_Auth_TokenIns";

                using (SqlCommand SqlCmd = new SqlCommand(sSPname, SqlCon))
                {
                    SqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    SqlCmd.Parameters.Add("@Token", System.Data.SqlDbType.VarChar).Value = token;
                    SqlCmd.Parameters.Add("@PermissionData", System.Data.SqlDbType.VarChar).Value = permissionData;
                    SqlCmd.Parameters.Add("@Expiry", System.Data.SqlDbType.Int).Value = expiry;
                    SqlCon.Open();
                    SqlCmd.ExecuteNonQuery();
                }
            }
        }

        public string Authenticate(string token)
        {
            using (SqlConnection SqlCon = new SqlConnection(_connectionString))
            {
                var sSPname = "dbo.Dot_Auth_PermissionDataSel";

                using (SqlCommand SqlCmd = new SqlCommand(sSPname, SqlCon))
                {
                    SqlCmd.CommandType = System.Data.CommandType.StoredProcedure;
                    SqlCmd.Parameters.Add("@Token", System.Data.SqlDbType.VarChar).Value = token;
                    SqlCon.Open();
                    return SqlCmd.ExecuteScalar().ToString();
                }
            }
        }
    }

}
