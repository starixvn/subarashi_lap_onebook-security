using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace LAP.ONEBook.Security.StorageProvider.SqlServer
{
    public class SqlServerProvider : IStorageProvider
    {
        private const string SpAuthPermissionDataSelect = "dbo.Dot_Auth_PermissionDataSel";
        private const string SpAuthTokenInsert = "dbo.Dot_Auth_TokenIns";

        private readonly string _connectionString;

        public SqlServerProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void ApplyToken(string token, string permissionData, int expiry = 1)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand(SpAuthTokenInsert, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Token", token);
                    cmd.Parameters.AddWithValue("@PermissionData", permissionData);
                    cmd.Parameters.AddWithValue("@Expiry", expiry);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public async Task ApplyTokenAsync(string token, string permissionData, int expiry = 1)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand(SpAuthTokenInsert, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Token", token);
                    cmd.Parameters.AddWithValue("@PermissionData", permissionData);
                    cmd.Parameters.AddWithValue("@Expiry", expiry);

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public string Authenticate(string token)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand(SpAuthPermissionDataSelect, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Token", token);
                    conn.Open();
                    return cmd.ExecuteScalar()?.ToString();
                }
            }
        }

        public async Task<string> AuthenticateAsync(string token)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var cmd = new SqlCommand(SpAuthPermissionDataSelect, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Token", token);

                    await conn.OpenAsync();
                    var result = await cmd.ExecuteScalarAsync();

                    return result.ToString();
                }
            }
        }
    }
}