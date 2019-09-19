using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace LAP.ONEBook.Security.StorageProviders
{
    public class DefaultProvider : IStorageProvider
    {
        private const string SpAuthPermissionDataSelect = "dbo.Dot_Auth_PermissionDataSel";
        private const string SpAuthTokenInsert = "dbo.Dot_Auth_TokenIns";

        private readonly string _connectionString;

        public DefaultProvider(string ConnectionString)
        {
            _connectionString = ConnectionString;
        }

        public void ApplyToken(string token, string permissionData, int expiry = 1)
        {
            ExecuteStoredProcedure(SpAuthTokenInsert, sqlCmd =>
            {
                AddCommandParameter(sqlCmd, "@Token", token);
                AddCommandParameter(sqlCmd, "@PermissionData", permissionData);
                AddCommandParameter(sqlCmd, "@Expiry", expiry);
                sqlCmd.ExecuteNonQuery();
            });
        }

        public Task ApplyTokenAsync(string token, string permissionData, int expiry = 1)
        {
            return ExecuteStoredProcedureAsync(SpAuthTokenInsert, sqlCmd =>
            {
                AddCommandParameter(sqlCmd, "@Token", token);
                AddCommandParameter(sqlCmd, "@PermissionData", permissionData);
                AddCommandParameter(sqlCmd, "@Expiry", expiry);
                return sqlCmd.ExecuteNonQueryAsync();
            });
        }

        public string Authenticate(string token)
        {
            string result = null;

            ExecuteStoredProcedure(SpAuthPermissionDataSelect, sqlCmd =>
            {
                AddCommandParameter(sqlCmd, "@Token", token);
                result = sqlCmd.ExecuteScalar().ToString();
            });

            return result;
        }

        public async Task<string> AuthenticateAsync(string token)
        {
            string result = null;

            await ExecuteStoredProcedureAsync(SpAuthPermissionDataSelect, async sqlCmd =>
            {
                AddCommandParameter(sqlCmd, "@Token", token);
                result = (await sqlCmd.ExecuteScalarAsync()).ToString();
            });

            return result;
        }

        protected virtual void ExecuteStoredProcedure(string commandText, Action<DbCommand> executer)
        {
            using (var sqlCon = new SqlConnection(_connectionString))
            {
                using (var sqlCmd = new SqlCommand(commandText, sqlCon))
                {
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    sqlCon.Open();
                    executer(sqlCmd);
                }
            }
        }

        protected virtual async Task ExecuteStoredProcedureAsync(string commandText, Func<DbCommand, Task> executerAsync)
        {
            using (var sqlCon = new SqlConnection(_connectionString))
            {
                using (var sqlCmd = new SqlCommand(commandText, sqlCon))
                {
                    sqlCmd.CommandType = CommandType.StoredProcedure;
                    await sqlCon.OpenAsync();
                    await executerAsync(sqlCmd);
                }
            }
        }

        private void AddCommandParameter<T>(IDbCommand command, string name, T value)
        {
            var param = command.CreateParameter();
            param.ParameterName = name;
            param.Value = value;
            command.Parameters.Add(param);
        }
    }
}