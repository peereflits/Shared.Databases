using System;
using System.Collections.Generic;
using Xunit;

namespace Peereflits.Shared.Databases.Tests;

public class ConnectionInfoTest
{
    private const string Server = "server";
    private const string Database = "database";
    private const string UserName = "user";
    private const string Password = "pass";


    public static IEnumerable<object[]> InvalidInfos = new List<object[]>
                                                       {
                                                           new object[] { new ConnectionInfo(null, Database, UserName, Password) },
                                                           new object[] { new ConnectionInfo(Server, "", UserName, Password) },
                                                           new object[] { new ConnectionInfo(Server, Database, " ", Password) },
                                                           new object[] { new ConnectionInfo(Server, Database, UserName, "\r\n") }
                                                       };

    [Theory]
    [MemberData(nameof(InvalidInfos))]
    public void WhenValidationFails_ItShouldThrow(ConnectionInfo info) => Assert.Throws<ArgumentException>(info.AssertIsValid);

    [Fact]
    public void WhenConstructed_WithoutUsernamePassword_ItShouldReturnAValidInstance()
    {
        var result = new ConnectionInfo(Server, Database);

        Assert.Same(Server, result.Server);
        Assert.Same(Database, result.Database);
        Assert.Null(result.User);
        Assert.Null(result.Password);
        result.AssertIsValid();
    }

    [Fact]
    public void WhenConstructed_WithUsernamePassword_ItShouldReturnAValidInstance()
    {
        var result = new ConnectionInfo(Server, Database, UserName, Password);

        Assert.Same(Server, result.Server);
        Assert.Same(Database, result.Database);
        Assert.Same(UserName, result.User);
        Assert.Same(Password, result.Password);

        result.AssertIsValid();
    }
}