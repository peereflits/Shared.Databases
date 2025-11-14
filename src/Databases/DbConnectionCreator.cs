using System.Data;
using Microsoft.Data.SqlClient;

namespace Peereflits.Shared.Databases;

internal interface ICreateDbConnection
{
    IDbConnection Execute(ConnectionInfo info);
}

internal class DbConnectionCreator
(
    IProvideConnectionString provider
) : ICreateDbConnection
{
    public IDbConnection Execute(ConnectionInfo info)
    {
        ConnectionInfo.AssertIsValid(info);

        string connection = provider.Execute(info);

        return new SqlConnection(connection);
    }
}