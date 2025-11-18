using System;
using System.Collections.Generic;
using System.Data;

namespace KikaPyde.AdoNetCore.Extensions
{
    public static partial class AdoNetCoreHelper
    {
        public static T Using<T>(
            this IDbCommand dbCommand,
            Func<IDbCommand, IDataReader, T> tryFunc,
            Func<IDbCommand, IDataReader?, Exception, T>? catchFunc = null,
            CommandBehavior? commandBehavior = null)
        {
            IDataReader? dataReader = null;
            return TryCatchFinally(
                tryFunc: () => tryFunc(
                    dbCommand,
                    dataReader = commandBehavior.HasValue
                        ? dbCommand.ExecuteReader(commandBehavior.Value)
                        : dbCommand.ExecuteReader()),
                catchFunc: catchFunc is null ? null : tryException => catchFunc(dbCommand, dataReader, tryException),
                finallyAction: () =>
                {
                    if (dataReader is not null)
                    {
                        dataReader.Close();
                        dataReader.Dispose();
                    }
                });
        }
        #region Execute
        private static int ExecuteNonQuery(
            this IDbCommand dbCommand,
            Action<IDbCommand> beforeExecute,
            Action<IDbCommand>? afterExecute = null)
        {
            beforeExecute.Invoke(dbCommand);
            var result = dbCommand.ExecuteNonQuery();
            afterExecute?.Invoke(dbCommand);
            return result;
        }
        public static T ExecuteReader<T>(
            this IDbCommand dbCommand,
            Func<IDataReader, T>? constructor = null,
            CommandBehavior? commandBehavior = null)
            => dbCommand.Using(
                tryFunc: (dbCommand, dataReader) =>
                {
                    var result = constructor is null
                        ? dataReader.Read()
                            ? dataReader.TakeFirstFieldValueOrThrowIfDbNullOrOutOfRange<T>().Item2
                            : throw new DataException("No data")
                        : constructor.Invoke(dataReader);
                    return result;
                },
                commandBehavior: commandBehavior);
        private static T ExecuteReader<T>(
            this IDbCommand dbCommand,
            Action<IDbCommand> beforeExecute,
            Func<IDataReader, T>? constructor = null,
            Action<IDbCommand, T>? afterExecute = null,
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
            this IDbCommand dbCommand,
            Action<IDbCommand> beforeExecute,
            Action<IDbCommand, object?>? afterExecute = null)
        {
            beforeExecute.Invoke(dbCommand);
            var result = dbCommand.ExecuteScalar();
            afterExecute?.Invoke(dbCommand, result);
            return result;
        }
        private static T? ExecuteScalar<T>(
            this IDbCommand dbCommand,
            Action<IDbCommand> beforeExecute,
            Func<object?, T?>? constructor = null,
            Action<IDbCommand, T?>? afterExecute = null)
        {
            beforeExecute.Invoke(dbCommand);
            var internalResult = dbCommand.ExecuteScalar();
            var result = constructor is null ? (T?)internalResult : constructor(internalResult);
            afterExecute?.Invoke(dbCommand, result);
            return result;
        }
        #endregion
        #region GetEnumerable
        #region Range
        public static TCollection GetCollection<TCollection, T>(
            this IDbCommand dbCommand,
            Func<IDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null)
            where TCollection : ICollection<T>, new()
            => dbCommand.ExecuteReader(
                constructor: dataReader => dataReader.GetCollection<TCollection, T>(
                    constructorByIndex: constructorByIndex),
                commandBehavior: commandBehavior);
        public static TCollection GetCollection<TCollection, T>(
            this IDbCommand dbCommand,
            Func<IDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null)
            where TCollection : ICollection<T>, new()
            => dbCommand.ExecuteReader(
                constructor: dataReader => dataReader.GetCollection<TCollection, T>(
                    constructor: constructor),
                commandBehavior: commandBehavior);
        public static List<T> GetList<T>(
            this IDbCommand dbCommand,
            Func<IDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dataReader => dataReader.GetList(
                    constructorByIndex: constructorByIndex),
                commandBehavior: commandBehavior);
        public static List<T> GetList<T>(
            this IDbCommand dbCommand,
            Func<IDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dataReader => dataReader.GetList(
                    constructor: constructor),
                commandBehavior: commandBehavior);
        public static Dictionary<int, T> GetDictionary<T>(
            this IDbCommand dbCommand,
            Func<IDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dataReader => dataReader.GetDictionary(
                    constructorByIndex: constructorByIndex),
                commandBehavior: commandBehavior);
        public static Dictionary<int, T> GetDictionary<T>(
            this IDbCommand dbCommand,
            Func<IDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dataReader => dataReader.GetDictionary(
                    constructor: constructor),
                commandBehavior: commandBehavior);
        public static DataTable? GetSchemaTable(
            this IDbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dataReader => dataReader.GetSchemaTable(),
                commandBehavior: commandBehavior);
        public static DataTable GetDataTable(
            this IDbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: GetDataTable,
                commandBehavior: commandBehavior);
        public static Dictionary<int, Dictionary<string, object?>> GetRawTable(
            this IDbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: GetRawTable,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?>> GetTuples<T1, T2>(
            this IDbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: GetTuples<T1, T2>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?>> GetTuples<T1, T2, T3>(
            this IDbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: GetTuples<T1, T2, T3>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?>> GetTuples<T1, T2, T3, T4>(
            this IDbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: GetTuples<T1, T2, T3, T4>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?>> GetTuples<T1, T2, T3, T4, T5>(
            this IDbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: GetTuples<T1, T2, T3, T4, T5>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>> GetTuples<T1, T2, T3, T4, T5, T6>(
            this IDbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: GetTuples<T1, T2, T3, T4, T5, T6>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>> GetTuples<T1, T2, T3, T4, T5, T6, T7>(
            this IDbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: GetTuples<T1, T2, T3, T4, T5, T6, T7>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8>(
            this IDbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: GetTuples<T1, T2, T3, T4, T5, T6, T7, T8>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this IDbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this IDbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>,
                commandBehavior: commandBehavior);
        #endregion
        #region ResultRange
        public static TCollection GetResultCollection<TCollection, T>(
            this IDbCommand dbCommand,
            Func<IDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null)
            where TCollection : ICollection<T>, new()
            => dbCommand.ExecuteReader(
                constructor: dataReader => dataReader.GetResultCollection<TCollection, T>(
                    constructorByIndex: constructorByIndex),
                commandBehavior: commandBehavior);
        public static TCollection GetResultCollection<TCollection, T>(
            this IDbCommand dbCommand,
            Func<IDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null)
            where TCollection : ICollection<T>, new()
            => dbCommand.ExecuteReader(
                constructor: dataReader => dataReader.GetResultCollection<TCollection, T>(
                    constructor: constructor),
                commandBehavior: commandBehavior);
        public static TCollection GetResultCollection<TCollection, T>(
            this IDbCommand dbCommand,
            Func<IDataReader, T> func,
            CommandBehavior? commandBehavior = null)
            where TCollection : ICollection<T>, new()
            => dbCommand.ExecuteReader(
                constructor: dataReader => dataReader.GetResultCollection<TCollection, T>(
                    func: func),
                commandBehavior: commandBehavior);
        public static List<T> GetResultList<T>(
            this IDbCommand dbCommand,
            Func<IDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dataReader => dataReader.GetResultList<T>(
                    constructorByIndex: constructorByIndex),
                commandBehavior: commandBehavior);
        public static List<T> GetResultList<T>(
            this IDbCommand dbCommand,
            Func<IDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dataReader => dataReader.GetResultList<T>(
                    constructor: constructor),
                commandBehavior: commandBehavior);
        public static List<T> GetResultList<T>(
            this IDbCommand dbCommand,
            Func<IDataReader, T> func,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dataReader => dataReader.GetResultList<T>(
                    func: func),
                commandBehavior: commandBehavior);
        public static Dictionary<int, T> GetResultDictionary<T>(
            this IDbCommand dbCommand,
            Func<IDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dataReader => dataReader.GetResultDictionary<T>(
                    constructorByIndex: constructorByIndex),
                commandBehavior: commandBehavior);
        public static Dictionary<int, T> GetResultDictionary<T>(
            this IDbCommand dbCommand,
            Func<IDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dataReader => dataReader.GetResultDictionary<T>(
                    constructor: constructor),
                commandBehavior: commandBehavior);
        public static Dictionary<int, T> GetResultDictionary<T>(
            this IDbCommand dbCommand,
            Func<IDataReader, T> func,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dataReader => dataReader.GetResultDictionary<T>(
                    func: func),
                commandBehavior: commandBehavior);
        public static Dictionary<int, Dictionary<int, Dictionary<string, object?>>> GetRawDatabase(
            this IDbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: GetRawDatabase,
                commandBehavior: commandBehavior);
        #endregion
        #region GlobalRange
        public static TCollection GetGlobalCollection<TCollection, T>(
            this IDbCommand dbCommand,
            Func<IDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null)
            where TCollection : ICollection<T>, new()
            => dbCommand.ExecuteReader(
                constructor: dataReader => dataReader.GetGlobalCollection<TCollection, T>(
                    constructorByIndex: constructorByIndex),
                commandBehavior: commandBehavior);
        public static TCollection GetGlobalCollection<TCollection, T>(
            this IDbCommand dbCommand,
            Func<IDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null)
            where TCollection : ICollection<T>, new()
            => dbCommand.ExecuteReader(
                constructor: dataReader => dataReader.GetGlobalCollection<TCollection, T>(
                    constructor: constructor),
                commandBehavior: commandBehavior);
        public static List<T> GetGlobalList<T>(
            this IDbCommand dbCommand,
            Func<IDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dataReader => dataReader.GetGlobalList<T>(
                    constructorByIndex: constructorByIndex),
                commandBehavior: commandBehavior);
        public static List<T> GetGlobalList<T>(
            this IDbCommand dbCommand,
            Func<IDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dataReader => dataReader.GetGlobalList<T>(
                    constructor: constructor),
                commandBehavior: commandBehavior);
        public static Dictionary<int, T> GetGlobalDictionary<T>(
            this IDbCommand dbCommand,
            Func<IDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex = null,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dataReader => dataReader.GetGlobalDictionary<T>(
                    constructorByIndex: constructorByIndex),
                commandBehavior: commandBehavior);
        public static Dictionary<int, T> GetGlobalDictionary<T>(
            this IDbCommand dbCommand,
            Func<IDataReader, ValueTuple<bool, T>>? constructor,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dataReader => dataReader.GetGlobalDictionary<T>(
                    constructor: constructor),
                commandBehavior: commandBehavior);
        public static Dictionary<int, Dictionary<string, object?>> GetGlobalRawTable(
            this IDbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: GetGlobalRawTable,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?>> GetGlobalTuples<T1, T2>(
            this IDbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: GetGlobalTuples<T1, T2>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?>> GetGlobalTuples<T1, T2, T3>(
            this IDbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: GetGlobalTuples<T1, T2, T3>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?>> GetGlobalTuples<T1, T2, T3, T4>(
            this IDbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: GetGlobalTuples<T1, T2, T3, T4>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?>> GetGlobalTuples<T1, T2, T3, T4, T5>(
            this IDbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: GetGlobalTuples<T1, T2, T3, T4, T5>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>> GetGlobalTuples<T1, T2, T3, T4, T5, T6>(
            this IDbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: GetGlobalTuples<T1, T2, T3, T4, T5, T6>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>> GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7>(
            this IDbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>> GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7, T8>(
            this IDbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7, T8>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>> GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this IDbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9>,
                commandBehavior: commandBehavior);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>> GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this IDbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>,
                commandBehavior: commandBehavior);
        #endregion
        #endregion
        public static bool HasRows(
            this IDbCommand dbCommand,
            CommandBehavior? commandBehavior = null)
            => dbCommand.ExecuteReader(
                constructor: dataReader => dataReader.Read(),
                commandBehavior: commandBehavior);
    }
}
