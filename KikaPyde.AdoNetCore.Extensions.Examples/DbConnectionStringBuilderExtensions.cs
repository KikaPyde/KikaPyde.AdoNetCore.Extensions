using Microsoft.Data.Sqlite;
using Npgsql;
using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace KikaPyde.AdoNetCore.Extensions.Examples
{
    public static partial class ExampleAdoNetCoreHelper
    {
        public static T Func<T>(
            this DbConnectionStringBuilder? builder,
            Func<SqliteConnectionStringBuilder, T>? sqlite = null,
            Func<NpgsqlConnectionStringBuilder, T>? npgsql = null,
            Func<DbConnectionStringBuilder, T>? unsupported = null,
            Func<DbConnectionStringBuilder?, T>? nullable = null)
            => builder switch
            {
                null => nullable is null
                    ? throw new ArgumentNullException(nameof(builder))
                    : nullable(builder),
                SqliteConnectionStringBuilder sqliteConnectionStringBuilder => sqlite is null
                    ? throw new NotImplementedException(nameof(sqlite))
                    : sqlite(sqliteConnectionStringBuilder),
                NpgsqlConnectionStringBuilder npgsqlConnectionStringBuilder => npgsql is null
                    ? throw new NotImplementedException(nameof(npgsql))
                    : npgsql(npgsqlConnectionStringBuilder),
                _ => unsupported is null
                    ? throw new NotSupportedException(builder.GetType().FullName)
                    : unsupported(builder),
            };
        [return: NotNullIfNotNull(nameof(builder))]
        public static DbConnectionStringBuilder? RequiredAction(
            this DbConnectionStringBuilder? builder,
            Action<SqliteConnectionStringBuilder>? sqlite = null,
            Action<NpgsqlConnectionStringBuilder>? npgsql = null,
            Action<DbConnectionStringBuilder>? unsupported = null,
            Action<DbConnectionStringBuilder?>? nullable = null)
            => builder.Func<DbConnectionStringBuilder?>(
                sqlite: sqlite is null ? null : x => { sqlite(x); return x; },
                npgsql: npgsql is null ? null : x => { npgsql(x); return x; },
                unsupported: unsupported is null ? null : x => { unsupported(x); return x; },
                nullable: nullable is null ? null : x => { nullable(x); return x; });
        [return: NotNullIfNotNull(nameof(builder))]
        public static DbConnectionStringBuilder? OptionalAction(
            this DbConnectionStringBuilder? builder,
            Action<SqliteConnectionStringBuilder>? sqlite = null,
            Action<NpgsqlConnectionStringBuilder>? npgsql = null,
            Action<DbConnectionStringBuilder>? unsupported = null,
            Action<DbConnectionStringBuilder?>? nullable = null)
            => builder.Func<DbConnectionStringBuilder?>(
                sqlite: x => { sqlite?.Invoke(x); return x; },
                npgsql: x => { npgsql?.Invoke(x); return x; },
                unsupported: x => { unsupported?.Invoke(x); return x; },
                nullable: x => { nullable?.Invoke(x); return x; });
        public static ExampleDbProviderCode GetExampleDbProviderCode(
            this DbConnectionStringBuilder? builder)
            => builder.Func(
                sqlite: x => ExampleDbProviderCode.Sqlite,
                npgsql: x => ExampleDbProviderCode.Npgsql,
                unsupported: x => ExampleDbProviderCode.Unsupported,
                nullable: x => ExampleDbProviderCode.None);
        [return: NotNullIfNotNull(nameof(builder))]
        public static DbConnectionStringBuilder? Clone(
            this DbConnectionStringBuilder? builder)
            => builder?.GetExampleDbProviderCode().CreateDbConnectionStringBuilder(builder?.ConnectionString);
    }
}
