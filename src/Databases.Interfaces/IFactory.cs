namespace Peereflits.Shared.Databases;

public interface IFactory
{
    IDatabaseQuery CreateQuery(ConnectionInfo info);
    IDatabaseCommand CreateCommand(ConnectionInfo info);
}