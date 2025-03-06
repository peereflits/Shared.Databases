using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Peereflits.Shared.Databases.Tests.Helpers;
using Xunit;

namespace Peereflits.Shared.Databases.Tests;

public class DatabaseQueryTest : IClassFixture<DatabaseFixture>
{
    private const string Sql = "SELECT 1";

    private readonly DatabaseFixture fixture;
    private readonly MockedLogger<DatabaseQuery> logger;
    private readonly DatabaseQuery subject;

    public DatabaseQueryTest(DatabaseFixture fixture)
    {
        this.fixture = fixture;
        logger = Substitute.For<MockedLogger<DatabaseQuery>>();
        subject = new DatabaseQuery(fixture.ConnectionCreator, fixture.ConnectionInfo, logger);
    }

    [Fact]
    [Trait(TestCategories.Key, TestCategories.Integration)]
    public async Task WhenExecute_ItShouldCreateAConnection()
    {
        IEnumerable<int> result = await subject.Execute<int>(Sql);

        Assert.Equal(1, result.Single());
        fixture.ConnectionCreator
               .Received()
               .Execute(fixture.ConnectionInfo);
    }

    [Fact]
    public async Task WhenExecuteWithoutStatement_ItShouldThrow()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => subject.Execute<int>(""));
    }

    [Fact]
    [Trait(TestCategories.Key, TestCategories.Integration)]
    public async Task WhenExecuteFails_ItShouldRetry()
    {
        fixture
               .ConnectionCreator
               .Execute(fixture.ConnectionInfo)
               .Returns
                        (
                         x => throw new RetriedDbException(),
                         x => fixture.CreateConnection()
                        );

        IEnumerable<int> result = await subject.Execute<int>(Sql);

        Assert.Equal(1, result.Single());
        fixture.ConnectionCreator
               .Received()
               .Execute(fixture.ConnectionInfo);
    }

    [Fact]
    public async Task WhenExecuteFailsTooOften_ItShouldThrow()
    {
        fixture
               .ConnectionCreator
               .Execute(fixture.ConnectionInfo)
               .Returns
                        (
                         x => throw new RetriedDbException(),
                         x => throw new RetriedDbException(),
                         x => throw new RetriedDbException(),
                         x => throw new RetriedDbException(), // MaxRetryCount = 3
                         x => fixture.CreateConnection() // This one should not be called!
                        );

        await Assert.ThrowsAsync<RetriedDbException>(() => subject.Execute<int>(Sql));
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

       _ = await subject.Execute<int>(Sql);

        logger
               .Received(1)
               .Log(LogLevel.Warning,
                    Arg.Is<string>(x=> x.Contains("Retry 1 of executing statement", StringComparison.CurrentCulture)
                                    && x.Contains(Sql, StringComparison.CurrentCulture)
                                  ));
    }

    [Fact]
    public async Task WhenExecuteFails_ItShouldLogError()
    {
        fixture
               .ConnectionCreator
               .Execute(fixture.ConnectionInfo)
               .Returns
                        (
                         x => throw new NoRetryDbException(),
                         x => fixture.CreateConnection() // This one should not be called!
                        );

        await Assert.ThrowsAsync<NoRetryDbException>(() => subject.Execute<int>(Sql));

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