using System.Data.Common;
using System.Linq;

namespace Peereflits.Shared.Databases;

internal abstract class Database
(
    ICreateDbConnection connection
)
{
    protected const int MaxRetryCount = 3;
    protected const int TransientErrorTimeout = 5000;

    protected ICreateDbConnection Connection { get; } = connection;

    protected void EnrichException(DbException ex, ConnectionInfo info, string statement, object? arguments = null)
    {
        ex.Data["Server"] = info.Server;
        ex.Data["Database"] = info.Database;
        ex.Data["IsTransient"] = IsTransient(ex);
        ex.Data["Statement"] = statement;

        if(arguments != null)
        {
            ex.Data.Add("Arguments", arguments);
        }
    }

    protected bool CanRetry(DbException ex, int retryCount) => IsTransient(ex) && retryCount <= MaxRetryCount;

    private static bool IsTransient(DbException ex)
    {
        int[] transientErrors =
        {
            4060,
            4221,
            40197,
            40501,
            40613,
            49918,
            49919,
            49920
        };

        return transientErrors.Contains(ex.ErrorCode);
    }
}