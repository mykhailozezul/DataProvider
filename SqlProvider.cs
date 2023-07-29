using System.Data.SqlClient;
using System.Data;

namespace DataProvider
{
    internal class SqlProvider
    {
        string _connectionString { get; set; }
        SqlConnection _connection { get; set; }

        public SqlProvider(string connectionString)
        {
            _connectionString = connectionString;
            _connection = new SqlConnection(connectionString);
        }

        public delegate void NonQueryParams(SqlParameterCollection col);
        public delegate void NonQueryOutput(SqlParameterCollection col);
        public delegate void ExecuteCmdParams(SqlParameterCollection col);
        public delegate void ExecuteCmdReader(SqlDataReader DataReader, short set);
        public async Task ExecuteNonQuery(string storedProcedure, NonQueryParams paramCollection, NonQueryOutput paramOutputCollection)
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
        }

        public async Task ExecuteCmd(string storedProcedure, ExecuteCmdParams paramCollection, ExecuteCmdReader readerOutput)
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
        }
    }
}
