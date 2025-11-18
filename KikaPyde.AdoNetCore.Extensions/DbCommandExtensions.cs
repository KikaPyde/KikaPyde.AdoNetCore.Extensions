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
            this DbCommand dbCommand,
            Func<DbCommand, DbDataReader, T> tryFunc,
            Func<DbCommand, DbDataReader?, Exception, T>? catchFunc = null,
            CommandBehavior? commandBehavior = null)
        {
            DbDataReader? dbDataReader = null;
            return TryCatchFinally(
                tryFunc: () => tryFunc(
                    dbCommand,
                    dbDataReader = commandBehavior.HasValue
                        ? dbCommand.ExecuteReader(commandBehavior.Value)
                        : dbCommand.ExecuteReader()),
                catchFunc: catchFunc is null ? null : tryException => catchFunc(dbCommand, dbDataReader, tryException),
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
            this DbCommand dbCommand,
            Func<DbCommand, DbDataReader, CancellationToken, Task<T>> tryFunc,
            Func<DbCommand, DbDataReader?, Exception, CancellationToken, Task<T>>? catchFunc = null,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
        {
            DbDataReader? dbDataReader = null;
            return await TryCatchFinallyAsync(
                tryFunc: async (cancellationToken) => await tryFunc(
                    dbCommand,
                    dbDataReader = commandBehavior.HasValue
                        ? await dbCommand.ExecuteReaderAsync(commandBehavior.Value, cancellationToken)
                        : await dbCommand.ExecuteReaderAsync(cancellationToken),
                    cancellationToken),
                catchFunc: catchFunc is null ? null : async (tryException, cancellationToken) => await catchFunc(dbCommand, dbDataReader, tryException, cancellationToken),
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
            this DbCommand dbCommand,
            Action<DbCommand> beforeExecute,
            Action<DbCommand>? afterExecute = null)
        {
            beforeExecute.Invoke(dbCommand);
            var result = dbCommand.ExecuteNonQuery();
            afterExecute?.Invoke(dbCommand);
            return result;
        }
        public static T ExecuteReader<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, T>? constructor = null,
            CommandBehavior? commandBehavior = null)
            => dbCommand.Using(
                tryFunc: (dbCommand, dbDataReader) =>
                {
                    var result = constructor is null
                        ? dbDataReader.Read()
                            ? dbDataReader.TakeFirstFieldValueOrThrowIfDbNullOrOutOfRange<T>().Item2
                            : throw new DataException("No data")
                        : constructor.Invoke(dbDataReader);
                    return result;
                },
                commandBehavior: commandBehavior);
        private static T ExecuteReader<T>(
            this DbCommand dbCommand,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, T>? constructor = null,
            Action<DbCommand, T>? afterExecute = null,
            CommandBehavior? commandBehavior = null)
        {
            beforeExecute.Invoke(dbCommand);
            var result = dbCommand.ExecuteReader(
                constructor: constructor,
                commandBehavior: commandBehavior);
            afterExecute?.Invoke(dbCommand, result);
            return result;
        }
        private static object? ExecuteScalar(
            this DbCommand dbCommand,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, object?>? afterExecute = null)
        {
            beforeExecute.Invoke(dbCommand);
            var result = dbCommand.ExecuteScalar();
            afterExecute?.Invoke(dbCommand, result);
            return result;
        }
        private static T? ExecuteScalar<T>(
            this DbCommand dbCommand,
            Action<DbCommand> beforeExecute,
            Func<object?, T?>? constructor = null,
            Action<DbCommand, T?>? afterExecute = null)
        {
            beforeExecute.Invoke(dbCommand);
            var internalResult = dbCommand.ExecuteScalar();
            var result = constructor is null ? (T?)internalResult : constructor(internalResult);
            afterExecute?.Invoke(dbCommand, result);
            return result;
        }
        #endregion
        #region ExecuteAsync
        private static async Task<int> ExecuteNonQueryAsync(
            this DbCommand dbCommand,
            Action<DbCommand> beforeExecute,
            Action<DbCommand>? afterExecute = null,
            CancellationToken cancellationToken = default)
        {
            beforeExecute.Invoke(dbCommand);
            var result = await dbCommand.ExecuteNonQueryAsync(cancellationToken);
            afterExecute?.Invoke(dbCommand);
            return result;
        }
        public static async Task<T> ExecuteReaderAsync<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, CancellationToken, Task<T>>? constructor = null,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.UsingAsync(
                tryFunc: async (dbCommand, dbDataReader, cancellationToken) =>
                {
                    if (await dbDataReader.ReadAsync(cancellationToken))
                    {
                        var result = await dbDataReader.TakeFirstFieldValueOrThrowIfDbNullOrOutOfRangeAsync<T>(cancellationToken);
                        return result.Item2;
                    }
                    else
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        throw new DataException("No data");
                    }
                },
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        private static async Task<T> ExecuteReaderAsync<T>(
            this DbCommand dbCommand,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<T>>? constructor = null,
            Action<DbCommand, T>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
        {
            beforeExecute.Invoke(dbCommand);
            var result = await dbCommand.ExecuteReaderAsync(
                constructor: constructor,
                commandBehavior: commandBehavior);
            afterExecute?.Invoke(dbCommand, result);
            return result;
        }
        private static async Task<object?> ExecuteScalarAsync(
            this DbCommand dbCommand,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, object?>? afterExecute = null,
            CancellationToken cancellationToken = default)
        {
            beforeExecute.Invoke(dbCommand);
            var result = await dbCommand.ExecuteScalarAsync(cancellationToken);
            afterExecute?.Invoke(dbCommand, result);
            return result;
        }
        private static async Task<T?> ExecuteScalarAsync<T>(
            this DbCommand dbCommand,
            Action<DbCommand> beforeExecute,
            Func<object?, T?>? constructor = null,
            Action<DbCommand, T?>? afterExecute = null,
            CancellationToken cancellationToken = default)
        {
            beforeExecute.Invoke(dbCommand);
            var internalResult = await dbCommand.ExecuteScalarAsync(cancellationToken);
            var result = constructor is null ? (T?)internalResult : constructor(internalResult);
            afterExecute?.Invoke(dbCommand, result);
            return result;
        }
        #endregion
        #region GetEnumerable
        #region Range
        public static TCollection GetCollection<TCollection, T>(
            this DbCommand dbCommand,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null)
            where TCollection : ICollection<T>, new()
            => dbCommand.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetCollection<TCollection, T>(
                    constructorByIndex: constructorByIndex),
                commandBehavior: commandBehavior);
        public static TCollection GetCollection<TCollection, T>(
            this DbCommand dbCommand,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null)
            where TCollection : ICollection<T>, new()
            => dbCommand.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetCollection<TCollection, T>(
                    constructor: constructor),
                commandBehavior: commandBehavior);
        public static List<T> GetList<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetList(
                    constructorByIndex: constructorByIndex),
                commandBehavior: commandBehavior);
        public static List<T> GetList<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetList(
                    constructor: constructor),
                commandBehavior: commandBehavior);
        public static Dictionary<int, T> GetDictionary<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetDictionary(
                    constructorByIndex: constructorByIndex),
                commandBehavior: commandBehavior);
        public static Dictionary<int, T> GetDictionary<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetDictionary(
                    constructor: constructor),
                commandBehavior: commandBehavior);
        public static DataTable? GetSchemaTable(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetSchemaTable(),
                commandBehavior: commandBehavior);
        public static DataTable GetDataTable(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: (Func<DbDataReader, DataTable>)GetDataTable,
                commandBehavior: commandBehavior);
        public static Dictionary<int, Dictionary<string, object?>> GetRawTable(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: (Func<DbDataReader, Dictionary<int, Dictionary<string, object?>>>)GetRawTable,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?>> GetTuples<T1, T2>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: (Func<DbDataReader, List<Tuple<T1?, T2?>>>)GetTuples<T1, T2>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?>> GetTuples<T1, T2, T3>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: (Func<DbDataReader, List<Tuple<T1?, T2?, T3?>>>)GetTuples<T1, T2, T3>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?>> GetTuples<T1, T2, T3, T4>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: (Func<DbDataReader, List<Tuple<T1?, T2?, T3?, T4?>>>)GetTuples<T1, T2, T3, T4>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?>> GetTuples<T1, T2, T3, T4, T5>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: (Func<DbDataReader, List<Tuple<T1?, T2?, T3?, T4?, T5?>>>)GetTuples<T1, T2, T3, T4, T5>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>> GetTuples<T1, T2, T3, T4, T5, T6>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: (Func<DbDataReader, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>>>)GetTuples<T1, T2, T3, T4, T5, T6>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>> GetTuples<T1, T2, T3, T4, T5, T6, T7>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: (Func<DbDataReader, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>>)GetTuples<T1, T2, T3, T4, T5, T6, T7>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: (Func<DbDataReader, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>>>)GetTuples<T1, T2, T3, T4, T5, T6, T7, T8>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: (Func<DbDataReader, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>>>)GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: (Func<DbDataReader, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>>>)GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>,
                commandBehavior: commandBehavior);
        #endregion
        #region ResultRange
        public static TCollection GetResultCollection<TCollection, T>(
            this DbCommand dbCommand,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null)
            where TCollection : ICollection<T>, new()
            => dbCommand.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetResultCollection<TCollection, T>(
                    constructorByIndex: constructorByIndex),
                commandBehavior: commandBehavior);
        public static TCollection GetResultCollection<TCollection, T>(
            this DbCommand dbCommand,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null)
            where TCollection : ICollection<T>, new()
            => dbCommand.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetResultCollection<TCollection, T>(
                    constructor: constructor),
                commandBehavior: commandBehavior);
        public static TCollection GetResultCollection<TCollection, T>(
            this DbCommand dbCommand,
            Func<DbDataReader, T> func,
            CommandBehavior? commandBehavior = null)
            where TCollection : ICollection<T>, new()
            => dbCommand.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetResultCollection<TCollection, T>(
                    func: func),
                commandBehavior: commandBehavior);
        public static List<T> GetResultList<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetResultList<T>(
                    constructorByIndex: constructorByIndex),
                commandBehavior: commandBehavior);
        public static List<T> GetResultList<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetResultList<T>(
                    constructor: constructor),
                commandBehavior: commandBehavior);
        public static List<T> GetResultList<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, T> func,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetResultList<T>(
                    func: func),
                commandBehavior: commandBehavior);
        public static Dictionary<int, T> GetResultDictionary<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetResultDictionary<T>(
                    constructorByIndex: constructorByIndex),
                commandBehavior: commandBehavior);
        public static Dictionary<int, T> GetResultDictionary<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetResultDictionary<T>(
                    constructor: constructor),
                commandBehavior: commandBehavior);
        public static Dictionary<int, T> GetResultDictionary<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, T> func,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetResultDictionary<T>(
                    func: func),
                commandBehavior: commandBehavior);
        public static Dictionary<int, Dictionary<int, Dictionary<string, object?>>> GetRawDatabase(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: (Func<DbDataReader, Dictionary<int, Dictionary<int, Dictionary<string, object?>>>>)GetRawDatabase,
                commandBehavior: commandBehavior);
        #endregion
        #region GlobalRange
        public static TCollection GetGlobalCollection<TCollection, T>(
            this DbCommand dbCommand,
            Func<DbDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null)
            where TCollection : ICollection<T>, new()
            => dbCommand.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetGlobalCollection<TCollection, T>(
                    constructorByIndex: constructorByIndex),
                commandBehavior: commandBehavior);
        public static TCollection GetGlobalCollection<TCollection, T>(
            this DbCommand dbCommand,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null)
            where TCollection : ICollection<T>, new()
            => dbCommand.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetGlobalCollection<TCollection, T>(
                    constructor: constructor),
                commandBehavior: commandBehavior);
        public static List<T> GetGlobalList<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetGlobalList<T>(
                    constructorByIndex: constructorByIndex),
                commandBehavior: commandBehavior);
        public static List<T> GetGlobalList<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetGlobalList<T>(
                    constructor: constructor),
                commandBehavior: commandBehavior);
        public static Dictionary<int, T> GetGlobalDictionary<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetGlobalDictionary<T>(
                    constructorByIndex: constructorByIndex),
                commandBehavior: commandBehavior);
        public static Dictionary<int, T> GetGlobalDictionary<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dbDataReader => dbDataReader.GetGlobalDictionary<T>(
                    constructor: constructor),
                commandBehavior: commandBehavior);
        public static Dictionary<int, Dictionary<string, object?>> GetGlobalRawTable(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: (Func<DbDataReader, Dictionary<int, Dictionary<string, object?>>>)GetGlobalRawTable,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?>> GetGlobalTuples<T1, T2>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: (Func<DbDataReader, List<Tuple<T1?, T2?>>>)GetGlobalTuples<T1, T2>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?>> GetGlobalTuples<T1, T2, T3>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: (Func<DbDataReader, List<Tuple<T1?, T2?, T3?>>>)GetGlobalTuples<T1, T2, T3>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?>> GetGlobalTuples<T1, T2, T3, T4>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: (Func<DbDataReader, List<Tuple<T1?, T2?, T3?, T4?>>>)GetGlobalTuples<T1, T2, T3, T4>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?>> GetGlobalTuples<T1, T2, T3, T4, T5>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: (Func<DbDataReader, List<Tuple<T1?, T2?, T3?, T4?, T5?>>>)GetGlobalTuples<T1, T2, T3, T4, T5>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>> GetGlobalTuples<T1, T2, T3, T4, T5, T6>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: (Func<DbDataReader, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>>>)GetGlobalTuples<T1, T2, T3, T4, T5, T6>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>> GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: (Func<DbDataReader, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>>)GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>> GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7, T8>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: (Func<DbDataReader, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>>>)GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7, T8>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>> GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: (Func<DbDataReader, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>>>)GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>> GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: (Func<DbDataReader, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>>>)GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>,
                commandBehavior: commandBehavior);
        #endregion
        #endregion
        #region GetEnumerableAsync
        #region RangeAsync
        public static async Task<TCollection> GetCollectionAsync<TCollection, T>(
            this DbCommand dbCommand,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbCommand.ExecuteReaderAsync(
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetCollectionAsync<TCollection, T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetCollectionAsync<TCollection, T>(
            this DbCommand dbCommand,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbCommand.ExecuteReaderAsync(
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetCollectionAsync<TCollection, T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetCollectionAsync<TCollection, T>(
            this DbCommand dbCommand,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbCommand.ExecuteReaderAsync(
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetCollectionAsync<TCollection, T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetCollectionAsync<TCollection, T>(
            this DbCommand dbCommand,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbCommand.ExecuteReaderAsync(
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetCollectionAsync<TCollection, T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetListAsync<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.GetCollectionAsync<List<T>, T>(
                constructorByIndex: constructorByIndex,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetListAsync<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.GetCollectionAsync<List<T>, T>(
                constructor: constructor,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetListAsync<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.GetCollectionAsync<List<T>, T>(
                constructorByIndex: constructorByIndex,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetListAsync<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.GetCollectionAsync<List<T>, T>(
                constructor: constructor,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetDictionaryAsync<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetDictionaryAsync(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetDictionaryAsync<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetDictionaryAsync(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetDictionaryAsync<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetDictionaryAsync(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetDictionaryAsync<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetDictionaryAsync(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<DataTable?> GetSchemaTableAsync(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetSchemaTableAsync(
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);

        public static async Task<DataTable> GetDataTableAsync(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: GetDataTableAsync,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, Dictionary<string, object?>>> GetRawTableAsync(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: GetRawTableAsync,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?>>> GetTuplesAsync<T1, T2>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: GetTuplesAsync<T1, T2>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?>>> GetTuplesAsync<T1, T2, T3>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: GetTuplesAsync<T1, T2, T3>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?>>> GetTuplesAsync<T1, T2, T3, T4>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: GetTuplesAsync<T1, T2, T3, T4>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?>>> GetTuplesAsync<T1, T2, T3, T4, T5>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: GetTuplesAsync<T1, T2, T3, T4, T5>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: GetTuplesAsync<T1, T2, T3, T4, T5, T6>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        #endregion
        #region ResultRangeAsync
        public static async Task<TCollection> GetResultCollectionAsync<TCollection, T>(
            this DbCommand dbCommand,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbCommand.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultCollectionAsync<TCollection, T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetResultCollectionAsync<TCollection, T>(
            this DbCommand dbCommand,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbCommand.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultCollectionAsync<TCollection, T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetResultCollectionAsync<TCollection, T>(
            this DbCommand dbCommand,
            Func<DbDataReader, CancellationToken, Task<T>> func,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbCommand.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultCollectionAsync<TCollection, T>(
                    func: func,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetResultListAsync<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultListAsync<T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetResultListAsync<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultListAsync<T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetResultListAsync<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, CancellationToken, Task<T>> func,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultListAsync<T>(
                    func: func,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetResultDictionaryAsync<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultDictionaryAsync<T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetResultDictionaryAsync<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultDictionaryAsync<T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetResultDictionaryAsync<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, CancellationToken, Task<T>> func,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultDictionaryAsync<T>(
                    func: func,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, Dictionary<int, Dictionary<string, object?>>>> GetRawDatabaseAsync(
            this DbCommand dbCommand,
            Action<DbCommand, Dictionary<int, Dictionary<int, Dictionary<string, object?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: GetRawDatabaseAsync,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        #endregion
        #region GlobalRangeAsync
        public static async Task<TCollection> GetGlobalCollectionAsync<TCollection, T>(
            this DbCommand dbCommand,
            Func<DbDataReader, int, int, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbCommand.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalCollectionAsync<TCollection, T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetGlobalCollectionAsync<TCollection, T>(
            this DbCommand dbCommand,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbCommand.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalCollectionAsync<TCollection, T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetGlobalCollectionAsync<TCollection, T>(
            this DbCommand dbCommand,
            Func<DbDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbCommand.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalCollectionAsync<TCollection, T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetGlobalCollectionAsync<TCollection, T>(
            this DbCommand dbCommand,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbCommand.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalCollectionAsync<TCollection, T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetGlobalListAsync<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, int, int, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalListAsync<T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetGlobalListAsync<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalListAsync<T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetGlobalListAsync<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalListAsync<T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetGlobalListAsync<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalListAsync<T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetGlobalDictionaryAsync<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, int, int, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalDictionaryAsync<T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetGlobalDictionaryAsync<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalDictionaryAsync<T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetGlobalDictionaryAsync<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalDictionaryAsync<T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetGlobalDictionaryAsync<T>(
            this DbCommand dbCommand,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalDictionaryAsync<T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, Dictionary<string, object?>>> GetGlobalRawTableAsync(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: GetGlobalRawTableAsync,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?>>> GetGlobalTuplesAsync<T1, T2>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: GetGlobalTuplesAsync<T1, T2>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?>>> GetGlobalTuplesAsync<T1, T2, T3>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: GetGlobalTuplesAsync<T1, T2, T3>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?>>> GetGlobalTuplesAsync<T1, T2, T3, T4>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: GetGlobalTuplesAsync<T1, T2, T3, T4>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?>>> GetGlobalTuplesAsync<T1, T2, T3, T4, T5>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: GetGlobalTuplesAsync<T1, T2, T3, T4, T5>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>>> GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>> GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>>> GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>>> GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>>> GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>,
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        #endregion
        #endregion
        #region HasRows/HasRowsAsync
        public static bool HasRows(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dbDataReader => dbDataReader.HasRows,
                commandBehavior: commandBehavior);
        public static async Task<bool> HasRowsAsync(
            this DbCommand dbCommand,
            CommandBehavior? commandBehavior = null,
            CancellationToken cancellationToken = default)
            => await dbCommand.ExecuteReaderAsync(
                constructor: (dbDataReader, cancellationToken) => Task.FromResult(dbDataReader.HasRows),
                commandBehavior: commandBehavior,
                cancellationToken: cancellationToken);
        #endregion
    }
}
