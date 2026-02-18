using System.Data;
using Dapper;

namespace RealEstateMap.DAL.Execution;

internal static class CommandDefinitionFactory
{
    public static CommandDefinition Create(
        string sql,
        object? parameters,
        IDbTransaction? transaction,
        int? commandTimeout,
        CommandType? commandType,
        CancellationToken cancellationToken) =>
        new(
            commandText: sql,
            parameters: parameters,
            transaction: transaction,
            commandTimeout: commandTimeout,
            commandType: commandType,
            cancellationToken: cancellationToken);
}
