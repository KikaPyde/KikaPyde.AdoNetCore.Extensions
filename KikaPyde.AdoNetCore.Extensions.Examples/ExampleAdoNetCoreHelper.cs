using Microsoft.Data.Sqlite;
using Npgsql;
using System.Data.Common;

namespace KikaPyde.AdoNetCore.Extensions.Examples
{
    public static partial class ExampleAdoNetCoreHelper
    {
        static ExampleAdoNetCoreHelper()
        {
            AdoNetCoreHelper.DefaultUsingOptions.AllowTransactionCommitOnSuccessTryFunc = false;
            AdoNetCoreHelper.DefaultUsingOptions.AllowTransactionCommitOnSuccessCatchFunc = false;
            AdoNetCoreHelper.DbConnectionCreating += (builder, e) =>
            {
                e.DbConnection = builder.Func<DbConnection>(
                    sqlite: x => new SqliteConnection(x.ConnectionString),
                    npgsql: x => new NpgsqlConnection(x.ConnectionString));
                e.Handled = true;
            };
        }
    }
}
