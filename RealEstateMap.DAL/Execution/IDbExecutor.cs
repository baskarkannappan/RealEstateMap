using System.Data;
using Dapper;

namespace RealEstateMap.DAL.Execution;

public interface IDbExecutor
{
    Task<IEnumerable<T>> QueryAsync<T>(
        string sql,
        object? parameters = null,
        CommandType? commandType = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default);

    Task<T?> QuerySingleAsync<T>(
        string sql,
        object? parameters = null,
        CommandType? commandType = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default);

    Task<int> ExecuteAsync(
        string sql,
        object? parameters = null,
        CommandType? commandType = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default);

    Task<T?> ExecuteScalarAsync<T>(
        string sql,
        object? parameters = null,
        CommandType? commandType = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default);

    Task<SqlMapper.GridReader> QueryMultipleAsync(
        string sql,
        object? parameters = null,
        CommandType? commandType = null,
        int? commandTimeout = null,
        CancellationToken cancellationToken = default);

    Task ExecuteTransactionAsync(
        Func<IDbConnection, IDbTransaction, CancellationToken, Task> action,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default);
}
