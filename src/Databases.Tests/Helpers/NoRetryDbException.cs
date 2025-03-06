using System.Data.Common;

namespace Peereflits.Shared.Databases.Tests.Helpers;

internal class NoRetryDbException : DbException
{
    private const int NonTransientErrorCode = 1;
    public NoRetryDbException() : base("Error", NonTransientErrorCode) { }
}