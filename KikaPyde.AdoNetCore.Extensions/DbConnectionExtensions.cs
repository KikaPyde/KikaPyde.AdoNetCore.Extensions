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
        #region Using
        private static T InternalUsing<T>(
            Func<DbConnection> factory,
            Func<DbConnection, T> tryFunc,
            Func<DbConnection?, Exception, T>? catchFunc = null)
        {
            DbConnection? dbConnection = null;
            var hasDbConnection = false;
            try
            {
                dbConnection = factory();
                hasDbConnection = true;
                return tryFunc(dbConnection);
            }
            catch (Exception tryException)
            {
                if (!hasDbConnection && catchFunc is not null)
                {
                    try
                    {
                        return catchFunc(dbConnection, tryException);
                    }
                    catch (Exception catchException)
                    {
                        if (!Equals(tryException, catchException))
                            throw new AggregateException(tryException, catchException);
                    }
                }
                throw;
            }
            finally
            {
                if (dbConnection is not null)
                    dbConnection.Dispose();
            }
        }
        private static T InternalUsing<T>(
            this DbConnection dbConnection,
            bool allowDbTransaction,
            bool allowDbCommand,
            bool allowDbBatch,
            bool allowDbDataReader,
            Delegate tryFunc,
            Delegate? catchFunc,
            IsolationLevel? isolationLevel,
            CommandBehavior? commandBehavior,
            IUsingOptions? usingOptions)
        {
            DbTransaction? dbTransaction = null;
            DbCommand? dbCommand = null;
#if NET6_0_OR_GREATER
            DbBatch? dbBatch = null;
#endif
            DbDataReader? dbDataReader = null;
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
#if NET6_0_OR_GREATER
                if (allowDbCommand && allowDbBatch)
                    throw new InvalidOperationException();
                else if (allowDbCommand)
                {
                    dbCommand = dbConnection.CreateCommand();
                    dbCommand.Transaction = dbTransaction;
                }
                else if (allowDbBatch)
                {
                    dbBatch = dbConnection.CreateBatch();
                    dbBatch.Transaction = dbTransaction;
                }
#else
                if (allowDbCommand)
                {
                    dbCommand = dbConnection.CreateCommand();
                    dbCommand.Transaction = dbTransaction;
                }
#endif
                if (allowDbDataReader)
                {
                    if (commandBehavior.HasValue)
                    {
                        if (dbCommand is not null)
                            dbDataReader = dbCommand.ExecuteReader(commandBehavior.Value);
#if NET6_0_OR_GREATER
                        else if (dbBatch is not null)
                            dbDataReader = dbBatch.ExecuteReader(commandBehavior.Value);
#endif
                        else
                            throw new NullReferenceException(nameof(dbDataReader));
                    }
                    else
                    {
                        if (dbCommand is not null)
                            dbDataReader = dbCommand.ExecuteReader();
#if NET6_0_OR_GREATER
                        else if (dbBatch is not null)
                            dbDataReader = dbBatch.ExecuteReader();
#endif
                        else
                            throw new NullReferenceException(nameof(dbDataReader));
                    }
                }
                var result = tryFunc switch
                {
                    null => throw new ArgumentNullException(nameof(tryFunc)),
                    Func<DbConnection, T> tryFunc2 when !allowDbTransaction && !allowDbCommand && !allowDbBatch && !allowDbDataReader
                        => tryFunc2.Invoke(dbConnection),
                    Func<DbConnection, DbTransaction, T> tryFunc3 when allowDbTransaction && dbTransaction is not null && !allowDbCommand && !allowDbBatch && !allowDbDataReader
                        => tryFunc3.Invoke(dbConnection, dbTransaction),
                    Func<DbConnection, DbCommand, T> tryFunc3 when allowDbCommand && dbCommand is not null && !allowDbTransaction && !allowDbBatch && !allowDbDataReader
                        => tryFunc3.Invoke(dbConnection, dbCommand),
#if NET6_0_OR_GREATER
                    Func<DbConnection, DbBatch, T> tryFunc3 when allowDbBatch && dbBatch is not null && !allowDbTransaction && !allowDbCommand && !allowDbDataReader
                        => tryFunc3.Invoke(dbConnection, dbBatch),
#endif
                    Func<DbConnection, DbTransaction, DbCommand, T> tryFunc4 when allowDbTransaction && dbTransaction is not null && allowDbCommand && dbCommand is not null && !allowDbBatch && !allowDbDataReader
                        => tryFunc4.Invoke(dbConnection, dbTransaction, dbCommand),
#if NET6_0_OR_GREATER
                    Func<DbConnection, DbTransaction, DbBatch, T> tryFunc4 when allowDbTransaction && dbTransaction is not null && allowDbBatch && dbBatch is not null && !allowDbCommand && !allowDbDataReader
                        => tryFunc4.Invoke(dbConnection, dbTransaction, dbBatch),
#endif
                    Func<DbConnection, DbCommand, DbDataReader, T> tryFunc4 when allowDbCommand && dbCommand is not null && allowDbDataReader && dbDataReader is not null && !allowDbBatch && !allowDbTransaction
                        => tryFunc4.Invoke(dbConnection, dbCommand, dbDataReader),
#if NET6_0_OR_GREATER
                    Func<DbConnection, DbBatch, DbDataReader, T> tryFunc4 when allowDbBatch && dbBatch is not null && allowDbDataReader && dbDataReader is not null && !allowDbCommand && !allowDbTransaction
                        => tryFunc4.Invoke(dbConnection, dbBatch, dbDataReader),
#endif
                    Func<DbConnection, DbTransaction, DbCommand, DbDataReader, T> tryFunc5 when allowDbTransaction && dbTransaction is not null && allowDbCommand && dbCommand is not null && allowDbDataReader && dbDataReader is not null && !allowDbBatch
                        => tryFunc5.Invoke(dbConnection, dbTransaction, dbCommand, dbDataReader),
#if NET6_0_OR_GREATER
                    Func<DbConnection, DbTransaction, DbBatch, DbDataReader, T> tryFunc5 when allowDbTransaction && dbTransaction is not null && allowDbBatch && dbBatch is not null && allowDbDataReader && dbDataReader is not null && !allowDbCommand
                        => tryFunc5.Invoke(dbConnection, dbTransaction, dbBatch, dbDataReader),
#endif
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
                            Func<DbConnection?, Exception, T> catchFunc3 when !allowDbTransaction && !allowDbCommand && !allowDbBatch && !allowDbDataReader
                                => catchFunc3.Invoke(dbConnection, tryException),
                            Func<DbConnection?, DbTransaction?, Exception, T> catchFunc4 when allowDbTransaction && !allowDbCommand && !allowDbBatch && !allowDbDataReader
                                => catchFunc4.Invoke(dbConnection, dbTransaction, tryException),
                            Func<DbConnection?, DbCommand?, Exception, T> catchFunc4 when allowDbCommand && !allowDbTransaction && !allowDbBatch && !allowDbDataReader
                                => catchFunc4.Invoke(dbConnection, dbCommand, tryException),
#if NET6_0_OR_GREATER
                            Func<DbConnection?, DbBatch?, Exception, T> catchFunc4 when allowDbBatch && !allowDbTransaction && !allowDbBatch && !allowDbDataReader
                                => catchFunc4.Invoke(dbConnection, dbBatch, tryException),
#endif
                            Func<DbConnection?, DbTransaction?, DbCommand?, Exception, T> catchFunc5 when allowDbTransaction && allowDbCommand && !allowDbBatch && !allowDbDataReader
                                => catchFunc5.Invoke(dbConnection, dbTransaction, dbCommand, tryException),
#if NET6_0_OR_GREATER
                            Func<DbConnection?, DbTransaction?, DbBatch?, Exception, T> catchFunc5 when allowDbTransaction && allowDbBatch && !allowDbCommand && !allowDbDataReader
                                => catchFunc5.Invoke(dbConnection, dbTransaction, dbBatch, tryException),
#endif
                            Func<DbConnection?, DbCommand?, DbDataReader?, Exception, T> tryFunc5 when allowDbCommand && allowDbDataReader && !allowDbBatch && !allowDbTransaction
                                => tryFunc5.Invoke(dbConnection, dbCommand, dbDataReader, tryException),
#if NET6_0_OR_GREATER
                            Func<DbConnection?, DbBatch?, DbDataReader?, Exception, T> tryFunc5 when allowDbBatch && allowDbDataReader && !allowDbCommand && !allowDbTransaction
                                => tryFunc5.Invoke(dbConnection, dbBatch, dbDataReader, tryException),
#endif
                            Func<DbConnection?, DbTransaction?, DbCommand?, DbDataReader?, Exception, T> tryFunc6 when allowDbTransaction && allowDbCommand && allowDbDataReader && !allowDbBatch
                                => tryFunc6.Invoke(dbConnection, dbTransaction, dbCommand, dbDataReader, tryException),
#if NET6_0_OR_GREATER
                            Func<DbConnection?, DbTransaction?, DbBatch?, DbDataReader?, Exception, T> tryFunc6 when allowDbTransaction && allowDbBatch && allowDbDataReader && !allowDbCommand
                                => tryFunc6.Invoke(dbConnection, dbTransaction, dbBatch, dbDataReader, tryException),
#endif
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
                dbDataReader?.Close();
                dbDataReader?.Dispose();
#if NET6_0_OR_GREATER
                dbBatch?.Dispose();
#endif
                dbCommand?.Dispose();
                dbTransaction?.Dispose();
                if (connectionState.HasValue && connectionState is not ConnectionState.Open)
                    dbConnection.Close();
            }
        }
        public static T Using<T>(
            this DbConnection dbConnection,
            Func<DbConnection, T> tryFunc,
            Func<DbConnection, Exception, T>? catchFunc = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.InternalUsing<T>(
                allowDbTransaction: false,
                allowDbCommand: false,
                allowDbBatch: false,
                allowDbDataReader: false,
                tryFunc: tryFunc,
                catchFunc: catchFunc,
                isolationLevel: null,
                commandBehavior: null,
                usingOptions: usingOptions);
        public static T Using<T>(
            this DbConnection dbConnection,
            Func<DbConnection, DbTransaction, T> tryFunc,
            Func<DbConnection, DbTransaction?, Exception, T>? catchFunc = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.InternalUsing<T>(
                allowDbTransaction: true,
                allowDbCommand: false,
                allowDbBatch: false,
                allowDbDataReader: false,
                tryFunc: tryFunc,
                catchFunc: catchFunc,
                isolationLevel: isolationLevel,
                commandBehavior: null,
                usingOptions: usingOptions);
        public static T Using<T>(
            this DbConnection dbConnection,
            Func<DbConnection, DbCommand, T> tryFunc,
            Func<DbConnection, DbCommand?, Exception, T>? catchFunc = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.InternalUsing<T>(
                allowDbTransaction: false,
                allowDbCommand: true,
                allowDbBatch: false,
                allowDbDataReader: false,
                tryFunc: tryFunc,
                catchFunc: catchFunc,
                isolationLevel: null,
                commandBehavior: null,
                usingOptions: usingOptions);
#if NET6_0_OR_GREATER
        public static T Using<T>(
            this DbConnection dbConnection,
            Func<DbConnection, DbBatch, T> tryFunc,
            Func<DbConnection, DbBatch?, Exception, T>? catchFunc = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.InternalUsing<T>(
                allowDbTransaction: false,
                allowDbCommand: false,
                allowDbBatch: true,
                allowDbDataReader: false,
                tryFunc: tryFunc,
                catchFunc: catchFunc,
                isolationLevel: null,
                commandBehavior: null,
                usingOptions: usingOptions);
#endif
        public static T Using<T>(
            this DbConnection dbConnection,
            Func<DbConnection, DbTransaction, DbCommand, T> tryFunc,
            Func<DbConnection, DbTransaction?, DbCommand?, Exception, T>? catchFunc = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.InternalUsing<T>(
                allowDbTransaction: true,
                allowDbCommand: true,
                allowDbBatch: false,
                allowDbDataReader: false,
                tryFunc: tryFunc,
                catchFunc: catchFunc,
                isolationLevel: isolationLevel,
                commandBehavior: null,
                usingOptions: usingOptions);
#if NET6_0_OR_GREATER
        public static T Using<T>(
            this DbConnection dbConnection,
            Func<DbConnection, DbTransaction, DbBatch, T> tryFunc,
            Func<DbConnection, DbTransaction?, DbBatch?, Exception, T>? catchFunc = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.InternalUsing<T>(
                allowDbTransaction: true,
                allowDbCommand: false,
                allowDbBatch: true,
                allowDbDataReader: false,
                tryFunc: tryFunc,
                catchFunc: catchFunc,
                isolationLevel: isolationLevel,
                commandBehavior: null,
                usingOptions: usingOptions);
#endif
        public static T Using<T>(
            this DbConnection dbConnection,
            Func<DbConnection, DbCommand, DbDataReader, T> tryFunc,
            Func<DbConnection, DbCommand?, DbDataReader?, Exception, T>? catchFunc = null,
            CommandBehavior? commandBehavior = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.InternalUsing<T>(
                allowDbTransaction: false,
                allowDbCommand: true,
                allowDbBatch: false,
                allowDbDataReader: true,
                tryFunc: tryFunc,
                catchFunc: catchFunc,
                isolationLevel: null,
                commandBehavior: commandBehavior,
                usingOptions: usingOptions);
#if NET6_0_OR_GREATER
        public static T Using<T>(
            this DbConnection dbConnection,
            Func<DbConnection, DbBatch, DbDataReader, T> tryFunc,
            Func<DbConnection, DbBatch?, DbDataReader?, Exception, T>? catchFunc = null,
            CommandBehavior? commandBehavior = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.InternalUsing<T>(
                allowDbTransaction: false,
                allowDbCommand: false,
                allowDbBatch: true,
                allowDbDataReader: true,
                tryFunc: tryFunc,
                catchFunc: catchFunc,
                isolationLevel: null,
                commandBehavior: commandBehavior,
                usingOptions: usingOptions);
#endif
        public static T Using<T>(
            this DbConnection dbConnection,
            Func<DbConnection, DbTransaction, DbCommand, DbDataReader, T> tryFunc,
            Func<DbConnection, DbTransaction?, DbCommand?, DbDataReader?, Exception, T>? catchFunc = null,
            IsolationLevel? isolationLevel = null,
            CommandBehavior? commandBehavior = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.InternalUsing<T>(
                allowDbTransaction: true,
                allowDbCommand: true,
                allowDbBatch: false,
                allowDbDataReader: true,
                tryFunc: tryFunc,
                catchFunc: catchFunc,
                isolationLevel: isolationLevel,
                commandBehavior: commandBehavior,
                usingOptions: usingOptions);
#if NET6_0_OR_GREATER
        public static T Using<T>(
            this DbConnection dbConnection,
            Func<DbConnection, DbTransaction, DbBatch, DbDataReader, T> tryFunc,
            Func<DbConnection, DbTransaction?, DbBatch?, DbDataReader?, Exception, T>? catchFunc = null,
            IsolationLevel? isolationLevel = null,
            CommandBehavior? commandBehavior = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.InternalUsing<T>(
                allowDbTransaction: true,
                allowDbCommand: false,
                allowDbBatch: true,
                allowDbDataReader: true,
                tryFunc: tryFunc,
                catchFunc: catchFunc,
                isolationLevel: isolationLevel,
                commandBehavior: commandBehavior,
                usingOptions: usingOptions);
#endif
        #endregion
        #region UsingAsync
        private static async Task<T> InternalUsingAsync<T>(
            Func<CancellationToken, Task<DbConnection>> factory,
            Func<DbConnection, CancellationToken, CancellationToken?, Task<T>> tryFunc,
            Func<DbConnection?, Exception, CancellationToken, Task<T>>? catchFunc = null,
            CancellationToken cancellationToken = default,
            CancellationToken? catchCancellationToken = default)
        {
            DbConnection? dbConnection = null;
            var hasDbConnection = false;
            try
            {
                dbConnection = await factory(cancellationToken);
                hasDbConnection = true;
                return await tryFunc(dbConnection, cancellationToken, catchCancellationToken);
            }
            catch (Exception tryException)
            {
                if (!hasDbConnection && catchFunc is not null)
                {
                    try
                    {
                        return await catchFunc(dbConnection, tryException, catchCancellationToken ?? cancellationToken);
                    }
                    catch (Exception catchException)
                    {
                        if (!Equals(tryException, catchException))
                            throw new AggregateException(tryException, catchException);
                    }
                }
                throw;
            }
            finally
            {
                if (dbConnection is not null)
                    await dbConnection.DisposeAsync();
            }
        }
        private static async Task<T> InternalUsingAsync<T>(
            this DbConnection dbConnection,
            bool allowDbTransaction,
            bool allowDbCommand,
            bool allowDbBatch,
            bool allowDbDataReader,
            Delegate tryFunc,
            Delegate? catchFunc,
            IsolationLevel? isolationLevel,
            CommandBehavior? commandBehavior,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default,
            CancellationToken? catchCancellationToken = default)
        {
            DbTransaction? dbTransaction = null;
            DbCommand? dbCommand = null;
#if NET6_0_OR_GREATER
            DbBatch? dbBatch = null;
#endif
            DbDataReader? dbDataReader = null;
            ConnectionState? connectionState = null;
            usingOptions = usingOptions ?? DefaultUsingOptions;
            try
            {
                connectionState = dbConnection.State;
                if (connectionState is not ConnectionState.Open)
                    await dbConnection.OpenAsync(cancellationToken);
                if (allowDbTransaction)
                {
                    dbTransaction = isolationLevel.HasValue
                        ? await dbConnection.BeginTransactionAsync(isolationLevel.Value, cancellationToken)
                        : await dbConnection.BeginTransactionAsync(cancellationToken);
                }
#if NET6_0_OR_GREATER
                if (allowDbCommand && allowDbBatch)
                    throw new InvalidOperationException();
                else if (allowDbCommand)
                {
                    dbCommand = dbConnection.CreateCommand();
                    dbCommand.Transaction = dbTransaction;
                }
                else if (allowDbBatch)
                {
                    dbBatch = dbConnection.CreateBatch();
                    dbBatch.Transaction = dbTransaction;
                }
#else
                if (allowDbCommand)
                {
                    dbCommand = dbConnection.CreateCommand();
                    dbCommand.Transaction = dbTransaction;
                }
#endif
                if (allowDbDataReader)
                {
                    if (commandBehavior.HasValue)
                    {
                        if (dbCommand is not null)
                            dbDataReader = await dbCommand.ExecuteReaderAsync(commandBehavior.Value, cancellationToken);
#if NET6_0_OR_GREATER
                        else if (dbBatch is not null)
                            dbDataReader = await dbBatch.ExecuteReaderAsync(commandBehavior.Value, cancellationToken);
#endif
                        else
                            throw new NullReferenceException(nameof(dbDataReader));
                    }
                    else
                    {
                        if (dbCommand is not null)
                            dbDataReader = await dbCommand.ExecuteReaderAsync(cancellationToken);
#if NET6_0_OR_GREATER
                        else if (dbBatch is not null)
                            dbDataReader = await dbBatch.ExecuteReaderAsync(cancellationToken);
#endif
                        else
                            throw new NullReferenceException(nameof(dbDataReader));
                    }
                }
                var result = tryFunc switch
                {
                    null => throw new ArgumentNullException(nameof(tryFunc)),
                    Func<DbConnection, CancellationToken, Task<T>> tryFunc3 when !allowDbTransaction && !allowDbCommand && !allowDbBatch && !allowDbDataReader
                        => await tryFunc3.Invoke(dbConnection, cancellationToken),
                    Func<DbConnection, DbTransaction, CancellationToken, Task<T>> tryFunc4 when allowDbTransaction && dbTransaction is not null && !allowDbCommand && !allowDbBatch && !allowDbDataReader
                        => await tryFunc4.Invoke(dbConnection, dbTransaction, cancellationToken),
                    Func<DbConnection, DbCommand, CancellationToken, Task<T>> tryFunc4 when allowDbCommand && dbCommand is not null && !allowDbTransaction && !allowDbBatch && !allowDbDataReader
                        => await tryFunc4.Invoke(dbConnection, dbCommand, cancellationToken),
#if NET6_0_OR_GREATER
                    Func<DbConnection, DbBatch, CancellationToken, Task<T>> tryFunc4 when allowDbBatch && dbBatch is not null && !allowDbTransaction && !allowDbCommand && !allowDbDataReader
                        => await tryFunc4.Invoke(dbConnection, dbBatch, cancellationToken),
#endif
                    Func<DbConnection, DbTransaction, DbCommand, CancellationToken, Task<T>> tryFunc5 when allowDbTransaction && dbTransaction is not null && allowDbCommand && dbCommand is not null && !allowDbBatch && !allowDbDataReader
                        => await tryFunc5.Invoke(dbConnection, dbTransaction, dbCommand, cancellationToken),
#if NET6_0_OR_GREATER
                    Func<DbConnection, DbTransaction, DbBatch, CancellationToken, Task<T>> tryFunc5 when allowDbTransaction && dbTransaction is not null && allowDbBatch && dbBatch is not null && !allowDbCommand && !allowDbDataReader
                        => await tryFunc5.Invoke(dbConnection, dbTransaction, dbBatch, cancellationToken),
#endif
                    Func<DbConnection, DbCommand, DbDataReader, CancellationToken, Task<T>> tryFunc5 when allowDbCommand && dbCommand is not null && allowDbDataReader && dbDataReader is not null && !allowDbTransaction && !allowDbBatch
                        => await tryFunc5.Invoke(dbConnection, dbCommand, dbDataReader, cancellationToken),
#if NET6_0_OR_GREATER
                    Func<DbConnection, DbBatch, DbDataReader, CancellationToken, Task<T>> tryFunc5 when allowDbBatch && dbBatch is not null && allowDbDataReader && dbDataReader is not null && !allowDbTransaction && !allowDbCommand
                        => await tryFunc5.Invoke(dbConnection, dbBatch, dbDataReader, cancellationToken),
#endif
                    Func<DbConnection, DbTransaction, DbCommand, DbDataReader, CancellationToken, Task<T>> tryFunc6 when allowDbTransaction && dbTransaction is not null && allowDbCommand && dbCommand is not null && allowDbDataReader && dbDataReader is not null && !allowDbBatch
                        => await tryFunc6.Invoke(dbConnection, dbTransaction, dbCommand, dbDataReader, cancellationToken),
#if NET6_0_OR_GREATER
                    Func<DbConnection, DbTransaction, DbBatch, DbDataReader, CancellationToken, Task<T>> tryFunc6 when allowDbTransaction && dbTransaction is not null && allowDbBatch && dbBatch is not null && allowDbDataReader && dbDataReader is not null && !allowDbCommand
                        => await tryFunc6.Invoke(dbConnection, dbTransaction, dbBatch, dbDataReader, cancellationToken),
#endif
                    _ => throw new InvalidCastException(nameof(tryFunc)),
                };
                if (dbTransaction is not null && usingOptions.AllowTransactionCommitOnSuccessTryFunc)
                    await dbTransaction.CommitAsync(CancellationToken.None);
                return result;
            }
            catch (Exception tryException)
            {
                try
                {
                    if (catchFunc is null)
                    {
                        if (dbTransaction is not null && usingOptions.AllowTransactionRollbackOnFailTryFunc)
                            await dbTransaction.RollbackAsync(CancellationToken.None);
                    }
                    else
                    {
                        var result = catchFunc switch
                        {
                            null => throw new ArgumentNullException(nameof(catchFunc)),
                            Func<DbConnection?, Exception, CancellationToken, Task<T>> catchFunc4 when !allowDbTransaction && !allowDbCommand && !allowDbBatch && !allowDbDataReader
                                => await catchFunc4.Invoke(dbConnection, tryException, catchCancellationToken ?? cancellationToken),
                            Func<DbConnection?, DbTransaction?, Exception, CancellationToken, Task<T>> catchFunc5 when allowDbTransaction && !allowDbCommand && !allowDbBatch && !allowDbDataReader
                                => await catchFunc5.Invoke(dbConnection, dbTransaction, tryException, catchCancellationToken ?? cancellationToken),
                            Func<DbConnection?, DbCommand?, Exception, CancellationToken, Task<T>> catchFunc5 when allowDbCommand && !allowDbTransaction && !allowDbBatch && !allowDbDataReader
                                => await catchFunc5.Invoke(dbConnection, dbCommand, tryException, catchCancellationToken ?? cancellationToken),
#if NET6_0_OR_GREATER
                            Func<DbConnection?, DbBatch?, Exception, CancellationToken, Task<T>> catchFunc5 when allowDbBatch && !allowDbTransaction && !allowDbBatch && !allowDbDataReader
                                => await catchFunc5.Invoke(dbConnection, dbBatch, tryException, catchCancellationToken ?? cancellationToken),
#endif
                            Func<DbConnection?, DbTransaction?, DbCommand?, Exception, CancellationToken, Task<T>> catchFunc6 when allowDbTransaction && allowDbCommand && !allowDbBatch && !allowDbDataReader
                                => await catchFunc6.Invoke(dbConnection, dbTransaction, dbCommand, tryException, catchCancellationToken ?? cancellationToken),
#if NET6_0_OR_GREATER
                            Func<DbConnection?, DbTransaction?, DbBatch?, Exception, CancellationToken, Task<T>> catchFunc6 when allowDbTransaction && allowDbBatch && !allowDbCommand && !allowDbDataReader
                                => await catchFunc6.Invoke(dbConnection, dbTransaction, dbBatch, tryException, catchCancellationToken ?? cancellationToken),
#endif
                            Func<DbConnection?, DbCommand?, DbDataReader?, Exception, CancellationToken, Task<T>> catchFunc6 when allowDbCommand && allowDbDataReader && !allowDbTransaction && !allowDbBatch
                                => await catchFunc6.Invoke(dbConnection, dbCommand, dbDataReader, tryException, catchCancellationToken ?? cancellationToken),
#if NET6_0_OR_GREATER
                            Func<DbConnection?, DbBatch?, DbDataReader?, Exception, CancellationToken, Task<T>> catchFunc6 when allowDbBatch && allowDbDataReader && !allowDbTransaction && !allowDbCommand
                                => await catchFunc6.Invoke(dbConnection, dbBatch, dbDataReader, tryException, catchCancellationToken ?? cancellationToken),
#endif
                            Func<DbConnection?, DbTransaction?, DbCommand?, DbDataReader?, Exception, CancellationToken, Task<T>> catchFunc7 when allowDbTransaction && allowDbCommand && allowDbDataReader && !allowDbBatch
                                => await catchFunc7.Invoke(dbConnection, dbTransaction, dbCommand, dbDataReader, tryException, catchCancellationToken ?? cancellationToken),
#if NET6_0_OR_GREATER
                            Func<DbConnection?, DbTransaction?, DbBatch?, DbDataReader?, Exception, CancellationToken, Task<T>> catchFunc7 when allowDbTransaction && allowDbBatch && allowDbDataReader && !allowDbCommand
                                => await catchFunc7.Invoke(dbConnection, dbTransaction, dbBatch, dbDataReader, tryException, catchCancellationToken ?? cancellationToken),
#endif                  
                            _ => throw new InvalidCastException(nameof(catchFunc)),
                        };
                        if (dbTransaction is not null && usingOptions.AllowTransactionCommitOnSuccessCatchFunc)
                            await dbTransaction.CommitAsync(CancellationToken.None);
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
                    if (catchFunc is not null && dbTransaction is not null && usingOptions.AllowTransactionRollbackOnFailCatchFunc)
                    {
                        try
                        {
                            await dbTransaction.RollbackAsync(CancellationToken.None);
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
                if (dbDataReader is not null)
                {
                    await dbDataReader.CloseAsync();
                    await dbDataReader.DisposeAsync();
                }
#if NET6_0_OR_GREATER
                if (dbBatch is not null)
                    await dbBatch.DisposeAsync();
#endif
                if (dbCommand is not null)
                    await dbCommand.DisposeAsync();
                if (dbTransaction is not null)
                    await dbTransaction.DisposeAsync();
                if (connectionState.HasValue && connectionState is not ConnectionState.Open)
                    await dbConnection.CloseAsync();
            }
        }
        public static async Task<T> UsingAsync<T>(
            this DbConnection dbConnection,
            Func<DbConnection, CancellationToken, Task<T>> tryFunc,
            Func<DbConnection, Exception, CancellationToken, Task<T>>? catchFunc = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default,
            CancellationToken? catchCancellationToken = default)
            => await dbConnection.InternalUsingAsync<T>(
                allowDbTransaction: false,
                allowDbCommand: false,
                allowDbBatch: false,
                allowDbDataReader: false,
                tryFunc: tryFunc,
                catchFunc: catchFunc,
                isolationLevel: null,
                commandBehavior: null,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken,
                catchCancellationToken: catchCancellationToken);
        public static async Task<T> UsingAsync<T>(
            this DbConnection dbConnection,
            Func<DbConnection, DbTransaction, CancellationToken, Task<T>> tryFunc,
            Func<DbConnection, DbTransaction?, Exception, CancellationToken, Task<T>>? catchFunc = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default,
            CancellationToken? catchCancellationToken = default)
            => await dbConnection.InternalUsingAsync<T>(
                allowDbTransaction: true,
                allowDbCommand: false,
                allowDbBatch: false,
                allowDbDataReader: false,
                tryFunc: tryFunc,
                catchFunc: catchFunc,
                isolationLevel: isolationLevel,
                commandBehavior: null,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken,
                catchCancellationToken: catchCancellationToken);
        public static async Task<T> UsingAsync<T>(
            this DbConnection dbConnection,
            Func<DbConnection, DbCommand, CancellationToken, Task<T>> tryFunc,
            Func<DbConnection, DbCommand?, Exception, CancellationToken, Task<T>>? catchFunc = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default,
            CancellationToken? catchCancellationToken = default)
            => await dbConnection.InternalUsingAsync<T>(
                allowDbTransaction: false,
                allowDbCommand: true,
                allowDbBatch: false,
                allowDbDataReader: false,
                tryFunc: tryFunc,
                catchFunc: catchFunc,
                isolationLevel: null,
                commandBehavior: null,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken,
                catchCancellationToken: catchCancellationToken);
#if NET6_0_OR_GREATER
        public static async Task<T> UsingAsync<T>(
            this DbConnection dbConnection,
            Func<DbConnection, DbBatch, CancellationToken, Task<T>> tryFunc,
            Func<DbConnection, DbBatch?, Exception, CancellationToken, Task<T>>? catchFunc = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default,
            CancellationToken? catchCancellationToken = default)
            => await dbConnection.InternalUsingAsync<T>(
                allowDbTransaction: false,
                allowDbCommand: false,
                allowDbBatch: true,
                allowDbDataReader: false,
                tryFunc: tryFunc,
                catchFunc: catchFunc,
                isolationLevel: null,
                commandBehavior: null,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken,
                catchCancellationToken: catchCancellationToken);
#endif
        public static async Task<T> UsingAsync<T>(
            this DbConnection dbConnection,
            Func<DbConnection, DbTransaction, DbCommand, CancellationToken, Task<T>> tryFunc,
            Func<DbConnection, DbTransaction?, DbCommand?, Exception, CancellationToken, Task<T>>? catchFunc = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default,
            CancellationToken? catchCancellationToken = default)
            => await dbConnection.InternalUsingAsync<T>(
                allowDbTransaction: true,
                allowDbCommand: true,
                allowDbBatch: false,
                allowDbDataReader: false,
                tryFunc: tryFunc,
                catchFunc: catchFunc,
                isolationLevel: isolationLevel,
                commandBehavior: null,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken,
                catchCancellationToken: catchCancellationToken);
#if NET6_0_OR_GREATER
        public static async Task<T> UsingAsync<T>(
            this DbConnection dbConnection,
            Func<DbConnection, DbTransaction, DbBatch, CancellationToken, Task<T>> tryFunc,
            Func<DbConnection, DbTransaction?, DbBatch?, Exception, CancellationToken, Task<T>>? catchFunc = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default,
            CancellationToken? catchCancellationToken = default)
            => await dbConnection.InternalUsingAsync<T>(
                allowDbTransaction: true,
                allowDbCommand: false,
                allowDbBatch: true,
                allowDbDataReader: false,
                tryFunc: tryFunc,
                catchFunc: catchFunc,
                isolationLevel: isolationLevel,
                commandBehavior: null,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken,
                catchCancellationToken: catchCancellationToken);
#endif
        public static async Task<T> UsingAsync<T>(
            this DbConnection dbConnection,
            Func<DbConnection, DbCommand, DbDataReader, CancellationToken, Task<T>> tryFunc,
            Func<DbConnection, DbCommand?, DbDataReader?, Exception, CancellationToken, Task<T>>? catchFunc = null,
            CommandBehavior? commandBehavior = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default,
            CancellationToken? catchCancellationToken = default)
            => await dbConnection.InternalUsingAsync<T>(
                allowDbTransaction: false,
                allowDbCommand: true,
                allowDbBatch: false,
                allowDbDataReader: true,
                tryFunc: tryFunc,
                catchFunc: catchFunc,
                isolationLevel: null,
                commandBehavior: commandBehavior,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken,
                catchCancellationToken: catchCancellationToken);
#if NET6_0_OR_GREATER
        public static async Task<T> UsingAsync<T>(
            this DbConnection dbConnection,
            Func<DbConnection, DbBatch, DbDataReader, CancellationToken, Task<T>> tryFunc,
            Func<DbConnection, DbBatch?, DbDataReader?, Exception, CancellationToken, Task<T>>? catchFunc = null,
            CommandBehavior? commandBehavior = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default,
            CancellationToken? catchCancellationToken = default)
            => await dbConnection.InternalUsingAsync<T>(
                allowDbTransaction: false,
                allowDbCommand: false,
                allowDbBatch: true,
                allowDbDataReader: true,
                tryFunc: tryFunc,
                catchFunc: catchFunc,
                isolationLevel: null,
                commandBehavior: commandBehavior,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken,
                catchCancellationToken: catchCancellationToken);
#endif
        public static async Task<T> UsingAsync<T>(
            this DbConnection dbConnection,
            Func<DbConnection, DbTransaction, DbCommand, DbDataReader, CancellationToken, Task<T>> tryFunc,
            Func<DbConnection, DbTransaction?, DbCommand?, DbDataReader?, Exception, CancellationToken, Task<T>>? catchFunc = null,
            IsolationLevel? isolationLevel = null,
            CommandBehavior? commandBehavior = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default,
            CancellationToken? catchCancellationToken = default)
            => await dbConnection.InternalUsingAsync<T>(
                allowDbTransaction: true,
                allowDbCommand: true,
                allowDbBatch: false,
                allowDbDataReader: true,
                tryFunc: tryFunc,
                catchFunc: catchFunc,
                isolationLevel: isolationLevel,
                commandBehavior: commandBehavior,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken,
                catchCancellationToken: catchCancellationToken);
#if NET6_0_OR_GREATER
        public static async Task<T> UsingAsync<T>(
            this DbConnection dbConnection,
            Func<DbConnection, DbTransaction, DbBatch, DbDataReader, CancellationToken, Task<T>> tryFunc,
            Func<DbConnection, DbTransaction?, DbBatch?, DbDataReader?, Exception, CancellationToken, Task<T>>? catchFunc = null,
            IsolationLevel? isolationLevel = null,
            CommandBehavior? commandBehavior = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default,
            CancellationToken? catchCancellationToken = default)
            => await dbConnection.InternalUsingAsync<T>(
                allowDbTransaction: true,
                allowDbCommand: false,
                allowDbBatch: true,
                allowDbDataReader: true,
                tryFunc: tryFunc,
                catchFunc: catchFunc,
                isolationLevel: isolationLevel,
                commandBehavior: commandBehavior,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken,
                catchCancellationToken: catchCancellationToken);
#endif
        #endregion
        #region Execute DbCommand
        public static int ExecuteNonQuery(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand>? afterExecute = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.Using(
                tryFunc: (dbConnection, dbTransaction, dbCommand) => dbCommand.ExecuteNonQuery(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute),
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static T ExecuteReader<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, T>? constructor = null,
            Action<DbCommand, T>? afterExecute = null,
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
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, object?>? afterExecute = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.Using(
                tryFunc: (dbConnection, dbTransaction, dbCommand) => dbCommand.ExecuteScalar(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute),
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static T? ExecuteScalar<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<object?, T?>? constructor = null,
            Action<DbCommand, T?>? afterExecute = null,
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
        #region ExecuteAsync DbCommand
        public static async Task<int> ExecuteNonQueryAsync(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand>? afterExecute = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.UsingAsync(
                tryFunc: async (dbConnection, dbTransaction, dbCommand, cancellationToken) => await dbCommand.ExecuteNonQueryAsync(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    cancellationToken: cancellationToken),
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<T> ExecuteReaderAsync<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<T>>? constructor = null,
            Action<DbCommand, T>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.UsingAsync(
                tryFunc: async (dbConnection, dbTransaction, dbCommand, cancellationToken) => await dbCommand.ExecuteReaderAsync(
                    beforeExecute: beforeExecute,
                    constructor: constructor,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    cancellationToken: cancellationToken),
                catchFunc: null,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken);
        public static async Task<object?> ExecuteScalarAsync(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, object?>? afterExecute = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.UsingAsync(
                tryFunc: async (dbConnection, dbTransaction, dbCommand, cancellationToken) => await dbCommand.ExecuteScalarAsync(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    cancellationToken: cancellationToken),
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<T?> ExecuteScalarAsync<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<object?, T?>? constructor = null,
            Action<DbCommand, T?>? afterExecute = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.UsingAsync(
                tryFunc: async (dbConnection, dbTransaction, dbCommand, cancellationToken) => await dbCommand.ExecuteScalarAsync(
                    beforeExecute: beforeExecute,
                    constructor: constructor,
                    afterExecute: afterExecute,
                    cancellationToken: cancellationToken),
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        #endregion
#if NET6_0_OR_GREATER
        #region Execute DbBatch
        public static int ExecuteNonQuery(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch>? afterExecute = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.Using(
                tryFunc: (dbConnection, dbTransaction, dbBatch) => dbBatch.ExecuteNonQuery(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute),
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static T ExecuteReader<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, T>? constructor = null,
            Action<DbBatch, T>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.Using(
                tryFunc: (dbConnection, dbTransaction, dbBatch) => dbBatch.ExecuteReader(
                    beforeExecute: beforeExecute,
                    constructor: constructor,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior),
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static object? ExecuteScalar(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, object?>? afterExecute = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.Using(
                tryFunc: (dbConnection, dbTransaction, dbCommand) => dbCommand.ExecuteScalar(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute),
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static T? ExecuteScalar<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<object?, T?>? constructor = null,
            Action<DbBatch, T?>? afterExecute = null,
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
        #region ExecuteAsync DbBatch
        public static async Task<int> ExecuteNonQueryAsync(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch>? afterExecute = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.UsingAsync(
                tryFunc: async (dbConnection, dbTransaction, dbBatch, cancellationToken) => await dbBatch.ExecuteNonQueryAsync(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    cancellationToken: cancellationToken),
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<T> ExecuteReaderAsync<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<T>>? constructor = null,
            Action<DbBatch, T>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.UsingAsync(
                tryFunc: async (dbConnection, dbTransaction, dbBatch, cancellationToken) => await dbBatch.ExecuteReaderAsync(
                    beforeExecute: beforeExecute,
                    constructor: constructor,
                    afterExecute: afterExecute,
                    commandBehavior: commandBehavior,
                    cancellationToken: cancellationToken),
                catchFunc: null,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken);
        public static async Task<object?> ExecuteScalarAsync(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, object?>? afterExecute = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.UsingAsync(
                tryFunc: async (dbConnection, dbTransaction, dbBatch, cancellationToken) => await dbBatch.ExecuteScalarAsync(
                    beforeExecute: beforeExecute,
                    afterExecute: afterExecute,
                    cancellationToken: cancellationToken),
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<T?> ExecuteScalarAsync<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<object?, T?>? constructor = null,
            Action<DbBatch, T?>? afterExecute = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.UsingAsync(
                tryFunc: async (dbConnection, dbTransaction, dbBatch, cancellationToken) => await dbBatch.ExecuteScalarAsync(
                    beforeExecute: beforeExecute,
                    constructor: constructor,
                    afterExecute: afterExecute,
                    cancellationToken: cancellationToken),
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        #endregion
#endif
        #region GetEnumerable DbCommand
        #region Range
        public static TCollection GetCollection<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            Action<DbCommand, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            where TCollection : ICollection<T>, new()
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetCollection<TCollection, T>(
                    constructorByIndex: constructorByIndex),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static TCollection GetCollection<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbCommand, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            where TCollection : ICollection<T>, new()
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetCollection<TCollection, T>(
                    constructor: constructor),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<T> GetList<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            Action<DbCommand, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetList(
                    constructorByIndex: constructorByIndex),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<T> GetList<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbCommand, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetList(
                    constructor: constructor),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static Dictionary<int, T> GetDictionary<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            Action<DbCommand, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetDictionary(
                    constructorByIndex: constructorByIndex),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static Dictionary<int, T> GetDictionary<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbCommand, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetDictionary(
                    constructor: constructor),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static DataTable? GetSchemaTable(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, DataTable?>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetSchemaTable(),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static DataTable GetDataTable(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, DataTable>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetDataTable,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static Dictionary<int, Dictionary<string, object?>> GetRawTable(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, Dictionary<int, Dictionary<string, object?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetRawTable,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?>> GetTuples<T1, T2>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetTuples<T1, T2>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?>> GetTuples<T1, T2, T3>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetTuples<T1, T2, T3>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?, T4?>> GetTuples<T1, T2, T3, T4>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetTuples<T1, T2, T3, T4>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?>> GetTuples<T1, T2, T3, T4, T5>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetTuples<T1, T2, T3, T4, T5>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>> GetTuples<T1, T2, T3, T4, T5, T6>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetTuples<T1, T2, T3, T4, T5, T6>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>> GetTuples<T1, T2, T3, T4, T5, T6, T7>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetTuples<T1, T2, T3, T4, T5, T6, T7>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetTuples<T1, T2, T3, T4, T5, T6, T7, T8>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        #endregion
        #region ResultRange
        public static TCollection GetResultCollection<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            Action<DbCommand, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            where TCollection : ICollection<T>, new()
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetResultCollection<TCollection, T>(
                    constructorByIndex: constructorByIndex),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static TCollection GetResultCollection<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbCommand, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            where TCollection : ICollection<T>, new()
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetResultCollection<TCollection, T>(
                    constructor: constructor),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static TCollection GetResultCollection<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, T> func,
            Action<DbCommand, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            where TCollection : ICollection<T>, new()
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetResultCollection<TCollection, T>(
                    func: func),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<T> GetResultList<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            Action<DbCommand, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetResultList(
                    constructorByIndex: constructorByIndex),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<T> GetResultList<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbCommand, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetResultList(
                    constructor: constructor),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<T> GetResultList<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader,T> func,
            Action<DbCommand, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetResultList(
                    func: func),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static Dictionary<int, T> GetResultDictionary<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            Action<DbCommand, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetResultDictionary(
                    constructorByIndex: constructorByIndex),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static Dictionary<int, T> GetResultDictionary<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbCommand, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetResultDictionary(
                    constructor: constructor),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static Dictionary<int, T> GetResultDictionary<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, T> func,
            Action<DbCommand, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetResultDictionary(
                    func: func),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static Dictionary<int, Dictionary<int, Dictionary<string, object?>>> GetRawDatabase(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, Dictionary<int, Dictionary<int, Dictionary<string, object?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetRawDatabase,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        #endregion
        #region GlobalRange
        public static TCollection GetGlobalCollection<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex = null,
            Action<DbCommand, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            where TCollection : ICollection<T>, new()
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetGlobalCollection<TCollection, T>(
                    constructorByIndex: constructorByIndex),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static TCollection GetGlobalCollection<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbCommand, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            where TCollection : ICollection<T>, new()
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetGlobalCollection<TCollection, T>(
                    constructor: constructor),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<T> GetGlobalList<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex = null,
            Action<DbCommand, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetGlobalList<T>(
                    constructorByIndex: constructorByIndex),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<T> GetGlobalList<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbCommand, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetGlobalList<T>(
                    constructor: constructor),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static Dictionary<int, T> GetGlobalDictionary<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex = null,
            Action<DbCommand, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetGlobalDictionary<T>(
                    constructorByIndex: constructorByIndex),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static Dictionary<int, T> GetGlobalDictionary<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbCommand, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetGlobalDictionary<T>(
                    constructor: constructor),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static Dictionary<int, Dictionary<string, object?>> GetGlobalRawTable(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, Dictionary<int, Dictionary<string, object?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetGlobalRawTable,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?>> GetGlobalTuples<T1, T2>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuples<T1, T2>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?>> GetGlobalTuples<T1, T2, T3>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuples<T1, T2, T3>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?, T4?>> GetGlobalTuples<T1, T2, T3, T4>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuples<T1, T2, T3, T4>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?>> GetGlobalTuples<T1, T2, T3, T4, T5>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuples<T1, T2, T3, T4, T5>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>> GetGlobalTuples<T1, T2, T3, T4, T5, T6>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuples<T1, T2, T3, T4, T5, T6>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>> GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>> GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7, T8>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7, T8>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>> GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>> GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        #endregion
        #endregion
        #region GetEnumerableAsync DbCommand
        #region RangeAsync
        public static async Task<TCollection> GetCollectionAsync<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            Action<DbCommand, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetCollectionAsync<TCollection, T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetCollectionAsync<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            Action<DbCommand, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetCollectionAsync<TCollection, T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetCollectionAsync<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex,
            Action<DbCommand, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetCollectionAsync<TCollection, T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetCollectionAsync<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbCommand, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetCollectionAsync<TCollection, T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetListAsync<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            Action<DbCommand, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetListAsync(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetListAsync<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            Action<DbCommand, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetListAsync(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetListAsync<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex,
            Action<DbCommand, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetListAsync(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetListAsync<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbCommand, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetListAsync(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetDictionaryAsync<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            Action<DbCommand, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetDictionaryAsync(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetDictionaryAsync<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            Action<DbCommand, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetDictionaryAsync(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetDictionaryAsync<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex,
            Action<DbCommand, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetDictionaryAsync(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetDictionaryAsync<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbCommand, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetDictionaryAsync(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<DataTable?> GetSchemaTableAsync(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, DataTable?>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetSchemaTableAsync(
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<DataTable> GetDataTableAsync(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, DataTable>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetDataTableAsync,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, Dictionary<string, object?>>> GetRawTableAsync(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, Dictionary<int, Dictionary<string, object?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetRawTableAsync,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?>>> GetTuplesAsync<T1, T2>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetTuplesAsync<T1, T2>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?>>> GetTuplesAsync<T1, T2, T3>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetTuplesAsync<T1, T2, T3>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?>>> GetTuplesAsync<T1, T2, T3, T4>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetTuplesAsync<T1, T2, T3, T4>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?>>> GetTuplesAsync<T1, T2, T3, T4, T5>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetTuplesAsync<T1, T2, T3, T4, T5>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetTuplesAsync<T1, T2, T3, T4, T5, T6>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        #endregion
        #region ResultRangeAsync
        public static async Task<TCollection> GetResultCollectionAsync<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            Action<DbCommand, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultCollectionAsync<TCollection, T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetResultCollectionAsync<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            Action<DbCommand, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultCollectionAsync<TCollection, T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetResultCollectionAsync<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<T>> func,
            Action<DbCommand, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultCollectionAsync<TCollection, T>(
                    func: func,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetResultListAsync<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            Action<DbCommand, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultListAsync<T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetResultListAsync<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            Action<DbCommand, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultListAsync<T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetResultListAsync<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<T>> func,
            Action<DbCommand, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultListAsync<T>(
                    func: func,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetResultDictionaryAsync<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            Action<DbCommand, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultDictionaryAsync<T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetResultDictionaryAsync<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            Action<DbCommand, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultDictionaryAsync<T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetResultDictionaryAsync<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<T>> func,
            Action<DbCommand, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultDictionaryAsync<T>(
                    func: func,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, Dictionary<int, Dictionary<string, object?>>>> GetRawDatabaseAsync(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, Dictionary<int, Dictionary<int, Dictionary<string, object?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetRawDatabaseAsync,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        #endregion
        #region GlobalRangeAsync
        public static async Task<TCollection> GetGlobalCollectionAsync<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, int, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            Action<DbCommand, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalCollectionAsync<TCollection, T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetGlobalCollectionAsync<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            Action<DbCommand, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalCollectionAsync<TCollection, T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetGlobalCollectionAsync<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex,
            Action<DbCommand, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalCollectionAsync<TCollection, T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetGlobalCollectionAsync<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbCommand, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalCollectionAsync<TCollection, T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetGlobalListAsync<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, int, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            Action<DbCommand, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalListAsync<T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetGlobalListAsync<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            Action<DbCommand, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalListAsync<T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetGlobalListAsync<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex,
            Action<DbCommand, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalListAsync<T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetGlobalListAsync<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbCommand, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalListAsync<T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetGlobalDictionaryAsync<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, int, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            Action<DbCommand, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalDictionaryAsync<T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetGlobalDictionaryAsync<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            Action<DbCommand, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalDictionaryAsync<T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetGlobalDictionaryAsync<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex,
            Action<DbCommand, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalDictionaryAsync<T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetGlobalDictionaryAsync<T>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbCommand, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalDictionaryAsync<T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, Dictionary<string, object?>>> GetGlobalRawTableAsync(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, Dictionary<int, Dictionary<string, object?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetGlobalRawTableAsync,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?>>> GetGlobalTuplesAsync<T1, T2>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuplesAsync<T1, T2>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?>>> GetGlobalTuplesAsync<T1, T2, T3>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuplesAsync<T1, T2, T3>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?>>> GetGlobalTuplesAsync<T1, T2, T3, T4>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuplesAsync<T1, T2, T3, T4>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?>>> GetGlobalTuplesAsync<T1, T2, T3, T4, T5>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuplesAsync<T1, T2, T3, T4, T5>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>>> GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>> GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>>> GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>>> GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>>> GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        #endregion
        #endregion
#if NET6_0_OR_GREATER
        #region GetEnumerable DbBatch
        #region Range
        public static TCollection GetCollection<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            Action<DbBatch, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            where TCollection : ICollection<T>, new()
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetCollection<TCollection, T>(
                    constructorByIndex: constructorByIndex),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static TCollection GetCollection<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbBatch, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            where TCollection : ICollection<T>, new()
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetCollection<TCollection, T>(
                    constructor: constructor),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<T> GetList<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            Action<DbBatch, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetList(
                    constructorByIndex: constructorByIndex),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<T> GetList<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbBatch, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetList(
                    constructor: constructor),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static Dictionary<int, T> GetDictionary<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            Action<DbBatch, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetDictionary(
                    constructorByIndex: constructorByIndex),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static Dictionary<int, T> GetDictionary<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbBatch, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetDictionary(
                    constructor: constructor),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static DataTable? GetSchemaTable(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, DataTable?>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetSchemaTable(),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static DataTable GetDataTable(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, DataTable>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetDataTable,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static Dictionary<int, Dictionary<string, object?>> GetRawTable(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, Dictionary<int, Dictionary<string, object?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetRawTable,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?>> GetTuples<T1, T2>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetTuples<T1, T2>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?>> GetTuples<T1, T2, T3>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetTuples<T1, T2, T3>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?, T4?>> GetTuples<T1, T2, T3, T4>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?, T4?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetTuples<T1, T2, T3, T4>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?>> GetTuples<T1, T2, T3, T4, T5>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?, T4?, T5?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetTuples<T1, T2, T3, T4, T5>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>> GetTuples<T1, T2, T3, T4, T5, T6>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetTuples<T1, T2, T3, T4, T5, T6>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>> GetTuples<T1, T2, T3, T4, T5, T6, T7>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetTuples<T1, T2, T3, T4, T5, T6, T7>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetTuples<T1, T2, T3, T4, T5, T6, T7, T8>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        #endregion
        #region ResultRange
        public static TCollection GetResultCollection<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            Action<DbBatch, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            where TCollection : ICollection<T>, new()
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetResultCollection<TCollection, T>(
                    constructorByIndex: constructorByIndex),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static TCollection GetResultCollection<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbBatch, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            where TCollection : ICollection<T>, new()
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetResultCollection<TCollection, T>(
                    constructor: constructor),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static TCollection GetResultCollection<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, T> func,
            Action<DbBatch, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            where TCollection : ICollection<T>, new()
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetResultCollection<TCollection, T>(
                    func: func),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<T> GetResultList<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            Action<DbBatch, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetResultList(
                    constructorByIndex: constructorByIndex),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<T> GetResultList<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbBatch, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetResultList(
                    constructor: constructor),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<T> GetResultList<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, T> func,
            Action<DbBatch, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetResultList(
                    func: func),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static Dictionary<int, T> GetResultDictionary<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null,
            Action<DbBatch, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetResultDictionary(
                    constructorByIndex: constructorByIndex),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static Dictionary<int, T> GetResultDictionary<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbBatch, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetResultDictionary(
                    constructor: constructor),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static Dictionary<int, T> GetResultDictionary<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, T> func,
            Action<DbBatch, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetResultDictionary(
                    func: func),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static Dictionary<int, Dictionary<int, Dictionary<string, object?>>> GetRawDatabase(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, Dictionary<int, Dictionary<int, Dictionary<string, object?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetRawDatabase,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        #endregion
        #region GlobalRange
        public static TCollection GetGlobalCollection<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex = null,
            Action<DbBatch, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            where TCollection : ICollection<T>, new()
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetGlobalCollection<TCollection, T>(
                    constructorByIndex: constructorByIndex),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static TCollection GetGlobalCollection<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbBatch, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            where TCollection : ICollection<T>, new()
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetGlobalCollection<TCollection, T>(
                    constructor: constructor),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<T> GetGlobalList<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex = null,
            Action<DbBatch, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetGlobalList<T>(
                    constructorByIndex: constructorByIndex),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<T> GetGlobalList<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbBatch, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetGlobalList<T>(
                    constructor: constructor),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static Dictionary<int, T> GetGlobalDictionary<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex = null,
            Action<DbBatch, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetGlobalDictionary<T>(
                    constructorByIndex: constructorByIndex),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static Dictionary<int, T> GetGlobalDictionary<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbBatch, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.GetGlobalDictionary<T>(
                    constructor: constructor),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static Dictionary<int, Dictionary<string, object?>> GetGlobalRawTable(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, Dictionary<int, Dictionary<string, object?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetGlobalRawTable,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?>> GetGlobalTuples<T1, T2>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuples<T1, T2>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?>> GetGlobalTuples<T1, T2, T3>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuples<T1, T2, T3>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?, T4?>> GetGlobalTuples<T1, T2, T3, T4>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?, T4?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuples<T1, T2, T3, T4>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?>> GetGlobalTuples<T1, T2, T3, T4, T5>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?, T4?, T5?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuples<T1, T2, T3, T4, T5>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>> GetGlobalTuples<T1, T2, T3, T4, T5, T6>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuples<T1, T2, T3, T4, T5, T6>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>> GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>> GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7, T8>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7, T8>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>> GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>> GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        #endregion
        #endregion
        #region GetEnumerableAsync DbBatch
        #region RangeAsync
        public static async Task<TCollection> GetCollectionAsync<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            Action<DbBatch, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetCollectionAsync<TCollection, T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetCollectionAsync<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            Action<DbBatch, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetCollectionAsync<TCollection, T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetCollectionAsync<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex,
            Action<DbBatch, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetCollectionAsync<TCollection, T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetCollectionAsync<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbBatch, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetCollectionAsync<TCollection, T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetListAsync<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            Action<DbBatch, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetListAsync(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetListAsync<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            Action<DbBatch, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetListAsync(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetListAsync<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex,
            Action<DbBatch, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetListAsync(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetListAsync<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbBatch, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetListAsync(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetDictionaryAsync<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            Action<DbBatch, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetDictionaryAsync(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetDictionaryAsync<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            Action<DbBatch, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetDictionaryAsync(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetDictionaryAsync<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex,
            Action<DbBatch, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetDictionaryAsync(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetDictionaryAsync<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbBatch, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetDictionaryAsync(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<DataTable?> GetSchemaTableAsync(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, DataTable?>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: async (dbDataReader, cancellationToken) => await dbDataReader.GetSchemaTableAsync(
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<DataTable> GetDataTableAsync(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, DataTable>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetDataTableAsync,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, Dictionary<string, object?>>> GetRawTableAsync(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, Dictionary<int, Dictionary<string, object?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetRawTableAsync,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?>>> GetTuplesAsync<T1, T2>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetTuplesAsync<T1, T2>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?>>> GetTuplesAsync<T1, T2, T3>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetTuplesAsync<T1, T2, T3>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?>>> GetTuplesAsync<T1, T2, T3, T4>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?, T4?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetTuplesAsync<T1, T2, T3, T4>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?>>> GetTuplesAsync<T1, T2, T3, T4, T5>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?, T4?, T5?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetTuplesAsync<T1, T2, T3, T4, T5>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetTuplesAsync<T1, T2, T3, T4, T5, T6>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        #endregion
        #region ResultRangeAsync
        public static async Task<TCollection> GetResultCollectionAsync<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            Action<DbBatch, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultCollectionAsync<TCollection, T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetResultCollectionAsync<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            Action<DbBatch, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultCollectionAsync<TCollection, T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetResultCollectionAsync<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<T>> func,
            Action<DbBatch, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultCollectionAsync<TCollection, T>(
                    func: func,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetResultListAsync<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            Action<DbBatch, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultListAsync<T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetResultListAsync<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            Action<DbBatch, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultListAsync<T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetResultListAsync<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<T>> func,
            Action<DbBatch, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultListAsync<T>(
                    func: func,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetResultDictionaryAsync<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            Action<DbBatch, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultDictionaryAsync<T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetResultDictionaryAsync<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            Action<DbBatch, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultDictionaryAsync<T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetResultDictionaryAsync<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<T>> func,
            Action<DbBatch, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetResultDictionaryAsync<T>(
                    func: func,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, Dictionary<int, Dictionary<string, object?>>>> GetRawDatabaseAsync(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, Dictionary<int, Dictionary<int, Dictionary<string, object?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetRawDatabaseAsync,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        #endregion
        #region GlobalRangeAsync
        public static async Task<TCollection> GetGlobalCollectionAsync<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, int, int, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            Action<DbBatch, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalCollectionAsync<TCollection, T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetGlobalCollectionAsync<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            Action<DbBatch, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalCollectionAsync<TCollection, T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetGlobalCollectionAsync<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex,
            Action<DbBatch, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalCollectionAsync<TCollection, T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetGlobalCollectionAsync<TCollection, T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbBatch, TCollection>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalCollectionAsync<TCollection, T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetGlobalListAsync<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, int, int, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            Action<DbBatch, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalListAsync<T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetGlobalListAsync<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            Action<DbBatch, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalListAsync<T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetGlobalListAsync<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex,
            Action<DbBatch, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalListAsync<T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetGlobalListAsync<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbBatch, List<T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalListAsync<T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetGlobalDictionaryAsync<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, int, int, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            Action<DbBatch, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalDictionaryAsync<T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetGlobalDictionaryAsync<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            Action<DbBatch, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalDictionaryAsync<T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetGlobalDictionaryAsync<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex,
            Action<DbBatch, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalDictionaryAsync<T>(
                    constructorByIndex: constructorByIndex,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetGlobalDictionaryAsync<T>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            Action<DbBatch, Dictionary<int, T>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => dbDataReader.GetGlobalDictionaryAsync<T>(
                    constructor: constructor,
                    cancellationToken: cancellationToken),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, Dictionary<string, object?>>> GetGlobalRawTableAsync(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, Dictionary<int, Dictionary<string, object?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetGlobalRawTableAsync,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?>>> GetGlobalTuplesAsync<T1, T2>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuplesAsync<T1, T2>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?>>> GetGlobalTuplesAsync<T1, T2, T3>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuplesAsync<T1, T2, T3>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?>>> GetGlobalTuplesAsync<T1, T2, T3, T4>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?, T4?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuplesAsync<T1, T2, T3, T4>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?>>> GetGlobalTuplesAsync<T1, T2, T3, T4, T5>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?, T4?, T5?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuplesAsync<T1, T2, T3, T4, T5>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>>> GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>> GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>>> GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>>> GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>>> GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this DbConnection dbConnection,
            Action<DbBatch> beforeExecute,
            Action<DbBatch, List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>>>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: GetGlobalTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        #endregion
        #endregion
#endif
        #region Check/CheckAsync
        #region Internal
        private static bool TryFuncForCheckDbConnection(
            this DbConnection dbConnection)
            => true;
        private static bool CatchFuncForCheckDbConnection(
            this DbConnection? dbConnection,
            Exception exception)
            => false;
        private static async Task<bool> TryFuncForCheckDbConnectionAsync(
            this DbConnection dbConnection,
            CancellationToken cancellationToken)
            => await Task.FromResult(true);
        private static async Task<bool> CatchFuncForCheckDbConnectionAsync(
            this DbConnection? dbConnection,
            Exception exception,
            CancellationToken cancellationToken)
            => exception is OperationCanceledException && cancellationToken.IsCancellationRequested
                ? throw exception
                : await Task.FromResult(false);
        #endregion
        public static bool Check(
            this DbConnection dbConnection)
            => dbConnection.Using(
                tryFunc: TryFuncForCheckDbConnection,
                catchFunc: CatchFuncForCheckDbConnection);
        public static async Task<bool> CheckAsync(
            this DbConnection dbConnection,
            CancellationToken cancellationToken = default)
            => await dbConnection.UsingAsync(
                tryFunc: TryFuncForCheckDbConnectionAsync,
                catchFunc: CatchFuncForCheckDbConnectionAsync,
                cancellationToken: cancellationToken);
        #endregion
        #region HasRows/HasRowsAsync
        public static bool HasRows(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, bool>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null)
            => dbConnection.ExecuteReader(
                beforeExecute: beforeExecute,
                constructor: dbDataReader => dbDataReader.HasRows,
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions);
        public static async Task<bool> HasRowsAsync(
            this DbConnection dbConnection,
            Action<DbCommand> beforeExecute,
            Action<DbCommand, bool>? afterExecute = null,
            CommandBehavior? commandBehavior = null,
            IsolationLevel? isolationLevel = null,
            IUsingOptions? usingOptions = null,
            CancellationToken cancellationToken = default)
            => await dbConnection.ExecuteReaderAsync(
                beforeExecute: beforeExecute,
                constructor: (dbDataReader, cancellationToken) => Task.FromResult(dbDataReader.HasRows),
                afterExecute: afterExecute,
                commandBehavior: commandBehavior,
                isolationLevel: isolationLevel,
                usingOptions: usingOptions,
                cancellationToken: cancellationToken);
        #endregion
    }
}
