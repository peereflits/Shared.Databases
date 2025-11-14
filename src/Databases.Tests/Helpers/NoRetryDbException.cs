using System.Data.Common;

namespace Peereflits.Shared.Databases.Tests.Helpers;

internal class NoRetryDbException() : DbException("Error", NonTransientErrorCode)
{
    private const int NonTransientErrorCode = 1;
}