using System.Data;
using System.Data.Common;

namespace KikaPyde.AdoNetCore.Extensions
{
    public static partial class AdoNetCoreHelper
    {
        public static T Using<T>(
            this DbBatch dbBatch,
            Func<DbBatch, DbDataReader, T> tryFunc,
            Func<DbBatch, DbDataReader?, Exception, T>? catchFunc = null,
            CommandBehavior? commandBehavior = null)
        {
            DbDataReader? dbDataReader = null;
            return TryCatchFinally(
                tryFunc: () => tryFunc(
                    dbBatch,
                    dbDataReader = commandBehavior.HasValue
                        ? dbBatch.ExecuteReader(commandBehavior.Value)
                        : dbBatch.ExecuteReader()),
                catchFunc: catchFunc is null ? null : tryException => catchFunc(dbBatch, dbDataReader, tryException),
                finallyAction: () =>
                {
                    if (dbDataReader is not null)
                    {
                        dbDataReader.Close();
                        dbDataReader.Dispose();
                    }
                });
        }
        public static async Task<T> UsingAsync<T>(
            this DbBatch DbBatch,
            Func<DbBatch, DbDataReader, CancellationToken, Task<T>> tryFunc,
            Func<DbBatch, DbDataReader?, Exception, CancellationToken, Task<T>>? catchFunc = null,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
        {
            DbDataReader? dbDataReader = null;
            return await TryCatchFinallyAsync(
                tryFunc: async (cancellationToken) => await tryFunc(
                    DbBatch,
                    dbDataReader = commandBehavior.HasValue
                        ? await DbBatch.ExecuteReaderAsync(commandBehavior.Value, cancellationToken)
                        : await DbBatch.ExecuteReaderAsync(cancellationToken),
                    cancellationToken),
                catchFunc: catchFunc is null ? null : async (tryException, cancellationToken) => await catchFunc(DbBatch, dbDataReader, tryException, cancellationToken),
                finallyFunc: async () =>
                {
                    if (dbDataReader is not null)
                    {
                        await dbDataReader.CloseAsync();
                        await dbDataReader.DisposeAsync();
                    }
                },
                cancellationToken: cancellationToken);
        }
        #region Execute
        private static int ExecuteNonQuery(
            this DbBatch dbBatch,
            Action<DbBatch> beforeExecute,
            Action<DbBatch>? afterExecute = null)
        {
            beforeExecute.Invoke(dbBatch);
            var result = dbBatch.ExecuteNonQuery();
            afterExecute?.Invoke(dbBatch);
            return result;
        }
        public static T ExecuteReader<T>(
            this DbBatch dbBatch,
            Func<DbDataReader, T>? constructor = null,
            CommandBehavior? commandBehavior = null)
            => dbBatch.Using(
                tryFunc: (dbBatch, dbDataReader) =>
                {
                    var result = constructor is null
                        ? dbDataReader.Read() && !dbDataReader.IsDBNull(0)
                            ? (T)dbDataReader.GetValue(0)
                            : throw new InvalidOperationException()
                        : constructor.Invoke(dbDataReader);
                    return result;
                },
                commandBehavior: commandBehavior);
        private static T ExecuteReader<T>(
            this DbBatch dbBatch,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, T>? constructor = null,
            Action<DbBatch, T>? afterExecute = null,
            CommandBehavior? commandBehavior = null)
        {
            beforeExecute.Invoke(dbBatch);
            var result = dbBatch.ExecuteReader<T>(
                constructor: constructor,
                commandBehavior: commandBehavior);
            afterExecute?.Invoke(dbBatch, result);
            return result;
        }
        private static object? ExecuteScalar(
            this DbBatch dbBatch,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, object?>? afterExecute = null)
        {
            beforeExecute.Invoke(dbBatch);
            var result = dbBatch.ExecuteScalar();
            afterExecute?.Invoke(dbBatch, result);
            return result;
        }
        private static T? ExecuteScalar<T>(
            this DbBatch dbBatch,
            Action<DbBatch> beforeExecute,
            Func<object?, T?>? constructor = null,
            Action<DbBatch, T?>? afterExecute = null)
        {
            beforeExecute.Invoke(dbBatch);
            var internalResult = dbBatch.ExecuteScalar();
            var result = constructor is null ? (T?)internalResult : constructor(internalResult);
            afterExecute?.Invoke(dbBatch, result);
            return result;
        }
        #endregion
        #region ExecuteAsync
        private static async Task<int> ExecuteNonQueryAsync(
            this DbBatch dbBatch,
            Action<DbBatch> beforeExecute,
            Action<DbBatch>? afterExecute = null,
            CancellationToken cancellationToken = default)
        {
            beforeExecute.Invoke(dbBatch);
            var result = await dbBatch.ExecuteNonQueryAsync(cancellationToken);
            afterExecute?.Invoke(dbBatch);
            return result;
        }
        public static async Task<T> ExecuteReaderAsync<T>(
            this DbBatch dbBatch,
            Func<DbDataReader, CancellationToken, Task<T>>? constructor = null,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.UsingAsync(
                tryFunc: async (dbBatch, dbDataReader, cancellationToken) =>
                {
                    var result = constructor is null
                        ? await dbDataReader.ReadAsync(cancellationToken) && !await dbDataReader.IsDBNullAsync(0, cancellationToken)
                            ? (T)dbDataReader.GetValue(0)
                            : throw (cancellationToken.IsCancellationRequested
                                ? new OperationCanceledException()
                                : new InvalidOperationException())
                        : await constructor.Invoke(dbDataReader, cancellationToken);
                    return result;
                },
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        private static async Task<T> ExecuteReaderAsync<T>(
            this DbBatch dbBatch,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<T>>? constructor = null,
            Action<DbBatch, T>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
        {
            beforeExecute.Invoke(dbBatch);
            var result = await dbBatch.ExecuteReaderAsync<T>(
                constructor: constructor,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
            afterExecute?.Invoke(dbBatch, result);
            return result;
        }
        private static async Task<object?> ExecuteScalarAsync(
            this DbBatch dbBatch,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, object?>? afterExecute = null,
            CancellationToken cancellationToken = default)
        {
            beforeExecute.Invoke(dbBatch);
            var result = await dbBatch.ExecuteScalarAsync(cancellationToken);
            afterExecute?.Invoke(dbBatch, result);
            return result;
        }
        private static async Task<T?> ExecuteScalarAsync<T>(
            this DbBatch dbBatch,
            Action<DbBatch> beforeExecute,
            Func<object?, T?>? constructor = null,
            Action<DbBatch, T?>? afterExecute = null,
            CancellationToken cancellationToken = default)
        {
            beforeExecute.Invoke(dbBatch);
            var internalResult = await dbBatch.ExecuteScalarAsync(cancellationToken);
            var result = constructor is null ? (T?)internalResult : constructor(internalResult);
            afterExecute?.Invoke(dbBatch, result);
            return result;
        }
        #endregion
        #region GetEnumerable
        public static TCollection GetCollection<TCollection, T>(
            this DbBatch dbBatch,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null)
            where TCollection : ICollection<T>, new()
            => dbBatch.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetCollection<TCollection, T>(
                    constructorByIndex: constructorByIndex),
                commandBehavior: commandBehavior);
        public static TCollection GetCollection<TCollection, T>(
            this DbBatch dbBatch,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null)
            where TCollection : ICollection<T>, new()
            => dbBatch.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetCollection<TCollection, T>(
                    constructor: constructor),
                commandBehavior: commandBehavior);
        public static List<T> GetList<T>(
            this DbBatch dbBatch,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null)
            => dbBatch.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetList(
                    constructorByIndex: constructorByIndex),
                commandBehavior: commandBehavior);
        public static List<T> GetList<T>(
            this DbBatch dbBatch,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null)
            => dbBatch.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetList(
                    constructor: constructor),
                commandBehavior: commandBehavior);
        public static Dictionary<int, T> GetDictionary<T>(
            this DbBatch dbBatch,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null)
            => dbBatch.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetDictionary(
                    constructorByIndex: constructorByIndex),
                commandBehavior: commandBehavior);
        public static Dictionary<int, T> GetDictionary<T>(
            this DbBatch dbBatch,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null)
            => dbBatch.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetDictionary(
                    constructor: constructor),
                commandBehavior: commandBehavior);
        public static DataTable? GetSchemaTable(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior)
            => dbBatch.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetSchemaTable(),
                commandBehavior: commandBehavior);
        public static DataTable GetDataTable(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior)
            => dbBatch.ExecuteReader(
                constructor: GetDataTable,
                commandBehavior: commandBehavior);
        public static Dictionary<int, Dictionary<string, object?>> GetRawTable(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null)
            => dbBatch.ExecuteReader(
                constructor: GetRawTable,
                commandBehavior: commandBehavior);
        #endregion
        #region GetEnumerableAsync
        public static async Task<TCollection> GetCollectionAsync<TCollection, T>(
            this DbBatch dbBatch,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbBatch.ExecuteReaderAsync(
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetCollectionAsync<TCollection, T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetCollectionAsync<TCollection, T>(
            this DbBatch dbBatch,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbBatch.ExecuteReaderAsync(
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetCollectionAsync<TCollection, T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetCollectionAsync<TCollection, T>(
            this DbBatch dbBatch,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbBatch.ExecuteReaderAsync(
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetCollectionAsync<TCollection, T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetCollectionAsync<TCollection, T>(
            this DbBatch dbBatch,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbBatch.ExecuteReaderAsync(
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetCollectionAsync<TCollection, T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetListAsync<T>(
            this DbBatch dbBatch,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.GetCollectionAsync<List<T>, T>(
                constructorByIndex: constructorByIndex,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetListAsync<T>(
            this DbBatch dbBatch,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.GetCollectionAsync<List<T>, T>(
                constructor: constructor,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetListAsync<T>(
            this DbBatch dbBatch,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.GetCollectionAsync<List<T>, T>(
                constructorByIndex: constructorByIndex,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetListAsync<T>(
            this DbBatch dbBatch,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.GetCollectionAsync<List<T>, T>(
                constructor: constructor,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetDictionaryAsync<T>(
            this DbBatch dbBatch,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetDictionaryAsync(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetDictionaryAsync<T>(
            this DbBatch dbBatch,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetDictionaryAsync(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetDictionaryAsync<T>(
            this DbBatch dbBatch,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetDictionaryAsync(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetDictionaryAsync<T>(
            this DbBatch dbBatch,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetDictionaryAsync(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<DataTable?> GetSchemaTableAsync(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetSchemaTableAsync(
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<DataTable> GetDataTableAsync(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: GetDataTableAsync,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, Dictionary<string, object?>>> GetRawTableAsync(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: GetRawTableAsync,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        #endregion
        #region HasRows/HasRowsAsync
        public static bool HasRows(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null)
            => dbBatch.ExecuteReader(
                constructor: dbDataReader => dbDataReader.HasRows,
                commandBehavior: commandBehavior);
        public static async Task<bool> HasRowsAsync(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => Task.FromResult(dbDataReader.HasRows),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        #endregion
    }
}
