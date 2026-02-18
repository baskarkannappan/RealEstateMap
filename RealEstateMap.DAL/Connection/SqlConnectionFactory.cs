using System;
using System.Data;  
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RealEstateMap.DAL.Configuration;
using Microsoft.Data.SqlClient;
namespace RealEstateMap.DAL.Connection;

public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(
        IConfiguration configuration,
        IOptions<DatabaseOptions> databaseOptions)
    {
        var options = databaseOptions.Value;

        _connectionString = configuration.GetConnectionString(options.ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{options.ConnectionStringName}' is missing.");
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
