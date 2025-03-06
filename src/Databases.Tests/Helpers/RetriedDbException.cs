using System.Data.Common;

namespace Peereflits.Shared.Databases.Tests.Helpers;

internal class RetriedDbException : DbException
{
    private const int TransientErrorCode = 4060;
    public RetriedDbException() : base("Error", TransientErrorCode) { }
}