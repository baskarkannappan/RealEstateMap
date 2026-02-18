using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RealEstateMap.DAL.Configuration;
using RealEstateMap.DAL.Connection;

namespace RealEstateMap.DAL.Execution;

public sealed class DapperDbExecutor : IDbExecutor
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<DapperDbExecutor> _logger;
    private readonly int _defaultTimeout;

    public DapperDbExecutor(
        IDbConnectionFactory connectionFactory,
        IOptions<DatabaseOptions> options,
        ILogger<DapperDbExecutor> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
        _defaultTimeout = Math.Max(1, options.Value.DefaultCommandTimeoutSeconds);
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, CommandType? commandType = null, int? commandTimeout = null, CancellationToken cancellationToken = default)
    {
        using var connection = await OpenConnectionAsync(cancellationToken);
        var command = CreateCommand(sql, parameters, null, commandType, commandTimeout, cancellationToken);
        return await connection.QueryAsync<T>(command);
    }

    public async Task<T?> QuerySingleAsync<T>(string sql, object? parameters = null, CommandType? commandType = null, int? commandTimeout = null, CancellationToken cancellationToken = default)
    {
        using var connection = await OpenConnectionAsync(cancellationToken);
        var command = CreateCommand(sql, parameters, null, commandType, commandTimeout, cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<T>(command);
    }

    public async Task<int> ExecuteAsync(string sql, object? parameters = null, CommandType? commandType = null, int? commandTimeout = null, CancellationToken cancellationToken = default)
    {
        using var connection = await OpenConnectionAsync(cancellationToken);
        var command = CreateCommand(sql, parameters, null, commandType, commandTimeout, cancellationToken);
        return await connection.ExecuteAsync(command);
    }

    public async Task<T?> ExecuteScalarAsync<T>(string sql, object? parameters = null, CommandType? commandType = null, int? commandTimeout = null, CancellationToken cancellationToken = default)
    {
        using var connection = await OpenConnectionAsync(cancellationToken);
        var command = CreateCommand(sql, parameters, null, commandType, commandTimeout, cancellationToken);
        return await connection.ExecuteScalarAsync<T>(command);
    }

    public async Task<SqlMapper.GridReader> QueryMultipleAsync(string sql, object? parameters = null, CommandType? commandType = null, int? commandTimeout = null, CancellationToken cancellationToken = default)
    {
        var connection = await OpenConnectionAsync(cancellationToken);
        var command = CreateCommand(sql, parameters, null, commandType, commandTimeout, cancellationToken);
        return await connection.QueryMultipleAsync(command);
    }

    public async Task ExecuteTransactionAsync(Func<IDbConnection, IDbTransaction, CancellationToken, Task> action, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
    {
        using var connection = await OpenConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction(isolationLevel);
        try
        {
            await action(connection, transaction, cancellationToken);
            transaction.Commit();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Transaction failed and will be rolled back.");
            transaction.Rollback();
            throw;
        }
    }

    private async Task<IDbConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = _connectionFactory.CreateConnection();
        if (connection is not Microsoft.Data.SqlClient.SqlConnection sqlConnection)
        {
            connection.Open();
            return connection;
        }

        await sqlConnection.OpenAsync(cancellationToken);
        return sqlConnection;
    }

    private CommandDefinition CreateCommand(
        string sql,
        object? parameters,
        IDbTransaction? transaction,
        CommandType? commandType,
        int? commandTimeout,
        CancellationToken cancellationToken)
    {
        var timeout = commandTimeout ?? _defaultTimeout;
        return CommandDefinitionFactory.Create(sql, parameters, transaction, timeout, commandType, cancellationToken);
    }
}
