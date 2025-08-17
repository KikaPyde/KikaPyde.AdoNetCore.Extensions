using Microsoft.Data.Sqlite;
using Npgsql;
using System;
using System.Data.Common;

namespace KikaPyde.AdoNetCore.Extensions.Examples
{
    public static partial class ExampleAdoNetCoreHelper
    {
        public static T Func<T>(
            this ExampleDbProviderCode code,
            Func<ExampleDbProviderCode, T>? sqlite = null,
            Func<ExampleDbProviderCode, T>? npgsql = null,
            Func<ExampleDbProviderCode, T>? unsupported = null,
            Func<ExampleDbProviderCode, T>? none = null)
            => code switch
            {
                ExampleDbProviderCode.None => none is null
                    ? throw new ArgumentException(nameof(code))
                    : none(code),
                ExampleDbProviderCode.Sqlite => sqlite is null
                    ? throw new NotImplementedException(nameof(sqlite))
                    : sqlite(code),
                ExampleDbProviderCode.Npgsql => npgsql is null
                    ? throw new NotImplementedException(nameof(npgsql))
                    : npgsql(code),
                ExampleDbProviderCode.Unsupported => unsupported is null
                    ? throw new NotSupportedException(code.ToString())
                    : unsupported(code),
                _ => throw new InvalidOperationException(),
            };
        public static DbConnectionStringBuilder CreateDbConnectionStringBuilder(
            this ExampleDbProviderCode code,
            string? dbConnectionString = null)
            => code.Func<DbConnectionStringBuilder>(
                sqlite: x => new SqliteConnectionStringBuilder(dbConnectionString),
                npgsql: x => new NpgsqlConnectionStringBuilder(dbConnectionString));
    }
}
