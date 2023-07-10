using Microsoft.Data.SqlClient;

namespace Peereflits.Shared.Databases;

internal interface IProvideConnectionString
{
    string Execute(ConnectionInfo info);
}

internal class ConnectionStringProvider : IProvideConnectionString
{
    public string Execute(ConnectionInfo info)
    {
        ConnectionInfo.AssertIsValid(info);

        const int retryCount = 3;
        const int retryInterval = 5;

        var sb = new SqlConnectionStringBuilder
                 {
                     DataSource = info.Server,
                     InitialCatalog = info.Database,
                     ConnectRetryCount = retryCount,
                     ConnectRetryInterval = retryInterval,
                     ConnectTimeout = retryCount * retryInterval + retryCount,
                     Encrypt = true,
                     MultipleActiveResultSets = false,
                     TrustServerCertificate = true,
                     PersistSecurityInfo = false,
                 };

        if(HasUserNamePassword(info))
        {
            sb.UserID = info.User;
            sb.Password = info.Password;
            sb.Authentication = SqlAuthenticationMethod.SqlPassword;
        }
        else
        {
            if(RunsOnAzure(info))
            {
                sb.Authentication = SqlAuthenticationMethod.ActiveDirectoryManagedIdentity;
            }
            else
            {
                sb.IntegratedSecurity = true;
                sb.Authentication = SqlAuthenticationMethod.ActiveDirectoryIntegrated;
            }
        }

        return sb.ToString();
    }

    private static bool RunsOnAzure(ConnectionInfo info) => info.Server.ToLowerInvariant().Contains("database.windows.net");
    private static bool HasUserNamePassword(ConnectionInfo info) => !string.IsNullOrEmpty(info.User) && !string.IsNullOrEmpty(info.Password);
}