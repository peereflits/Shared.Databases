using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Peereflits.Shared.Databases.Tests.Helpers;
using Xunit;
using static NSubstitute.Arg;

namespace Peereflits.Shared.Databases.Tests;

public class DatabaseCommandTest : IClassFixture<DatabaseFixture>
{
    private const string Sql = "PRINT 'Hello World'";

    private readonly DatabaseFixture fixture;
    private readonly ILogger<DatabaseCommand> logger;
    private readonly DatabaseCommand subject;

    public DatabaseCommandTest(DatabaseFixture fixture)
    {
        this.fixture = fixture;
        logger = Substitute.For<ILogger<DatabaseCommand>>();
        subject = new DatabaseCommand(fixture.ConnectionCreator, fixture.ConnectionInfo, logger);
    }

    [Fact]
    [Trait(TestCategories.Key, TestCategories.Integration)]
    public async Task WhenExecute_ItShouldCreateAConnection()
    {
        await subject.Execute(Sql);

        fixture.ConnectionCreator.Received().Execute(fixture.ConnectionInfo);
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

        fixture.ConnectionCreator.Received().Execute(fixture.ConnectionInfo);
    }

    [Fact]
    public async Task WhenExecuteFailsTooOften_ItShouldThrow()
    {
        fixture.ConnectionCreator.Execute(fixture.ConnectionInfo)
               .Returns(
                        x => throw new RetriedDbException(),
                        x => throw new RetriedDbException(),
                        x => throw new RetriedDbException(),
                        x => throw new RetriedDbException(),
                        x => fixture.CreateConnection()
                       );

        await Assert.ThrowsAsync<RetriedDbException>(() => subject.Execute(Sql));
    }

    [Fact]
    public async Task WhenExecuteFails_ItShouldLog()
    {
        fixture.ConnectionCreator.Execute(fixture.ConnectionInfo)
               .Returns(
                        x => throw new NoRetryDbException(),
                        x => fixture.CreateConnection()
                       );

        await Assert.ThrowsAsync<NoRetryDbException>(() => subject.Execute(Sql));

        logger
               .Received()
               .Log(LogLevel.Error,
                    Arg.Any<EventId>(),
                    Arg.Any<AnyType>(),
                    Arg.Any<NoRetryDbException>(),
                    Arg.Any<Func<AnyType, Exception?, string>>()
                   );
    }
}