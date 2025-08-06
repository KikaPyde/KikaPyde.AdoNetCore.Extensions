using System.Data.Common;

namespace KikaPyde.AdoNetCore.Extensions
{
    public static partial class AdoNetCoreHelper
    {
        #region Using
        private static T InternalUsing<T>(
            this IDbConnectionFactory factory,
            Func<DbConnection, T> tryFunc,
            Func<DbConnection?, Exception, T>? catchFunc = null)
            => InternalUsing(
                factory: factory.CreateDbConnection,
                tryFunc: tryFunc,
                catchFunc: catchFunc);
        public static T Using<T>(
            this IDbConnectionFactory factory,
            Func<DbConnection, T> tryFunc,
            Func<DbConnection?, Exception, T>? catchFunc = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.Using(
                    tryFunc: tryFunc,
                    catchFunc: catchFunc,
                    usingOptions: usingOptions),
                catchFunc: catchFunc);
        public static T Using<T>(
            this IDbConnectionFactory factory,
            Func<DbConnection, DbTransaction, T> tryFunc,
            Func<DbConnection?, DbTransaction?, Exception, T>? catchFunc = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.Using(
                    tryFunc: tryFunc,
                    catchFunc: catchFunc,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions),
                catchFunc: catchFunc is null ? null : (dbConnection, exception) => catchFunc(dbConnection, null, exception));
        public static T Using<T>(
            this IDbConnectionFactory factory,
            Func<DbConnection, DbCommand, T> tryFunc,
            Func<DbConnection?, DbCommand?, Exception, T>? catchFunc = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.Using(
                    tryFunc: tryFunc,
                    catchFunc: catchFunc,
                    usingOptions: usingOptions),
                catchFunc: catchFunc is null ? null : (dbConnection, exception) => catchFunc(dbConnection, null, exception));
        public static T Using<T>(
            this IDbConnectionFactory factory,
            Func<DbConnection, DbBatch, T> tryFunc,
            Func<DbConnection?, DbBatch?, Exception, T>? catchFunc = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.Using(
                    tryFunc: tryFunc,
                    catchFunc: catchFunc,
                    usingOptions: usingOptions),
                catchFunc: catchFunc is null ? null : (dbConnection, exception) => catchFunc(dbConnection, null, exception));
        public static T Using<T>(
            this IDbConnectionFactory factory,
            Func<DbConnection, DbTransaction, DbCommand, T> tryFunc,
            Func<DbConnection?, DbTransaction?, DbCommand?, Exception, T>? catchFunc = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.Using(
                    tryFunc: tryFunc,
                    catchFunc: catchFunc,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions),
                catchFunc: catchFunc is null ? null : (dbConnection, exception) => catchFunc(dbConnection, null, null, exception));
        public static T Using<T>(
            this IDbConnectionFactory factory,
            Func<DbConnection, DbTransaction, DbBatch, T> tryFunc,
            Func<DbConnection?, DbTransaction?, DbBatch?, Exception, T>? catchFunc = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.Using(
                    tryFunc: tryFunc,
                    catchFunc: catchFunc,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions),
                catchFunc: catchFunc is null ? null : (dbConnection, exception) => catchFunc(dbConnection, null, null, exception));
        public static T Using<T>(
            this IDbConnectionFactory factory,
            Func<DbConnection, DbCommand, DbDataReader, T> tryFunc,
            Func<DbConnection?, DbCommand?, DbDataReader?, Exception, T>? catchFunc = null,
            CommandBehavior? commandBehavior = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.Using(
                    tryFunc: tryFunc,
                    catchFunc: catchFunc,
                    commandBehavior: commandBehavior,
                    usingOptions: usingOptions),
                catchFunc: catchFunc is null ? null : (dbConnection, exception) => catchFunc(dbConnection, null, null, exception));
        public static T Using<T>(
            this IDbConnectionFactory factory,
            Func<DbConnection, DbBatch, DbDataReader, T> tryFunc,
            Func<DbConnection?, DbBatch?, DbDataReader?, Exception, T>? catchFunc = null,
            CommandBehavior? commandBehavior = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.Using(
                    tryFunc: tryFunc,
                    catchFunc: catchFunc,
                    commandBehavior: commandBehavior,
                    usingOptions: usingOptions),
                catchFunc: catchFunc is null ? null : (dbConnection, exception) => catchFunc(dbConnection, null, null, exception));
        public static T Using<T>(
            this IDbConnectionFactory factory,
            Func<DbConnection, DbTransaction, DbCommand, DbDataReader, T> tryFunc,
            Func<DbConnection?, DbTransaction?, DbCommand?, DbDataReader?, Exception, T>? catchFunc = null,
            IsolationLevel? isolationLevel = null,
            CommandBehavior? commandBehavior = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.Using(
                    tryFunc: tryFunc,
                    catchFunc: catchFunc,
                    isolationLevel: isolationLevel,
                    commandBehavior: commandBehavior,
                    usingOptions: usingOptions),
                catchFunc: catchFunc is null ? null : (dbConnection, exception) => catchFunc(dbConnection, null, null, null, exception));
        public static T Using<T>(
            this IDbConnectionFactory factory,
            Func<DbConnection, DbTransaction, DbBatch, DbDataReader, T> tryFunc,
            Func<DbConnection?, DbTransaction?, DbBatch?, DbDataReader?, Exception, T>? catchFunc = null,
            IsolationLevel? isolationLevel = null,
            CommandBehavior? commandBehavior = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.Using(
                    tryFunc: tryFunc,
                    catchFunc: catchFunc,
                    isolationLevel: isolationLevel,
                    commandBehavior: commandBehavior,
                    usingOptions: usingOptions),
                catchFunc: catchFunc is null ? null : (dbConnection, exception) => catchFunc(dbConnection, null, null, null, exception));
        #endregion
        #region UsingAsync
        private static async Task<T> InternalUsingAsync<T>(
            this IDbConnectionFactory factory,
            Func<DbConnection, CancellationToken, CancellationToken?, Task<T>> tryFunc,
            Func<DbConnection?, Exception, CancellationToken, Task<T>>? catchFunc = null,
            CancellationToken cancellationToken = default,
            CancellationToken? catchCancellationToken = default)
            => await InternalUsingAsync(
                factory: factory.CreateDbConnectionAsync,
                tryFunc: tryFunc,
                catchFunc: catchFunc,
                cancellationToken: cancellationToken,
                catchCancellationToken: catchCancellationToken);
        private static async Task<T> InternalUsingAsync<T>(
            this IDbConnectionFactory factory,
            Func<DbConnection, CancellationToken, Task<T>> tryFunc,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken, catchCancellationToken) => await tryFunc(dbConnection, cancellationToken),
                catchFunc: null,
                cancellationToken: cancellationToken,
                catchCancellationToken: null);
        public static async Task<T> UsingAsync<T>(
            this IDbConnectionFactory factory,
            Func<DbConnection, CancellationToken, Task<T>> tryFunc,
            Func<DbConnection?, Exception, CancellationToken, Task<T>>? catchFunc = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default,
            CancellationToken? catchCancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken, catchCancellationToken) => await dbConnection.UsingAsync(
                    tryFunc: tryFunc,
                    catchFunc: catchFunc,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken,
                    catchCancellationToken: catchCancellationToken),
                catchFunc: catchFunc,
                cancellationToken: cancellationToken,
                catchCancellationToken: catchCancellationToken);
        public static async Task<T> UsingAsync<T>(
            this IDbConnectionFactory factory,
            Func<DbConnection, DbTransaction, CancellationToken, Task<T>> tryFunc,
            Func<DbConnection?, DbTransaction?, Exception, CancellationToken, Task<T>>? catchFunc = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default,
            CancellationToken? catchCancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken, catchCancellationToken) => await dbConnection.UsingAsync(
                    tryFunc: tryFunc,
                    catchFunc: catchFunc,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken,
                    catchCancellationToken: catchCancellationToken),
                catchFunc: catchFunc is null ? null : async (dbConnection, exception, cancellationToken) => await catchFunc(
                    dbConnection,
                    null,
                    exception,
                    cancellationToken),
                cancellationToken: cancellationToken,
                catchCancellationToken: catchCancellationToken);
        public static async Task<T> UsingAsync<T>(
            this IDbConnectionFactory factory,
            Func<DbConnection, DbCommand, CancellationToken, Task<T>> tryFunc,
            Func<DbConnection?, DbCommand?, Exception, CancellationToken, Task<T>>? catchFunc = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default,
            CancellationToken? catchCancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken, catchCancellationToken) => await dbConnection.UsingAsync(
                    tryFunc: tryFunc,
                    catchFunc: catchFunc,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken,
                    catchCancellationToken: catchCancellationToken),
                catchFunc: catchFunc is null ? null : async (dbConnection, exception, cancellationToken) => await catchFunc(
                    dbConnection,
                    null,
                    exception,
                    cancellationToken),
                cancellationToken: cancellationToken,
                catchCancellationToken: catchCancellationToken);
        public static async Task<T> UsingAsync<T>(
            this IDbConnectionFactory factory,
            Func<DbConnection, DbBatch, CancellationToken, Task<T>> tryFunc,
            Func<DbConnection?, DbBatch?, Exception, CancellationToken, Task<T>>? catchFunc = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default,
            CancellationToken? catchCancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken, catchCancellationToken) => await dbConnection.UsingAsync(
                    tryFunc: tryFunc,
                    catchFunc: catchFunc,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken,
                    catchCancellationToken: catchCancellationToken),
                catchFunc: catchFunc is null ? null : async (dbConnection, exception, cancellationToken) => await catchFunc(
                    dbConnection,
                    null,
                    exception,
                    cancellationToken),
                cancellationToken: cancellationToken,
                catchCancellationToken: catchCancellationToken);
        public static async Task<T> UsingAsync<T>(
            this IDbConnectionFactory factory,
            Func<DbConnection, DbTransaction, DbCommand, CancellationToken, Task<T>> tryFunc,
            Func<DbConnection?, DbTransaction?, DbCommand?, Exception, CancellationToken, Task<T>>? catchFunc = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default,
            CancellationToken? catchCancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken, catchCancellationToken) => await dbConnection.UsingAsync(
                    tryFunc: tryFunc,
                    catchFunc: catchFunc,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken,
                    catchCancellationToken: catchCancellationToken),
                catchFunc: catchFunc is null ? null : async (dbConnection, exception, cancellationToken) => await catchFunc(
                    dbConnection,
                    null,
                    null,
                    exception,
                    cancellationToken),
                cancellationToken: cancellationToken,
                catchCancellationToken: catchCancellationToken);
        public static async Task<T> UsingAsync<T>(
            this IDbConnectionFactory factory,
            Func<DbConnection, DbTransaction, DbBatch, CancellationToken, Task<T>> tryFunc,
            Func<DbConnection?, DbTransaction?, DbBatch?, Exception, CancellationToken, Task<T>>? catchFunc = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default,
            CancellationToken? catchCancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken, catchCancellationToken) => await dbConnection.UsingAsync(
                    tryFunc: tryFunc,
                    catchFunc: catchFunc,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken,
                    catchCancellationToken: catchCancellationToken),
                catchFunc: catchFunc is null ? null : async (dbConnection, exception, cancellationToken) => await catchFunc(
                    dbConnection,
                    null,
                    null,
                    exception,
                    cancellationToken),
                cancellationToken: cancellationToken,
                catchCancellationToken: catchCancellationToken);
        public static async Task<T> UsingAsync<T>(
            this IDbConnectionFactory factory,
            Func<DbConnection, DbCommand, DbDataReader, CancellationToken, Task<T>> tryFunc,
            Func<DbConnection?, DbCommand?, DbDataReader?, Exception, CancellationToken, Task<T>>? catchFunc = null,
            CommandBehavior? commandBehavior = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default,
            CancellationToken? catchCancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken, catchCancellationToken) => await dbConnection.UsingAsync(
                    tryFunc: tryFunc,
                    catchFunc: catchFunc,
                    commandBehavior: commandBehavior,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken,
                    catchCancellationToken: catchCancellationToken),
                catchFunc: catchFunc is null ? null : async (dbConnection, exception, cancellationToken) => await catchFunc(
                    dbConnection,
                    null,
                    null,
                    exception,
                    cancellationToken),
                cancellationToken: cancellationToken,
                catchCancellationToken: catchCancellationToken);
        public static async Task<T> UsingAsync<T>(
            this IDbConnectionFactory factory,
            Func<DbConnection, DbBatch, DbDataReader, CancellationToken, Task<T>> tryFunc,
            Func<DbConnection?, DbBatch?, DbDataReader?, Exception, CancellationToken, Task<T>>? catchFunc = null,
            CommandBehavior? commandBehavior = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default,
            CancellationToken? catchCancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken, catchCancellationToken) => await dbConnection.UsingAsync(
                    tryFunc: tryFunc,
                    catchFunc: catchFunc,
                    commandBehavior: commandBehavior,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken,
                    catchCancellationToken: catchCancellationToken),
                catchFunc: catchFunc is null ? null : async (dbConnection, exception, cancellationToken) => await catchFunc(
                    dbConnection,
                    null,
                    null,
                    exception,
                    cancellationToken),
                cancellationToken: cancellationToken,
                catchCancellationToken: catchCancellationToken);
        public static async Task<T> UsingAsync<T>(
            this IDbConnectionFactory factory,
            Func<DbConnection, DbTransaction, DbCommand, DbDataReader, CancellationToken, Task<T>> tryFunc,
            Func<DbConnection?, DbTransaction?, DbCommand?, DbDataReader?, Exception, CancellationToken, Task<T>>? catchFunc = null,
            IsolationLevel? isolationLevel = null,
            CommandBehavior? commandBehavior = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default,
            CancellationToken? catchCancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken, catchCancellationToken) => await dbConnection.UsingAsync(
                    tryFunc: tryFunc,
                    catchFunc: catchFunc,
                    isolationLevel: isolationLevel,
                    commandBehavior: commandBehavior,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken,
                    catchCancellationToken: catchCancellationToken),
                catchFunc: catchFunc is null ? null : async (dbConnection, exception, cancellationToken) => await catchFunc(
                    dbConnection,
                    null,
                    null,
                    null,
                    exception,
                    cancellationToken),
                cancellationToken: cancellationToken,
                catchCancellationToken: catchCancellationToken);
        public static async Task<T> UsingAsync<T>(
            this IDbConnectionFactory factory,
            Func<DbConnection, DbTransaction, DbBatch, DbDataReader, CancellationToken, Task<T>> tryFunc,
            Func<DbConnection?, DbTransaction?, DbBatch?, DbDataReader?, Exception, CancellationToken, Task<T>>? catchFunc = null,
            IsolationLevel? isolationLevel = null,
            CommandBehavior? commandBehavior = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default,
            CancellationToken? catchCancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken, catchCancellationToken) => await dbConnection.UsingAsync(
                    tryFunc: tryFunc,
                    catchFunc: catchFunc,
                    isolationLevel: isolationLevel,
                    commandBehavior: commandBehavior,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken,
                    catchCancellationToken: catchCancellationToken),
                catchFunc: catchFunc is null ? null : async (dbConnection, exception, cancellationToken) => await catchFunc(
                    dbConnection,
                    null,
                    null,
                    null,
                    exception,
                    cancellationToken),
                cancellationToken: cancellationToken,
                catchCancellationToken: catchCancellationToken);
        #endregion
        #region Execute DbCommand
        public static int ExecuteNonQuery(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand>? afterExecute = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConenction => dbConenction.ExecuteNonQuery(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions));
        public static T ExecuteReader<T>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, T>? constructor = null,
            Action<DbCommand, T>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.ExecuteReader(
                    beforeExecute: beforeExecute,
                    constructor: constructor,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions));
        public static object? ExecuteScalar(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, object?>? afterExecute = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.ExecuteScalar(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions));
        public static T? ExecuteScalar<T>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Func<object?, T?>? constructor = null,
            Action<DbCommand, T?>? afterExecute = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.ExecuteScalar(
                    beforeExecute: beforeExecute,
                    constructor: constructor,
                    afterExecute: afterExecute,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions));
        #endregion
        #region Execute DbBatch
        public static int ExecuteNonQuery(
            this IDbConnectionFactory factory,
            Action<DbBatch> beforeExecute,
            Action<DbBatch>? afterExecute = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConenction => dbConenction.ExecuteNonQuery(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions));
        public static T ExecuteReader<T>(
            this IDbConnectionFactory factory,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, T>? constructor = null,
            Action<DbBatch, T>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.ExecuteReader(
                    beforeExecute: beforeExecute,
                    constructor: constructor,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions));
        public static object? ExecuteScalar(
            this IDbConnectionFactory factory,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, object?>? afterExecute = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.ExecuteScalar(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions));
        public static T? ExecuteScalar<T>(
            this IDbConnectionFactory factory,
            Action<DbBatch> beforeExecute,
            Func<object?, T?>? constructor = null,
            Action<DbBatch, T?>? afterExecute = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.ExecuteScalar(
                    beforeExecute: beforeExecute,
                    constructor: constructor,
                    afterExecute: afterExecute,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions));
        #endregion
        #region ExecuteAsync DbCommand
        public static async Task<int> ExecuteNonQueryAsync(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand>? afterExecute = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken, catchCancellationToken) => await dbConnection.ExecuteNonQueryAsync(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<T> ExecuteReaderAsync<T>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<T>>? constructor = null,
            Action<DbCommand, T>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken, catchCancellationToken) => await dbConnection.ExecuteReaderAsync(
                    beforeExecute: beforeExecute,
                    constructor: constructor,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<object?> ExecuteScalarAsync(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, object?>? afterExecute = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken, catchCancellationToken) => await dbConnection.ExecuteScalarAsync(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<T?> ExecuteScalarAsync<T>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Func<object?, T?>? constructor = null,
            Action<DbCommand, T?>? afterExecute = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken, catchCancellationToken) => await dbConnection.ExecuteScalarAsync(
                    beforeExecute: beforeExecute,
                    constructor: constructor,
                    afterExecute: afterExecute,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        #endregion
        #region ExecuteAsync DbBatch
        public static async Task<int> ExecuteNonQueryAsync(
            this IDbConnectionFactory factory,
            Action<DbBatch> beforeExecute,
            Action<DbBatch>? afterExecute = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken, catchCancellationToken) => await dbConnection.ExecuteNonQueryAsync(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<T> ExecuteReaderAsync<T>(
            this IDbConnectionFactory factory,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<T>>? constructor = null,
            Action<DbBatch, T>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken, catchCancellationToken) => await dbConnection.ExecuteReaderAsync(
                    beforeExecute: beforeExecute,
                    constructor: constructor,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<object?> ExecuteScalarAsync(
            this IDbConnectionFactory factory,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, object?>? afterExecute = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken, catchCancellationToken) => await dbConnection.ExecuteScalarAsync(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<T?> ExecuteScalarAsync<T>(
            this IDbConnectionFactory factory,
            Action<DbBatch> beforeExecute,
            Func<object?, T?>? constructor = null,
            Action<DbBatch, T?>? afterExecute = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken, catchCancellationToken) => await dbConnection.ExecuteScalarAsync(
                    beforeExecute: beforeExecute,
                    constructor: constructor,
                    afterExecute: afterExecute,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        #endregion
        #region GetEnumerable DbCommand
        public static TCollection GetCollection<TCollection, T>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            Action<DbCommand, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            where TCollection : ICollection<T>, new()
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.GetCollection(
                    beforeExecute: beforeExecute,
                    constructorByIndex: constructorByIndex,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions));
        public static TCollection GetCollection<TCollection, T>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbCommand, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            where TCollection : ICollection<T>, new()
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.GetCollection(
                    beforeExecute: beforeExecute,
                    constructor: constructor,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions));
        public static List<T> GetList<T>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            Action<DbCommand, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.GetList(
                    beforeExecute: beforeExecute,
                    constructorByIndex: constructorByIndex,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions));
        public static List<T> GetList<T>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbCommand, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.GetList(
                    beforeExecute: beforeExecute,
                    constructor: constructor,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions));
        public static Dictionary<int, T> GetDictionary<T>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            Action<DbCommand, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.GetDictionary(
                    beforeExecute: beforeExecute,
                    constructorByIndex: constructorByIndex,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions));
        public static Dictionary<int, T> GetDictionary<T>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbCommand, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.GetDictionary(
                    beforeExecute: beforeExecute,
                    constructor: constructor,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions));
        public static DataTable GetDataTable(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, DataTable>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.GetDataTable(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions));
        public static DataTable? GetSchemaTable(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, DataTable?>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.GetSchemaTable(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions));
        public static DataTable GetSchema(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, DataTable>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.GetSchema());
        public static Dictionary<int, Dictionary<string, object?>> GetRawTable(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, Dictionary<int, Dictionary<string, object?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.GetRawTable(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions));
        public static List<Tuple<T1?, T2?>> GetTuples<T1, T2>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.GetTuples<T1, T2>(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions));
        public static List<Tuple<T1?, T2?, T3?>> GetTuples<T1, T2, T3>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.GetTuples<T1, T2, T3>(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions));
        public static List<Tuple<T1?, T2?, T3?, T4?>> GetTuples<T1, T2, T3, T4>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.GetTuples<T1, T2, T3, T4>(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?>> GetTuples<T1, T2, T3, T4, T5>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.GetTuples<T1, T2, T3, T4, T5>(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>> GetTuples<T1, T2, T3, T4, T5, T6>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.GetTuples<T1, T2, T3, T4, T5, T6>(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>> GetTuples<T1, T2, T3, T4, T5, T6, T7>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.GetTuples<T1, T2, T3, T4, T5, T6, T7>(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.GetTuples<T1, T2, T3, T4, T5, T6, T7, T8>(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions));
        #endregion
        #region GetEnumerableAsync DbCommand
        public static async Task<TCollection> GetCollectionAsync<TCollection, T>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            Action<DbCommand, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken) => await dbConnection.GetCollectionAsync(
                    beforeExecute: beforeExecute,
                    constructorByIndex: constructorByIndex,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetCollectionAsync<TCollection, T>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            Action<DbCommand, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken) => await dbConnection.GetCollectionAsync(
                    beforeExecute: beforeExecute,
                    constructor: constructor,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetCollectionAsync<TCollection, T>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex,
            Action<DbCommand, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken) => await dbConnection.GetCollectionAsync(
                    beforeExecute: beforeExecute,
                    constructorByIndex: constructorByIndex,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetCollectionAsync<TCollection, T>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbCommand, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken) => await dbConnection.GetCollectionAsync(
                    beforeExecute: beforeExecute,
                    constructor: constructor,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetListAsync<T>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            Action<DbCommand, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken) => await dbConnection.GetListAsync(
                    beforeExecute: beforeExecute,
                    constructorByIndex: constructorByIndex,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetListAsync<T>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            Action<DbCommand, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken) => await dbConnection.GetListAsync(
                    beforeExecute: beforeExecute,
                    constructor: constructor,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetListAsync<T>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex,
            Action<DbCommand, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken) => await dbConnection.GetListAsync(
                    beforeExecute: beforeExecute,
                    constructorByIndex: constructorByIndex,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetListAsync<T>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbCommand, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken) => await dbConnection.GetListAsync(
                    beforeExecute: beforeExecute,
                    constructor: constructor,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetDictionaryAsync<T>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            Action<DbCommand, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken) => await dbConnection.GetDictionaryAsync(
                    beforeExecute: beforeExecute,
                    constructorByIndex: constructorByIndex,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetDictionaryAsync<T>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            Action<DbCommand, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken) => await dbConnection.GetDictionaryAsync(
                    beforeExecute: beforeExecute,
                    constructor: constructor,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetDictionaryAsync<T>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex,
            Action<DbCommand, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken) => await dbConnection.GetDictionaryAsync(
                    beforeExecute: beforeExecute,
                    constructorByIndex: constructorByIndex,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetDictionaryAsync<T>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbCommand, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken) => await dbConnection.GetDictionaryAsync(
                    beforeExecute: beforeExecute,
                    constructor: constructor,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<DataTable> GetDataTableAsync(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, DataTable>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken) => await dbConnection.GetDataTableAsync(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<DataTable?> GetSchemaTableAsync(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, DataTable?>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken) => await dbConnection.GetSchemaTableAsync(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<DataTable> GetSchemaAsync(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, DataTable>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken) => await dbConnection.GetSchemaAsync(
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, Dictionary<string, object?>>> GetRawTableAsync(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, Dictionary<int, Dictionary<string, object?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken) => await dbConnection.GetRawTableAsync(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?>>> GetTuplesAsync<T1, T2>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken) => await dbConnection.GetTuplesAsync(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?>>> GetTuplesAsync<T1, T2, T3>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken) => await dbConnection.GetTuplesAsync(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?>>> GetTuplesAsync<T1, T2, T3, T4>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken) => await dbConnection.GetTuplesAsync(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?>>> GetTuplesAsync<T1, T2, T3, T4, T5>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken) => await dbConnection.GetTuplesAsync(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken) => await dbConnection.GetTuplesAsync(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken) => await dbConnection.GetTuplesAsync(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken) => await dbConnection.GetTuplesAsync(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken) => await dbConnection.GetTuplesAsync(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken) => await dbConnection.GetTuplesAsync(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        #endregion
        #region CheckDbConnection/CheckDbConnectionAsync
        public static bool CheckDbConnection(
            this IDbConnectionFactory factory)
            => factory.Using(
                tryFunc: TryFuncForCheckDbConnection,
                catchFunc: CatchFuncForCheckDbConnection);
        public static async Task<bool> CheckDbConnectionAsync(
            this IDbConnectionFactory factory,
            CancellationToken cancellationToken = default)
            => await factory.UsingAsync(
                tryFunc: TryFuncForCheckDbConnectionAsync,
                catchFunc: CatchFuncForCheckDbConnectionAsync,
                cancellationToken: cancellationToken);
        #endregion
        #region HasRows/HasRowsAsync
        public static bool HasRows(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, bool>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => factory.InternalUsing(
                tryFunc: dbConnection => dbConnection.HasRows(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions));
        public static async Task<bool> HasRowsAsync(
            this IDbConnectionFactory factory,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, bool>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await factory.InternalUsingAsync(
                tryFunc: async (dbConnection, cancellationToken) => await dbConnection.HasRowsAsync(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    isolationLevel: isolationLevel,
                    usingOptions: usingOptions,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        #endregion
    }
}
