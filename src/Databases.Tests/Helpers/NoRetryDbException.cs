using System.Data.Common;

namespace Peereflits.Shared.Databases.Tests.Helpers;

internal class NoRetryDbException : DbException
{
    public NoRetryDbException() : base("Error", 1) { }
}