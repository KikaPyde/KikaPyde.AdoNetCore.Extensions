#if NET6_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace KikaPyde.AdoNetCore.Extensions
{
    public static partial class AdoNetCoreHelper
    {
        #region Using/UsingAsync
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
            this DbBatch dbBatch,
            Func<DbBatch, DbDataReader, CancellationToken, Task<T>> tryFunc,
            Func<DbBatch, DbDataReader?, Exception, CancellationToken, Task<T>>? catchFunc = null,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
        {
            DbDataReader? dbDataReader = null;
            return await TryCatchFinallyAsync(
                tryFunc: async (cancellationToken) => await tryFunc(
                    dbBatch,
                    dbDataReader = commandBehavior.HasValue
                        ? await dbBatch.ExecuteReaderAsync(commandBehavior.Value, cancellationToken)
                        : await dbBatch.ExecuteReaderAsync(cancellationToken),
                    cancellationToken),
                catchFunc: catchFunc is null ? null : async (tryException, cancellationToken) => await catchFunc(dbBatch, dbDataReader, tryException, cancellationToken),
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
        #endregion
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
            CommandBehavior? commandBehavior = null)
            => dbBatch.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetSchemaTable(),
                commandBehavior: commandBehavior);
        public static DataTable GetDataTable(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null)
            => dbBatch.ExecuteReader(
                constructor: GetDataTable,
                commandBehavior: commandBehavior);
        public static Dictionary<int, Dictionary<string, object?>> GetRawTable(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null)
            => dbBatch.ExecuteReader(
                constructor: GetRawTable,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?>> GetTuples<T1, T2>(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null)
            => dbBatch.ExecuteReader(
                constructor: GetTuples<T1, T2>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?>> GetTuples<T1, T2, T3>(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null)
            => dbBatch.ExecuteReader(
                constructor: GetTuples<T1, T2, T3>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?>> GetTuples<T1, T2, T3, T4>(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null)
            => dbBatch.ExecuteReader(
                constructor: GetTuples<T1, T2, T3, T4>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?>> GetTuples<T1, T2, T3, T4, T5>(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null)
            => dbBatch.ExecuteReader(
                constructor: GetTuples<T1, T2, T3, T4, T5>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>> GetTuples<T1, T2, T3, T4, T5, T6>(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null)
            => dbBatch.ExecuteReader(
                constructor: GetTuples<T1, T2, T3, T4, T5, T6>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>> GetTuples<T1, T2, T3, T4, T5, T6, T7>(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null)
            => dbBatch.ExecuteReader(
                constructor: GetTuples<T1, T2, T3, T4, T5, T6, T7>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8>(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null)
            => dbBatch.ExecuteReader(
                constructor: GetTuples<T1, T2, T3, T4, T5, T6, T7, T8>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null)
            => dbBatch.ExecuteReader(
                constructor: GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null)
            => dbBatch.ExecuteReader(
                constructor: GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>,
                commandBehavior: commandBehavior);
        #endregion
        #region GetEnumerableAsync
        #region RangeAsync
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
        public static async Task<List<Tuple<T1?, T2?>>> GetTuplesAsync<T1, T2>(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: GetTuplesAsync<T1, T2>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?>>> GetTuplesAsync<T1, T2, T3>(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: GetTuplesAsync<T1, T2, T3>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?>>> GetTuplesAsync<T1, T2, T3, T4>(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: GetTuplesAsync<T1, T2, T3, T4>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?>>> GetTuplesAsync<T1, T2, T3, T4, T5>(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: GetTuplesAsync<T1, T2, T3, T4, T5>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6>(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: GetTuplesAsync<T1, T2, T3, T4, T5, T6>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7>(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        #endregion
        #region ResultRangeAsync
        public static async Task<TCollection> GetResultCollectionAsync<TCollection, T>(
            this DbBatch dbBatch,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbBatch.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultCollectionAsync<TCollection, T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetResultCollectionAsync<TCollection, T>(
            this DbBatch dbBatch,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbBatch.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultCollectionAsync<TCollection, T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetResultCollectionAsync<TCollection, T>(
            this DbBatch dbBatch,
            Func<DbDataReader, CancellationToken, Task<T>> func,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbBatch.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultCollectionAsync<TCollection, T>(
                    func: func,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetResultListAsync<T>(
            this DbBatch dbBatch,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultListAsync<T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetResultListAsync<T>(
            this DbBatch dbBatch,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultListAsync<T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetResultListAsync<T>(
            this DbBatch dbBatch,
            Func<DbDataReader, CancellationToken, Task<T>> func,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultListAsync<T>(
                    func: func,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetResultDictionaryAsync<T>(
            this DbBatch dbBatch,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultDictionaryAsync<T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetResultDictionaryAsync<T>(
            this DbBatch dbBatch,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultDictionaryAsync<T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetResultDictionaryAsync<T>(
            this DbBatch dbBatch,
            Func<DbDataReader, CancellationToken, Task<T>> func,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultDictionaryAsync<T>(
                    func: func,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, Dictionary<int, Dictionary<string, object?>>>> GetRawDatabaseAsync(
            this DbBatch dbBatch,
            Action<DbBatch, Dictionary<int, Dictionary<int, Dictionary<string, object?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: GetRawDatabaseAsync,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        #endregion
        #region GlobalRangeAsync
        public static async Task<TCollection> GetGlobalCollectionAsync<TCollection, T>(
            this DbBatch dbBatch,
            Func<DbDataReader, int, int, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbBatch.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalCollectionAsync<TCollection, T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetGlobalCollectionAsync<TCollection, T>(
            this DbBatch dbBatch,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbBatch.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalCollectionAsync<TCollection, T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetGlobalCollectionAsync<TCollection, T>(
            this DbBatch dbBatch,
            Func<DbDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbBatch.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalCollectionAsync<TCollection, T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetGlobalCollectionAsync<TCollection, T>(
            this DbBatch dbBatch,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbBatch.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalCollectionAsync<TCollection, T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetGlobalListAsync<T>(
            this DbBatch dbBatch,
            Func<DbDataReader, int, int, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalListAsync<T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetGlobalListAsync<T>(
            this DbBatch dbBatch,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalListAsync<T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetGlobalListAsync<T>(
            this DbBatch dbBatch,
            Func<DbDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalListAsync<T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetGlobalListAsync<T>(
            this DbBatch dbBatch,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalListAsync<T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetGlobalDictionaryAsync<T>(
            this DbBatch dbBatch,
            Func<DbDataReader, int, int, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalDictionaryAsync<T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetGlobalDictionaryAsync<T>(
            this DbBatch dbBatch,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalDictionaryAsync<T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetGlobalDictionaryAsync<T>(
            this DbBatch dbBatch,
            Func<DbDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalDictionaryAsync<T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetGlobalDictionaryAsync<T>(
            this DbBatch dbBatch,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalDictionaryAsync<T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, Dictionary<string, object?>>> GetGlobalRawTableAsync(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: GetGlobalRawTableAsync,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?>>> GetGlobalTuplesAsync<T1, T2>(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: GetGlobalTuplesAsync<T1, T2>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?>>> GetGlobalTuplesAsync<T1, T2, T3>(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: GetGlobalTuplesAsync<T1, T2, T3>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?>>> GetGlobalTuplesAsync<T1, T2, T3, T4>(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: GetGlobalTuplesAsync<T1, T2, T3, T4>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?>>> GetGlobalTuplesAsync<T1, T2, T3, T4, T5>(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: GetGlobalTuplesAsync<T1, T2, T3, T4, T5>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>>> GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6>(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>> GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7>(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>>> GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>>> GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>>> GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this DbBatch dbBatch,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbBatch.ExecuteReaderAsync(
                constructor: GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        #endregion
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
#endif