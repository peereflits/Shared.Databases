using System.Threading.Tasks;

namespace Peereflits.Shared.Databases;

public interface IDatabaseCommand
{
    Task Execute(string statement, int? commandTimeout = null);
    Task Execute(string statement, object? arguments, int? commandTimeout = null);
}