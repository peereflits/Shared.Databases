using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Peereflits.Shared.Databases.Tests.Helpers;
using Xunit;

namespace Peereflits.Shared.Databases.Tests;

public class DatabaseCommandTest : IClassFixture<DatabaseFixture>
{
    private const string Sql = "PRINT 'Hello World'";

    private readonly DatabaseFixture fixture;
    private readonly MockedLogger<DatabaseCommand> logger;
    private readonly DatabaseCommand subject;

    public DatabaseCommandTest(DatabaseFixture fixture)
    {
        this.fixture = fixture;
        logger = Substitute.For<MockedLogger<DatabaseCommand>>();
        subject = new DatabaseCommand(fixture.ConnectionCreator, fixture.ConnectionInfo, logger);
    }

    [Fact]
    [Trait(TestCategories.Key, TestCategories.Integration)]
    public async Task WhenExecute_ItShouldCreateAConnection()
    {
        await subject.Execute(Sql);

        fixture.ConnectionCreator
               .Received()
               .Execute(fixture.ConnectionInfo);
    }

    [Fact]
    public async Task WhenExecuteWithoutStatement_ItShouldThrow()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => subject.Execute(""));
    }

    [Fact]
    [Trait(TestCategories.Key, TestCategories.Integration)]
    public async Task WhenExecuteFails_ItShouldRetry()
    {
        fixture.ConnectionCreator.Execute(fixture.ConnectionInfo)
               .Returns(
                        x => throw new RetriedDbException(),
                        x => fixture.CreateConnection()
                       );

        await subject.Execute(Sql);

        fixture.ConnectionCreator
               .Received()
               .Execute(fixture.ConnectionInfo);
    }

    [Fact]
    [Trait(TestCategories.Key, TestCategories.Integration)]
    public async Task WhenExecute_IsRetried_ItShouldLogWarning()
    {
        fixture
               .ConnectionCreator
               .Execute(fixture.ConnectionInfo)
               .Returns
                        (
                         x => throw new RetriedDbException(),
                         x => fixture.CreateConnection()
                        );

        await subject.Execute(Sql);

        logger
               .Received(1)
               .Log(LogLevel.Warning,
                    Arg.Is<string>(x=> x.Contains("Retry 1 of executing statement", StringComparison.CurrentCulture)
                                    && x.Contains(Sql, StringComparison.CurrentCulture)
                                  ));
    }

    [Fact]
    public async Task WhenExecuteFailsTooOften_ItShouldThrow()
    {
        fixture.ConnectionCreator.Execute(fixture.ConnectionInfo)
               .Returns(
                        x => throw new RetriedDbException(),
                        x => throw new RetriedDbException(),
                        x => throw new RetriedDbException(),
                        x => throw new RetriedDbException(), // MaxRetryCount = 3
                        x => fixture.CreateConnection() // This one should not be called!
                       );

        await Assert.ThrowsAsync<RetriedDbException>(() => subject.Execute(Sql));
    }

    [Fact]
    public async Task WhenExecuteFails_ItShouldLog()
    {
        fixture.ConnectionCreator.Execute(fixture.ConnectionInfo)
               .Returns(
                        x => throw new NoRetryDbException(),
                        x => fixture.CreateConnection() // This one should not be called!
                       );

        await Assert.ThrowsAsync<NoRetryDbException>(() => subject.Execute(Sql));

        logger
               .Received()
               .Log(LogLevel.Error,
                    Arg.Is<string>(x=> x.Contains("Failed to execute statement", StringComparison.CurrentCulture)
                                    && x.Contains(Sql, StringComparison.CurrentCulture)
                                    && x.Contains(fixture.ConnectionInfo.Server)
                                    && x.Contains(fixture.ConnectionInfo.Database)
                                  ));
    }
}