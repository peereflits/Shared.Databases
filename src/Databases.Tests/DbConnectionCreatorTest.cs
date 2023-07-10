using System.Data;
using Microsoft.Data.SqlClient;
using NSubstitute;
using Xunit;

namespace Peereflits.Shared.Databases.Tests;

public class DbConnectionCreatorTest
{
    private readonly IProvideConnectionString provider;

    private readonly DbConnectionCreator subject;

    public DbConnectionCreatorTest()
    {
        provider = Substitute.For<IProvideConnectionString>();
        subject = new DbConnectionCreator(provider);
    }

    [Fact]
    public void WhenCreate_ItShouldReturnAnInstance()
    {
        provider.Execute(Arg.Any<ConnectionInfo>())
                .Returns("Server=.;Database=master;Trusted_Connection=True;");

        var info = new ConnectionInfo(".", "master");
        IDbConnection result = subject.Execute(info);

        Assert.NotNull(result);
        Assert.IsType<SqlConnection>(result);
        provider.Received(1).Execute(info);
    }
}