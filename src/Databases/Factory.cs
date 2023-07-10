using Microsoft.Extensions.Logging;

namespace Peereflits.Shared.Databases;

public class Factory: IFactory
{
    private readonly ILoggerFactory loggerFactory;

    public Factory(ILoggerFactory loggerFactory) => this.loggerFactory = loggerFactory;

    public IDatabaseQuery CreateQuery(ConnectionInfo info) 
        => new DatabaseQuery(CreateConnection(), info, loggerFactory.CreateLogger<DatabaseQuery>());

    public IDatabaseCommand CreateCommand(ConnectionInfo info) 
        => new DatabaseCommand(CreateConnection(), info, loggerFactory.CreateLogger<DatabaseCommand>());

    private static ICreateDbConnection CreateConnection() => new DbConnectionCreator(new ConnectionStringProvider());
}