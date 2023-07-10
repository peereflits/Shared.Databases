using System.Data.Common;

namespace Peereflits.Shared.Databases.Tests.Helpers;

internal class RetriedDbException : DbException
{
    public RetriedDbException() : base("Error", 4060) { }
}