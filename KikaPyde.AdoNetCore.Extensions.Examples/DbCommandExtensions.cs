using Microsoft.Data.Sqlite;
using Npgsql;
using System.Data.Common;

namespace KikaPyde.AdoNetCore.Extensions.Examples
{
    public static partial class ExampleAdoNetCoreHelper
    {
        public static T Func<T>(
            this DbCommand dbCommand,
            Func<SqliteCommand, T>? sqlite = null,
            Func<NpgsqlCommand, T>? npgsql = null)
            => dbCommand switch
            {
                SqliteCommand sqliteCommand => sqlite is null
                    ? throw new NotImplementedException(nameof(sqlite))
                    : sqlite(sqliteCommand),
                NpgsqlCommand npgsqlCommand => npgsql is null
                    ? throw new NotImplementedException(nameof(npgsql))
                    : npgsql(npgsqlCommand),
                null => throw new NullReferenceException(nameof(dbCommand)),
                _ => throw new NotSupportedException(dbCommand.GetType().FullName),
            };
        public static DbCommand RequiredAction(
            this DbCommand dbCommand,
            Action<SqliteCommand>? sqlite = null,
            Action<NpgsqlCommand>? npgsql = null)
            => dbCommand.Func<DbCommand>(
                sqlite: sqlite is null ? null : x => { sqlite(x); return x; },
                npgsql: npgsql is null ? null : x => { npgsql(x); return x; });
        public static DbCommand ChangeCommandText(
            this DbCommand dbCommand,
            string? sqlite = null,
            string? npgsql = null)
            => dbCommand.RequiredAction(
                sqlite: string.IsNullOrEmpty(sqlite) ? null : x => x.CommandText = sqlite,
                npgsql: string.IsNullOrEmpty(npgsql) ? null : x => x.CommandText = npgsql);
        public static DbCommand ChangeParameters(
            this DbCommand dbCommand,
            IEnumerable<SqliteParameter?>? sqlite = null,
            IEnumerable<NpgsqlParameter?>? npgsql = null)
            => dbCommand.RequiredAction(
                sqlite: sqlite is null ? null : sqliteCommand =>
                {
                    sqliteCommand.Parameters.Clear();
                    var sqliteParameters = sqlite.OfType<SqliteParameter>().ToArray();
                    if (sqliteParameters.Length != 0)
                        sqliteCommand.Parameters.AddRange(sqliteParameters);
                },
                npgsql: npgsql is null ? null : npgsqlCommand =>
                {
                    npgsqlCommand.Parameters.Clear();
                    var npgsqlParameters = npgsql.OfType<NpgsqlParameter>().ToArray();
                    if (npgsqlParameters.Length != 0)
                        npgsqlCommand.Parameters.AddRange(npgsqlParameters);
                });
    }
}
