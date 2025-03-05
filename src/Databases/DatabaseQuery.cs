using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;

namespace Peereflits.Shared.Databases;

internal class DatabaseQuery : Database, IDatabaseQuery
{
    private readonly ConnectionInfo info;
    private readonly ILogger<DatabaseQuery> logger;

    public DatabaseQuery
    (
        ICreateDbConnection connection,
        ConnectionInfo info,
        ILogger<DatabaseQuery> logger
    ) : base(connection)
    {
        this.info = info;
        this.logger = logger;
    }

    public async Task<IEnumerable<TResult>> Execute<TResult>(string statement, int? commandTimeout = null) 
        => await Execute<TResult>(statement, null, commandTimeout);

    public async Task<IEnumerable<T>> Execute<T>(string statement, object? arguments, int? commandTimeout = null)
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
                var result = new List<T>();

                using(IDbConnection con = Connection.Execute(info))
                {
                    result.AddRange(arguments == null
                                            ? await con.QueryAsync<T>(statement, commandTimeout: commandTimeout)
                                            : await con.QueryAsync<T>(statement, arguments, commandTimeout: commandTimeout)
                                   );
                }

                if(logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Executed statement:\r\n{SqlStatement}\r\non {SqlServer}\\{SqlDatabase}", statement, info?.Server, info?.Database);
                }
                return result;
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
                    logger.LogError(ex, "Failed to execute statement:\r\n{SqlStatement}\r\non {SqlServer}\\{SqlDatabase}", statement, info?.Server, info?.Database);
                    throw;
                }
            }
        }
    }
}