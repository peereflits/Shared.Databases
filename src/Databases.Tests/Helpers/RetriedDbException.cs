using System.Data.Common;

namespace Peereflits.Shared.Databases.Tests.Helpers;

internal class RetriedDbException() : DbException("Error", TransientErrorCode)
{
    private const int TransientErrorCode = 4060;
}