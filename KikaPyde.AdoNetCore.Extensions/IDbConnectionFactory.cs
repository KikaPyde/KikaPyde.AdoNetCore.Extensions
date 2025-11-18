using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace KikaPyde.AdoNetCore.Extensions
{
    public interface IDbConnectionFactory
    {
        DbConnection CreateDbConnection();
        Task<DbConnection> CreateDbConnectionAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(CreateDbConnection());
    }
}
