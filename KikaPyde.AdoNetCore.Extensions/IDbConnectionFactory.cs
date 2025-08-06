using System.Data.Common;

namespace KikaPyde.AdoNetCore.Extensions
{
    public interface IDbConnectionFactory
    {
        DbConnection CreateDbConnection();
        Task<DbConnection> CreateDbConnectionAsync(CancellationToken cancellationToken = default)
            => Task.Run(CreateDbConnection, cancellationToken);
    }
}
