using System.Data.SqlClient;
using System.Data;
using JWTAuth.Interfaces;
using System.Data.Common;

namespace JWTAuth.Services
{
    public class DataProviderService : IDataProviderService
    {     
        IConfiguration _configuration;

        public DataProviderService(IConfiguration config)
        {
            _configuration = config;            
        }

        public delegate void NonQueryParams(SqlParameterCollection col);
        public delegate void NonQueryOutput(SqlParameterCollection col);
        public delegate void ExecuteCmdParams(SqlParameterCollection col);
        public delegate void ExecuteCmdReader(SqlDataReader DataReader, short set);
        public async Task ExecuteNonQuery(string storedProcedure, NonQueryParams paramCollection, NonQueryOutput paramOutputCollection)
        {
            using (SqlConnection _connection = new SqlConnection(_configuration.GetConnectionString("Sql")))
            {
                using (SqlCommand command = new SqlCommand(storedProcedure, _connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    if (paramCollection != null)
                    {
                        paramCollection(command.Parameters);
                    }
                    await _connection.OpenAsync();
                    int rows = await command.ExecuteNonQueryAsync();
                    if (paramOutputCollection != null)
                    {
                        paramOutputCollection(command.Parameters);
                    }
                    _connection.Close();
                };
            };            
        }

        public async Task ExecuteCmd(string storedProcedure, ExecuteCmdParams paramCollection, ExecuteCmdReader readerOutput)
        {
            using (SqlConnection _connection = new SqlConnection(_configuration.GetConnectionString("Sql")))
            {
                using (SqlCommand command = new SqlCommand(storedProcedure, _connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    if (paramCollection != null)
                    {
                        paramCollection(command.Parameters);
                    }
                    await _connection.OpenAsync();
                    SqlDataReader dataReader = await command.ExecuteReaderAsync();
                    if (readerOutput != null)
                    {
                        short set = 0;
                        do
                        {
                            while (dataReader.Read())
                            {
                                readerOutput(dataReader, set);
                            }
                            set++;
                        }
                        while (dataReader.NextResult());
                    }
                    _connection.Close();
                };
            };
        }
    }
}
