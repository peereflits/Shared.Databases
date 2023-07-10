using System;
using Microsoft.Data.SqlClient;
using Xunit;

namespace Peereflits.Shared.Databases.Tests;

public class ConnectionStringProviderTest
{
    private readonly IProvideConnectionString subject = new ConnectionStringProvider();

    [Fact]
    public void WhenThereIsNoConnectionInfo_ItShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => subject.Execute(default));
    }

    [Fact]
    public void WhenExecute_WithValidConnectionInfo_ItShouldReturnAConnectionString()
    {
        // Arrange
        var server = Guid.NewGuid().ToString();
        var database = Guid.NewGuid().ToString();
        var user = Guid.NewGuid().ToString();
        var password = Guid.NewGuid().ToString();

        var info = new ConnectionInfo(server, database, user, password);

        // Act
        string result = subject.Execute(info);

        // Assert
        Assert.True(result.Contains(info.Server, StringComparison.OrdinalIgnoreCase));
        Assert.True(result.Contains(info.Database, StringComparison.OrdinalIgnoreCase));
        Assert.True(result.Contains(info.User, StringComparison.OrdinalIgnoreCase));
        Assert.True(result.Contains(info.Password, StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("localhost", null, null, SqlAuthenticationMethod.ActiveDirectoryIntegrated)]
    [InlineData("localhost", "sa", "password", SqlAuthenticationMethod.SqlPassword)]
    [InlineData("tcp:myserver.database.windows.net,1433", "sa", "password", SqlAuthenticationMethod.SqlPassword)]
    [InlineData("tcp:myserver.database.windows.net,1433", null, null, SqlAuthenticationMethod.ActiveDirectoryManagedIdentity)]
    public void WhenExecute_ItShouldHaveProperAuthenticationMethod(string server, string user, string pass, SqlAuthenticationMethod method)
    {
        // Arrange
        var database = Guid.NewGuid().ToString();

        var info = new ConnectionInfo(server, database, user, pass);

        // Act
        string result = subject.Execute(info);

        // Assert
        var sb = new SqlConnectionStringBuilder(result);
        Assert.Equal(method, sb.Authentication);
    }
}