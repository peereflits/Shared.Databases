using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace Peereflits.Shared.Databases.Tests.Helpers;

public class DatabaseFixture
{
    private readonly TestSettings settings;

    internal readonly ConnectionInfo ConnectionInfo;
    internal readonly ICreateDbConnection ConnectionCreator;

    public DatabaseFixture()
    {
        settings = GetTestSettings();
        ConnectionInfo = new ConnectionInfo(settings.DbServerName, "master");

        ConnectionCreator = Substitute.For<ICreateDbConnection>();

        ConnectionCreator
               .Execute(Arg.Any<ConnectionInfo>())
               .Returns(CreateConnection());
    }

    internal IDbConnection CreateConnection()
    {
        var sb = new SqlConnectionStringBuilder
        {
            DataSource = ConnectionInfo.Server,
            InitialCatalog = ConnectionInfo.Database,
            MultipleActiveResultSets = false,
            TrustServerCertificate = true,
            PersistSecurityInfo = false,
            IntegratedSecurity = settings.IsTrusted
        };

        return new SqlConnection(sb.ToString());
    }

    private TestSettings GetTestSettings()
    {
        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        const string defaultTestSettings = "testsettings.json";
        var environmentTestSettings = $"testsettings.{environment}.json";

        IConfigurationRoot builder = new ConfigurationBuilder()
                                    .AddJsonFile(defaultTestSettings)
                                    .AddJsonFile(environmentTestSettings, true)
                                    .Build();

        var result = new TestSettings();
        builder.GetSection(nameof(TestSettings)).Bind(result);

        return result;
    }
}