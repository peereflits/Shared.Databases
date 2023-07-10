using System.Collections.Generic;
using System.Threading.Tasks;

namespace Peereflits.Shared.Databases;

public interface IDatabaseQuery
{
    Task<IEnumerable<T>> Execute<T>(string statement, int? commandTimeout = null);
    Task<IEnumerable<T>> Execute<T>(string statement, object arguments, int? commandTimeout = null);
}