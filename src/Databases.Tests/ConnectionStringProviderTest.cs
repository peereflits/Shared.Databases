using System;
using Microsoft.Data.SqlClient;
using Xunit;

namespace Peereflits.Shared.Databases.Tests;

public class ConnectionStringProviderTest
{
    private readonly IProvideConnectionString subject = new ConnectionStringProvider();

    [Fact]
    public void WhenThereIsInvalidConnectionInfo_ItShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => subject.Execute(new ConnectionInfo(string.Empty, string.Empty)));
    }

    [Fact]
    public void WhenExecute_WithValidConnectionInfo_ItShouldReturnAConnectionString()
    {
        var server = Guid.NewGuid().ToString();
        var database = Guid.NewGuid().ToString();
        var user = Guid.NewGuid().ToString();
        var password = Guid.NewGuid().ToString();

        var info = new ConnectionInfo(server, database, user, password);

        string result = subject.Execute(info);

        Assert.True(result.Contains(server, StringComparison.OrdinalIgnoreCase));
        Assert.True(result.Contains(database, StringComparison.OrdinalIgnoreCase));
        Assert.True(result.Contains(user, StringComparison.OrdinalIgnoreCase));
        Assert.True(result.Contains(password, StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("localhost", null, null, SqlAuthenticationMethod.ActiveDirectoryIntegrated)]
    [InlineData("localhost", "sa", "password", SqlAuthenticationMethod.SqlPassword)]
    [InlineData("tcp:myserver.database.windows.net,1433", "sa", "password", SqlAuthenticationMethod.SqlPassword)]
    [InlineData("tcp:myserver.database.windows.net,1433", null, null, SqlAuthenticationMethod.ActiveDirectoryManagedIdentity)]
    public void WhenExecute_ItShouldHaveProperAuthenticationMethod(string server, string user, string pass, SqlAuthenticationMethod method)
    {
        var database = Guid.NewGuid().ToString();
        var info = new ConnectionInfo(server, database, user, pass);

        string result = subject.Execute(info);

        var sb = new SqlConnectionStringBuilder(result);
        Assert.Equal(method, sb.Authentication);
    }
}