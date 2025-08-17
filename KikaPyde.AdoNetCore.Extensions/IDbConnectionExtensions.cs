using System;
using System.Collections.Generic;
using System.Data;

namespace KikaPyde.AdoNetCore.Extensions
{
    public static partial class AdoNetCoreHelper
    {
        #region Using
        private static T InternalUsing<T>(
            this IDbConnection dbConnection,
            Delegate tryFunc,
            Delegate? catchFunc,
            bool allowDbTransaction = false,
            bool allowDbCommand = false,
            bool allowDataReader = false,
            IsolationLevel? isolationLevel = null,
            CommandBehavior? commandBehavior = null,
            IUsingOptions? usingOptions = null)
        {
            IDbTransaction? dbTransaction = null;
            IDbCommand? dbCommand = null;
            IDataReader? dataReader = null;
            ConnectionState? connectionState = null;
            usingOptions = usingOptions ?? DefaultUsingOptions;
            try
            {
                connectionState = dbConnection.State;
                if (connectionState is not ConnectionState.Open)
                    dbConnection.Open();
                if (allowDbTransaction)
                {
                    dbTransaction = isolationLevel.HasValue
                        ? dbConnection.BeginTransaction(isolationLevel.Value)
                        : dbConnection.BeginTransaction();
                }
                if (allowDbCommand)
                {
                    dbCommand = dbConnection.CreateCommand();
                    dbCommand.Transaction = dbTransaction;
                }
                if (allowDataReader)
                {
                    if (dbCommand is not null)
                    {
                        dataReader = commandBehavior.HasValue
                            ? dbCommand.ExecuteReader(commandBehavior.Value)
                            : dbCommand.ExecuteReader();
                    }
                }
                var result = tryFunc switch
                {
                    null => throw new ArgumentNullException(nameof(tryFunc)),
                    Func<IDbConnection, T> tryFunc2
                        => tryFunc2(dbConnection),
                    Func<IDbConnection, IDbTransaction, T> tryFunc3 when dbTransaction is not null
                        => tryFunc3(dbConnection, dbTransaction),
                    Func<IDbConnection, IDbCommand, T> tryFunc3 when dbCommand is not null
                        => tryFunc3(dbConnection, dbCommand),
                    Func<IDbConnection, IDbTransaction, IDbCommand, T> tryFunc4 when dbTransaction is not null && dbCommand is not null
                        => tryFunc4(dbConnection, dbTransaction, dbCommand),
                    Func<IDbConnection, IDbCommand, IDataReader, T> tryFunc4 when dbCommand is not null && dataReader is not null
                        => tryFunc4(dbConnection, dbCommand, dataReader),
                    Func<IDbConnection, IDbTransaction, IDbCommand, IDataReader, T> tryFunc5 when dbTransaction is not null && dbCommand is not null && dataReader is not null
                        => tryFunc5(dbConnection, dbTransaction, dbCommand, dataReader),
                    _ => throw new InvalidCastException(nameof(tryFunc)),
                };
                if (dbTransaction is not null && usingOptions.AllowTransactionCommitOnSuccessTryFunc)
                    dbTransaction.Commit();
                return result;
            }
            catch (Exception tryException)
            {
                try
                {
                    if (catchFunc is null)
                    {
                        if (dbTransaction is not null && usingOptions.AllowTransactionRollbackOnFailTryFunc)
                            dbTransaction.Rollback();
                    }
                    else
                    {
                        var result = catchFunc switch
                        {
                            null => throw new ArgumentNullException(nameof(catchFunc)),
                            Func<IDbConnection, Exception, T> catchFunc3
                                => catchFunc3(dbConnection, tryException),
                            Func<IDbConnection, IDbTransaction?, Exception, T> catchFunc4
                                => catchFunc4(dbConnection, dbTransaction, tryException),
                            Func<IDbConnection, IDbCommand?, Exception, T> catchFunc4
                                => catchFunc4(dbConnection, dbCommand, tryException),
                            Func<IDbConnection, IDbTransaction?, IDbCommand?, Exception, T> catchFunc5
                                => catchFunc5(dbConnection, dbTransaction, dbCommand, tryException),
                            Func<IDbConnection, IDbCommand?, IDataReader?, Exception, T> catchFunc5
                                => catchFunc5(dbConnection, dbCommand, dataReader, tryException),
                            Func<IDbConnection, IDbTransaction?, IDbCommand?, IDataReader?, Exception, T> catchFunc6
                                => catchFunc6(dbConnection, dbTransaction, dbCommand, dataReader, tryException),
                            _ => throw new InvalidCastException(nameof(catchFunc)),
                        };
                        if (dbTransaction is not null && usingOptions.AllowTransactionCommitOnSuccessCatchFunc)
                            dbTransaction.Commit();
                        return result;
                    }
                }
                catch (Exception catchException)
                {
                    var exceptions = new List<Exception>
                    {
                        tryException,
                        catchException,
                    };
                    if (catchException is not null && dbTransaction is not null && usingOptions.AllowTransactionRollbackOnFailCatchFunc)
                    {
                        try
                        {
                            dbTransaction.Rollback();
                        }
                        catch (Exception rollbackException)
                        {
                            exceptions.Add(rollbackException);
                        }
                    }
                    if (!Equals(tryException, catchException))
                        throw new AggregateException(exceptions);
                }
                throw;
            }
            finally
            {
                dataReader?.Close();
                dataReader?.Dispose();
                dbCommand?.Dispose();
                dbTransaction?.Dispose();
                if (connectionState.HasValue && connectionState is not ConnectionState.Open)
                    dbConnection.Close();
            }
        }
        public static T Using<T>(
            this IDbConnection dbConnection,
            Func<IDbConnection, T> tryFunc,
            Func<IDbConnection, Exception, T>? catchFunc = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.InternalUsing<T>(
                tryFunc: tryFunc,
                catchFunc: catchFunc,
                usingOptions: usingOptions);
        public static T Using<T>(
            this IDbConnection dbConnection,
            Func<IDbConnection, IDbTransaction, T> tryFunc,
            Func<IDbConnection, IDbTransaction?, Exception, T>? catchFunc = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.InternalUsing<T>(
                tryFunc: tryFunc,
                catchFunc: catchFunc,
                allowDbTransaction: true,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static T Using<T>(
            this IDbConnection dbConnection,
            Func<IDbConnection, IDbCommand, T> tryFunc,
            Func<IDbConnection, IDbCommand?, Exception, T>? catchFunc = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.InternalUsing<T>(
                tryFunc: tryFunc,
                catchFunc: catchFunc,
                allowDbCommand: true,
                usingOptions: usingOptions);
        public static T Using<T>(
            this IDbConnection dbConnection,
            Func<IDbConnection, IDbTransaction, IDbCommand, T> tryFunc,
            Func<IDbConnection, IDbTransaction?, IDbCommand?, Exception, T>? catchFunc = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.InternalUsing<T>(
                tryFunc: tryFunc,
                catchFunc: catchFunc,
                allowDbTransaction: true,
                allowDbCommand: true,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static T Using<T>(
            this IDbConnection dbConnection,
            Func<IDbConnection, IDbCommand, IDataReader, T> tryFunc,
            Func<IDbConnection, IDbCommand?, IDataReader?, Exception, T>? catchFunc = null,
            CommandBehavior? commandBehavior = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.InternalUsing<T>(
                tryFunc: tryFunc,
                catchFunc: catchFunc,
                allowDbCommand: true,
                allowDataReader: true,
                commandBehavior: commandBehavior,
                usingOptions: usingOptions);
        public static T Using<T>(
            this IDbConnection dbConnection,
            Func<IDbConnection, IDbTransaction, IDbCommand, IDataReader, T> tryFunc,
            Func<IDbConnection, IDbTransaction?, IDbCommand?, IDataReader?, Exception, T>? catchFunc = null,
            IsolationLevel? isolationLevel = null,
            CommandBehavior? commandBehavior = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.InternalUsing<T>(
                tryFunc: tryFunc,
                catchFunc: catchFunc,
                allowDbTransaction: true,
                allowDbCommand: true,
                allowDataReader: true,
                isolationLevel: isolationLevel,
                commandBehavior: commandBehavior,
                usingOptions: usingOptions);
        #endregion
        #region Execute IDbCommand
        public static int ExecuteNonQuery(
            this IDbConnection dbConnection,
            Action<IDbCommand> beforeExecute,
            Action<IDbCommand>? afterExecute = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.Using(
                tryFunc: (dbConnection, dbTransaction, dbCommand) => dbCommand.ExecuteNonQuery(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute),
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static T ExecuteReader<T>(
            this IDbConnection dbConnection,
            Action<IDbCommand> beforeExecute,
            Func<IDataReader, T>? constructor = null,
            Action<IDbCommand, T>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.Using(
                tryFunc: (dbConnection, dbTransaction, dbCommand) => dbCommand.ExecuteReader(
                    beforeExecute: beforeExecute,
                    constructor: constructor,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior),
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static object? ExecuteScalar(
            this IDbConnection dbConnection,
            Action<IDbCommand> beforeExecute,
            Action<IDbCommand, object?>? afterExecute = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.Using(
                tryFunc: (dbConnection, dbTransaction, dbCommand) => dbCommand.ExecuteScalar(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute),
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static T? ExecuteScalar<T>(
            this IDbConnection dbConnection,
            Action<IDbCommand> beforeExecute,
            Func<object?, T?>? constructor = null,
            Action<IDbCommand, T?>? afterExecute = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.Using(
                tryFunc: (dbConnection, dbTransaction, dbCommand) => dbCommand.ExecuteScalar(
                    beforeExecute: beforeExecute,
                    constructor: constructor,
                    afterExecute: afterExecute),
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        #endregion
        #region GetEnumerable
        public static TCollection GetCollection<TCollection, T>(
            this IDbConnection dbConnection,
            Action<IDbCommand> beforeExecute,
            Func<IDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex = null,
            Action<IDbCommand, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null)
            where TCollection : ICollection<T>, new()
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dataReader => dataReader.GetCollection<TCollection, T>(
                    constructorByIndex: constructorByIndex),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel);
        public static TCollection GetCollection<TCollection, T>(
            this IDbConnection dbConnection,
            Action<IDbCommand> beforeExecute,
            Func<IDataReader, ValueTuple<bool, T>>? constructor,
            Action<IDbCommand, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null)
            where TCollection : ICollection<T>, new()
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dataReader => dataReader.GetCollection<TCollection, T>(
                    constructor: constructor),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel);
        public static List<T> GetList<T>(
            this IDbConnection dbConnection,
            Action<IDbCommand> beforeExecute,
            Func<IDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex = null,
            Action<IDbCommand, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dataReader => dataReader.GetList(
                    constructorByIndex: constructorByIndex),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel);
        public static List<T> GetList<T>(
            this IDbConnection dbConnection,
            Action<IDbCommand> beforeExecute,
            Func<IDataReader, ValueTuple<bool, T>>? constructor,
            Action<IDbCommand, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dataReader => dataReader.GetList(
                    constructor: constructor),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel);
        public static Dictionary<int, T> GetDictionary<T>(
            this IDbConnection dbConnection,
            Action<IDbCommand> beforeExecute,
            Func<IDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex = null,
            Action<IDbCommand, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dataReader => dataReader.GetDictionary(
                    constructorByIndex: constructorByIndex),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel);
        public static Dictionary<int, T> GetDictionary<T>(
            this IDbConnection dbConnection,
            Action<IDbCommand> beforeExecute,
            Func<IDataReader, ValueTuple<bool, T>>? constructor,
            Action<IDbCommand, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dataReader => dataReader.GetDictionary(
                    constructor: constructor),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel);
        public static DataTable? GetSchemaTable(
            this IDbConnection dbConnection,
            Action<IDbCommand> beforeExecute,
            Action<IDbCommand, DataTable?>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dataReader => dataReader.GetSchemaTable(),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel);
        public static DataTable GetDataTable(
            this IDbConnection dbConnection,
            Action<IDbCommand> beforeExecute,
            Action<IDbCommand, DataTable>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetDataTable,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel);
        public static Dictionary<int, Dictionary<string, object?>> GetRawTable(
            this IDbConnection dbConnection,
            Action<IDbCommand> beforeExecute,
            Action<IDbCommand, Dictionary<int, Dictionary<string, object?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetRawTable,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel);
        public static List<Tuple<T1?, T2?>> GetTuples<T1, T2>(
            this IDbConnection dbConnection,
            Action<IDbCommand> beforeExecute,
            Action<IDbCommand, List<Tuple<T1?, T2?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetTuples<T1, T2>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel);
        public static List<Tuple<T1?, T2?, T3?>> GetTuples<T1, T2, T3>(
            this IDbConnection dbConnection,
            Action<IDbCommand> beforeExecute,
            Action<IDbCommand, List<Tuple<T1?, T2?, T3?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetTuples<T1, T2, T3>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel);
        public static List<Tuple<T1?, T2?, T3?, T4?>> GetTuples<T1, T2, T3, T4>(
            this IDbConnection dbConnection,
            Action<IDbCommand> beforeExecute,
            Action<IDbCommand, List<Tuple<T1?, T2?, T3?, T4?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetTuples<T1, T2, T3, T4>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?>> GetTuples<T1, T2, T3, T4, T5>(
            this IDbConnection dbConnection,
            Action<IDbCommand> beforeExecute,
            Action<IDbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetTuples<T1, T2, T3, T4, T5>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>> GetTuples<T1, T2, T3, T4, T5, T6>(
            this IDbConnection dbConnection,
            Action<IDbCommand> beforeExecute,
            Action<IDbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetTuples<T1, T2, T3, T4, T5, T6>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>> GetTuples<T1, T2, T3, T4, T5, T6, T7>(
            this IDbConnection dbConnection,
            Action<IDbCommand> beforeExecute,
            Action<IDbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetTuples<T1, T2, T3, T4, T5, T6, T7>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8>(
            this IDbConnection dbConnection,
            Action<IDbCommand> beforeExecute,
            Action<IDbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetTuples<T1, T2, T3, T4, T5, T6, T7, T8>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this IDbConnection dbConnection,
            Action<IDbCommand> beforeExecute,
            Action<IDbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this IDbConnection dbConnection,
            Action<IDbCommand> beforeExecute,
            Action<IDbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel);
        #endregion
        #region Check
        public static bool Check(
            this IDbConnection dbConnection)
            => dbConnection.Using(
                tryFunc: _ => true,
                catchFunc: (_, _) => false);

        #endregion
        #region HasRows
        public static bool HasRows(
            this IDbConnection dbConnection,
            Action<IDbCommand> beforeExecute,
            Action<IDbCommand, bool>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dataReader => dataReader.Read(),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        #endregion
    }
}
