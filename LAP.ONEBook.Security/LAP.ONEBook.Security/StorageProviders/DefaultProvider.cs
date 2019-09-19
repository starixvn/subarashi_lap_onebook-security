using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

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
            ExecuteStoredProcedure("dbo.Dot_Auth_TokenIns", sqlCmd =>
            {
                AddCommandParameter(sqlCmd, "@Token", token);
                AddCommandParameter(sqlCmd, "@PermissionData", permissionData);
                AddCommandParameter(sqlCmd, "@Expiry", expiry);
                sqlCmd.ExecuteNonQuery();
            });
        }

        public Task ApplyTokenAsync(string token, string permissionData, int expiry = 1)
        {
            throw new NotImplementedException();
        }

        public string Authenticate(string token)
        {
            string result = null;

            ExecuteStoredProcedure("dbo.Dot_Auth_PermissionDataSel", sqlCmd =>
            {
                AddCommandParameter(sqlCmd, "@Token", token);
                result = sqlCmd.ExecuteScalar().ToString();
            });

            return result;
        }

        public Task<string> AuthenticateAsync(string key)
        {
            throw new NotImplementedException();
        }

        protected virtual void ExecuteStoredProcedure(string commandText, Action<IDbCommand> executer)
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

        private void AddCommandParameter<T>(IDbCommand command, string name, T value)
        {
            var param = command.CreateParameter();
            param.ParameterName = name;
            param.Value = value;
            command.Parameters.Add(param);
        }
    }
}