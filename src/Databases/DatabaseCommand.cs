using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Peereflits.Shared.Databases;

internal class DatabaseCommand
(
    ICreateDbConnection connection,
    ConnectionInfo info,
    ILogger<DatabaseCommand> logger
) : Database(connection), IDatabaseCommand
{
    public async Task Execute(string statement, int? commandTimeout = null) 
        => await Execute(statement, null, commandTimeout);

    public async Task Execute(string statement, object? arguments, int? commandTimeout = null)
    {
        if(logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug("Executing statement:\r\n{SqlStatement}\r\non {SqlServer}\\{SqlDatabase}", statement, info.Server, info.Database);
        }

        if(string.IsNullOrWhiteSpace(statement))
        {
            throw new ArgumentNullException(nameof(statement));
        }

        var retryCount = 0;

        while(true)
        {
            retryCount++;

            try
            {
                using(IDbConnection con = Connection.Execute(info))
                {
                    if(arguments == null)
                    {
                        await con.ExecuteAsync(statement, commandTimeout: commandTimeout);
                    }
                    else
                    {
                        await con.ExecuteAsync(statement, arguments, commandTimeout: commandTimeout);
                    }
                }

                if(logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Executed statement:\r\n{SqlStatement}\r\non {SqlServer}\\{SqlDatabase}", statement, info.Server, info.Database);
                }
                return;
            }
            catch(DbException ex)
            {
                if(CanRetry(ex, retryCount))
                {
                    logger.LogWarning("Retry {RetryCount} of executing statement: {SqlStatement}", retryCount, statement);
                    await Task.Delay(TransientErrorTimeout);
                }
                else
                {
                    EnrichException(ex, info, statement, arguments);
                    logger.LogError(ex, "Failed to execute statement:\r\n{SqlStatement}\r\non {SqlServer}\\{SqlDatabase}", statement, info.Server, info.Database);
                    throw;
                }
            }
        }
    }
}