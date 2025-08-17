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
        #region Sync
        #region TakeOrSkip
        /// <summary>
        /// It is used by default in the "Get[CollectionTypeName]" methods
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbDataReader"></param>
        /// <param name="func"></param>
        /// <param name="fieldIndex"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static ValueTuple<bool, T> TakeFieldValueOrSkipIfDbNullOrOutOfRange<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, T> func,
            int fieldIndex,
            int resultIndex = default,
            int globalIndex = default,
            int index = default)
            => dbDataReader.FieldCount <= fieldIndex && dbDataReader.IsDBNull(fieldIndex) ? ValueTuple.Create(false, default(T)!) : ValueTuple.Create(true, func(dbDataReader, fieldIndex));
        public static ValueTuple<bool, T> TakeFieldValueOrSkipIfDbNullOrOutOfRange<T>(
            this DbDataReader dbDataReader,
            int fieldIndex,
            int resultIndex = default,
            int globalIndex = default,
            int index = default)
            => dbDataReader.TakeFieldValueOrSkipIfDbNullOrOutOfRange(
                func: (dbDataReader, fieldIndex) => dbDataReader.GetFieldValue<T>(fieldIndex),
                fieldIndex: fieldIndex,
                resultIndex: resultIndex,
                globalIndex: globalIndex,
                index: index);
        public static ValueTuple<bool, T> TakeFieldValueOrSkipIfDbNullOrOutOfRange<T>(
            this DbDataReader dbDataReader,
            int fieldIndex,
            int resultIndex = default)
            => dbDataReader.TakeFieldValueOrSkipIfDbNullOrOutOfRange<T>(
                fieldIndex: fieldIndex,
                resultIndex: resultIndex,
                globalIndex: default,
                index: default);
        public static ValueTuple<bool, T> TakeFieldValueOrSkipIfDbNullOrOutOfRange<T>(
            this DbDataReader dbDataReader,
            int fieldIndex)
            => dbDataReader.TakeFieldValueOrSkipIfDbNullOrOutOfRange<T>(
                fieldIndex: fieldIndex,
                resultIndex: default,
                globalIndex: default,
                index: default);
        public static ValueTuple<bool, T> TakeFirstFieldValueOrSkipIfDbNullOrOutOfRange<T>(
            this DbDataReader dbDataReader,
            int resultIndex = default,
            int globalIndex = default,
            int index = default)
            => dbDataReader.TakeFieldValueOrSkipIfDbNullOrOutOfRange<T>(
                fieldIndex: default,
                resultIndex: default,
                globalIndex: default,
                index: default);
        public static ValueTuple<bool, T> TakeFirstFieldValueOrSkipIfDbNullOrOutOfRange<T>(
            this DbDataReader dbDataReader,
            int resultIndex = default)
            => dbDataReader.TakeFieldValueOrSkipIfDbNullOrOutOfRange<T>(
                fieldIndex: default,
                resultIndex: default,
                globalIndex: default,
                index: default);
        public static ValueTuple<bool, T> TakeFirstFieldValueOrSkipIfDbNullOrOutOfRange<T>(
            this DbDataReader dbDataReader)
            => dbDataReader.TakeFieldValueOrSkipIfDbNullOrOutOfRange<T>(
                fieldIndex: default,
                resultIndex: default,
                globalIndex: default,
                index: default);
        #endregion
        #region TakeOrDefault
        public static ValueTuple<bool, T?> TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, T> func,
            int fieldIndex,
            int resultIndex = default,
            int globalIndex = default,
            int index = default)
            => dbDataReader.FieldCount <= fieldIndex && dbDataReader.IsDBNull(fieldIndex) ? ValueTuple.Create(true, default(T)) : ValueTuple.Create<bool, T?>(true, func(dbDataReader, fieldIndex));
        public static ValueTuple<bool, T?> TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T>(
            this DbDataReader dbDataReader,
            int fieldIndex,
            int resultIndex = default,
            int globalIndex = default,
            int index = default)
            => dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange(
                func: (dbDataReader, fieldIndex) => dbDataReader.GetFieldValue<T?>(fieldIndex),
                fieldIndex: fieldIndex,
                resultIndex: resultIndex,
                globalIndex: globalIndex,
                index: index);
        public static ValueTuple<bool, T?> TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T>(
            this DbDataReader dbDataReader,
            int fieldIndex,
            int resultIndex = default)
            => dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T>(
                fieldIndex: fieldIndex,
                resultIndex: resultIndex,
                globalIndex: default,
                index: default);
        public static ValueTuple<bool, T?> TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T>(
            this DbDataReader dbDataReader,
            int fieldIndex)
            => dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T>(
                fieldIndex: fieldIndex,
                resultIndex: default,
                globalIndex: default,
                index: default);
        public static ValueTuple<bool, T?> TakeFirstFieldValueOrDefaultIfDbNullOrOutOfRange<T>(
            this DbDataReader dbDataReader,
            int resultIndex = default,
            int globalIndex = default,
            int index = default)
            => dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T>(
                fieldIndex: default,
                resultIndex: default,
                globalIndex: default,
                index: default);
        public static ValueTuple<bool, T?> TakeFirstFieldValueOrDefaultIfDbNullOrOutOfRange<T>(
            this DbDataReader dbDataReader,
            int resultIndex = default)
            => dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T>(
                fieldIndex: default,
                resultIndex: default,
                globalIndex: default,
                index: default);
        public static ValueTuple<bool, T?> TakeFirstFieldValueOrDefaultIfDbNullOrOutOfRange<T>(
            this DbDataReader dbDataReader)
            => dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T>(
                fieldIndex: default,
                resultIndex: default,
                globalIndex: default,
                index: default);
        #endregion
        #region TakeOrThrow
        public static ValueTuple<bool, T> TakeFieldValueOrThrowIfDbNullOrOutOfRange<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, T> func,
            int fieldIndex,
            int resultIndex = default,
            int globalIndex = default,
            int index = default)
            => dbDataReader.FieldCount <= fieldIndex && dbDataReader.IsDBNull(fieldIndex) ? throw new DataException() : ValueTuple.Create(true, func(dbDataReader, fieldIndex));
        public static ValueTuple<bool, T> TakeFieldValueOrThrowIfDbNullOrOutOfRange<T>(
            this DbDataReader dbDataReader,
            int fieldIndex,
            int resultIndex = default,
            int globalIndex = default,
            int index = default)
            => dbDataReader.TakeFieldValueOrThrowIfDbNullOrOutOfRange(
                func: (dbDataReader, fieldIndex) => dbDataReader.GetFieldValue<T>(fieldIndex),
                fieldIndex: fieldIndex,
                resultIndex: resultIndex,
                globalIndex: globalIndex,
                index: index);
        public static ValueTuple<bool, T> TakeFieldValueOrThrowIfDbNullOrOutOfRange<T>(
            this DbDataReader dbDataReader,
            int fieldIndex,
            int resultIndex = default)
            => dbDataReader.TakeFieldValueOrThrowIfDbNullOrOutOfRange<T>(
                fieldIndex: fieldIndex,
                resultIndex: resultIndex,
                globalIndex: default,
                index: default);
        public static ValueTuple<bool, T> TakeFieldValueOrThrowIfDbNullOrOutOfRange<T>(
            this DbDataReader dbDataReader,
            int fieldIndex)
            => dbDataReader.TakeFieldValueOrThrowIfDbNullOrOutOfRange<T>(
                fieldIndex: fieldIndex,
                resultIndex: default,
                globalIndex: default,
                index: default);
        public static ValueTuple<bool, T> TakeFirstFieldValueOrThrowIfDbNullOrOutOfRange<T>(
            this DbDataReader dbDataReader,
            int resultIndex = default,
            int globalIndex = default,
            int index = default)
            => dbDataReader.TakeFieldValueOrThrowIfDbNullOrOutOfRange<T>(
                fieldIndex: default,
                resultIndex: default,
                globalIndex: default,
                index: default);
        public static ValueTuple<bool, T> TakeFirstFieldValueOrThrowIfDbNullOrOutOfRange<T>(
            this DbDataReader dbDataReader,
            int resultIndex = default)
            => dbDataReader.TakeFieldValueOrThrowIfDbNullOrOutOfRange<T>(
                fieldIndex: default,
                resultIndex: default,
                globalIndex: default,
                index: default);
        public static ValueTuple<bool, T> TakeFirstFieldValueOrThrowIfDbNullOrOutOfRange<T>(
            this DbDataReader dbDataReader)
            => dbDataReader.TakeFieldValueOrThrowIfDbNullOrOutOfRange<T>(
                fieldIndex: default,
                resultIndex: default,
                globalIndex: default,
                index: default);
        #endregion
        public static Dictionary<string, object?> GetRawRow(
            this DbDataReader dbDataReader)
        {
            var row = new Dictionary<string, object?>();
            for (var fieldIndex = 0; fieldIndex < dbDataReader.FieldCount; fieldIndex++)
                row[dbDataReader.GetName(fieldIndex)] = dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<object>(fieldIndex).Item2;
            return row;
        }
        private static ValueTuple<bool, T> InvokeConstructor<T>(
            this DbDataReader dbDataReader,
            Delegate? constructorByIndex,
            int resultIndex,
            int globalIndex,
            int index,
            bool allowAggregateResult)
            => constructorByIndex switch
            {
                Func<DbDataReader, int, ValueTuple<bool, T>> func3 => allowAggregateResult
                    ? func3(dbDataReader, resultIndex)
                    : func3(dbDataReader, index),
                Func<DbDataReader, int, int, int, ValueTuple<bool, T>> func5 when !allowAggregateResult =>
                    func5(dbDataReader, resultIndex, globalIndex, index),
                null => dbDataReader.TakeFirstFieldValueOrSkipIfDbNullOrOutOfRange<T>(),
                _ => throw new InvalidCastException(nameof(constructorByIndex)),
            };
        private static TCollection InternalAddResultRange<TCollection, T>(
            this DbDataReader dbDataReader,
            TCollection collection,
            Delegate? constructorByIndex,
            bool allowNextResult,
            bool allowAggregateResult)
            where TCollection : ICollection<T>
        {
            var resultIndex = 0;
            var globalIndex = 0;
            var index = globalIndex;
            do
            {
                if (allowAggregateResult)
                {
                    if (constructorByIndex is null)
                        dbDataReader.Read();
                    var result = dbDataReader.InvokeConstructor<T>(constructorByIndex, resultIndex, globalIndex++, index, allowAggregateResult);
                    if (result.Item1)
                        collection.Add(result.Item2);
                }
                else
                {
                    index = 0;
                    while (dbDataReader.Read())
                    {
                        var result = dbDataReader.InvokeConstructor<T>(constructorByIndex, resultIndex, globalIndex++, index++, allowAggregateResult);
                        if (result.Item1)
                            collection.Add(result.Item2);
                    }
                }
                resultIndex++;
            }
            while (allowNextResult && dbDataReader.NextResult());
            return collection;
        }
        private static ValueTuple<bool, KeyValuePair<int, T>> GetKeyValuePairValueTuple<T>(
            this DbDataReader dbDataReader,
            Delegate? constructorByIndex,
            int resultIndex,
            int globalIndex,
            int index,
            bool allowAggregateResult)
        {
            var result = dbDataReader.InvokeConstructor<T>(constructorByIndex, resultIndex, globalIndex, index, allowAggregateResult);
            return ValueTuple.Create(result.Item1, KeyValuePair.Create(allowAggregateResult ? resultIndex : index, result.Item2));
        }
        #region Range
        public static TCollection AddRange<TCollection, T>(
            this DbDataReader dbDataReader,
            TCollection collection,
            Func<DbDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex = null,
            bool allowNextResult = false)
            where TCollection : ICollection<T>
            => dbDataReader.InternalAddResultRange<TCollection, T>(
                collection: collection,
                constructorByIndex: constructorByIndex,
                allowNextResult: allowNextResult,
                allowAggregateResult: false);
        public static TCollection AddRange<TCollection, T>(
            this DbDataReader dbDataReader,
            TCollection collection,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            bool allowNextResult = false)
            where TCollection : ICollection<T>
            => dbDataReader.AddRange<TCollection, T>(
                collection: collection,
                constructorByIndex: constructor is null ? null : (x, _, _, _) => constructor(x),
                allowNextResult: allowNextResult);
        public static TCollection GetCollection<TCollection, T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex = null,
            bool allowNextResult = false)
            where TCollection : ICollection<T>, new()
            => dbDataReader.AddRange(
                collection: new TCollection(),
                constructorByIndex: constructorByIndex,
                allowNextResult: allowNextResult);
        public static TCollection GetCollection<TCollection, T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            bool allowNextResult = false)
            where TCollection : ICollection<T>, new()
            => dbDataReader.AddRange(
                collection: new TCollection(),
                constructor: constructor,
                allowNextResult: allowNextResult);
        public static List<T> GetList<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex = null,
            bool allowNextResult = false)
            => dbDataReader.GetCollection<List<T>, T>(
                constructorByIndex: constructorByIndex,
                allowNextResult: allowNextResult);
        public static List<T> GetList<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            bool allowNextResult = false)
            => dbDataReader.GetCollection<List<T>, T>(
                constructor: constructor,
                allowNextResult: allowNextResult);
        public static Dictionary<int, T> GetDictionary<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex = null,
            bool allowNextResult = false)
            => dbDataReader.GetCollection<Dictionary<int, T>, KeyValuePair<int, T>>(
                constructorByIndex: (dbDataReader, resultIndex, globalIndex, index) => dbDataReader.GetKeyValuePairValueTuple<T>(
                    constructorByIndex: constructorByIndex,
                    resultIndex: resultIndex,
                    globalIndex: globalIndex,
                    index: index,
                    allowAggregateResult: false),
                allowNextResult: allowNextResult);
        public static Dictionary<int, T> GetDictionary<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            bool allowNextResult = false)
            => dbDataReader.GetDictionary<T>(
                constructorByIndex: constructor is null ? null : (x, _, _, _) => constructor(x),
                allowNextResult: allowNextResult);
        public static DataTable GetDataTable(
            this DbDataReader dbDataReader)
        {
            var dataTable = new DataTable();
            while (dbDataReader.Read())
            {
                var dataRow = dataTable.NewRow();
                for (var fieldIndex = 0; fieldIndex < dbDataReader.FieldCount; fieldIndex++)
                {
                    if (dataTable.Columns.Count == fieldIndex)
                        dataTable.Columns.Add(dbDataReader.GetName(fieldIndex), dbDataReader.GetFieldType(fieldIndex));
                    if (!dbDataReader.IsDBNull(fieldIndex))
                        dataRow[fieldIndex] = dbDataReader.GetFieldValue<object>(fieldIndex);
                }
                dataTable.Rows.Add(dataRow);
            }
            return dataTable;
        }
        public static Dictionary<int, Dictionary<string, object?>> GetRawTable(
            this DbDataReader dbDataReader)
            => dbDataReader.GetDictionary(
                constructor: dbDataReader => ValueTuple.Create(true, dbDataReader.GetRawRow()));
        public static List<Tuple<T1?, T2?>> GetTuples<T1, T2>(
            this DbDataReader dbDataReader)
            => dbDataReader.GetList(
                constructor: dbDataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T1>(0).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T2>(1).Item2)));
        public static List<Tuple<T1?, T2?, T3?>> GetTuples<T1, T2, T3>(
            this DbDataReader dbDataReader)
            => dbDataReader.GetList(
                constructor: dbDataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T1>(0).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T2>(1).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T3>(2).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?>> GetTuples<T1, T2, T3, T4>(
            this DbDataReader dbDataReader)
            => dbDataReader.GetList(
                constructor: dbDataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T1>(0).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T2>(1).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T3>(2).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T4>(3).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?>> GetTuples<T1, T2, T3, T4, T5>(
            this DbDataReader dbDataReader)
            => dbDataReader.GetList(
                constructor: dbDataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T1>(0).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T2>(1).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T3>(2).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T4>(3).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T5>(4).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>> GetTuples<T1, T2, T3, T4, T5, T6>(
            this DbDataReader dbDataReader)
            => dbDataReader.GetList(
                constructor: dbDataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T1>(0).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T2>(1).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T3>(2).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T4>(3).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T5>(4).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T6>(5).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>> GetTuples<T1, T2, T3, T4, T5, T6, T7>(
            this DbDataReader dbDataReader)
            => dbDataReader.GetList(
                constructor: dbDataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T1>(0).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T2>(1).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T3>(2).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T4>(3).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T5>(4).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T6>(5).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T7>(6).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8>(
            this DbDataReader dbDataReader)
            => dbDataReader.GetList(
                constructor: dbDataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T1>(0).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T2>(1).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T3>(2).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T4>(3).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T5>(4).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T6>(5).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T7>(6).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T8>(7).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this DbDataReader dbDataReader)
            => dbDataReader.GetList(
                constructor: dbDataReader => ValueTuple.Create(
                    true,
                    new Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>(
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T1>(0).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T2>(1).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T3>(2).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T4>(3).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T5>(4).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T6>(5).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T7>(6).Item2,
                        Tuple.Create(
                            dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T8>(7).Item2,
                            dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T9>(8).Item2))));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this DbDataReader dbDataReader)
            => dbDataReader.GetList(
                constructor: dbDataReader => ValueTuple.Create(
                    true,
                    new Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>(
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T1>(0).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T2>(1).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T3>(2).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T4>(3).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T5>(4).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T6>(5).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T7>(6).Item2,
                        Tuple.Create(
                            dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T8>(7).Item2,
                            dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T9>(8).Item2,
                            dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T10>(10).Item2))));
        #endregion
        #region ResultRange
        public static TCollection AddResultRange<TCollection, T>(
            this DbDataReader dbDataReader,
            TCollection collection,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null)
            where TCollection : ICollection<T>
            => dbDataReader.InternalAddResultRange<TCollection, T>(
                collection: collection,
                constructorByIndex: constructorByIndex,
                allowNextResult: true,
                allowAggregateResult: true);
        public static TCollection AddResultRange<TCollection, T>(
            this DbDataReader dbDataReader,
            TCollection collection,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor)
            where TCollection : ICollection<T>
            => dbDataReader.AddResultRange<TCollection, T>(
                collection: collection,
                constructorByIndex: constructor is null ? null : (dbDataReader, _) => constructor(dbDataReader));
        public static TCollection AddResultRange<TCollection, T>(
            this DbDataReader dbDataReader,
            TCollection collection,
            Func<DbDataReader, T> func)
            where TCollection : ICollection<T>, new()
            => dbDataReader.AddResultRange(
                collection,
                constructorByIndex: (dbDataReader, _) => ValueTuple.Create(true, func(dbDataReader)));
        public static TCollection GetResultCollection<TCollection, T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null)
            where TCollection : ICollection<T>, new()
            => dbDataReader.AddResultRange(
                collection: new TCollection(),
                constructorByIndex: constructorByIndex);
        public static TCollection GetResultCollection<TCollection, T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor)
            where TCollection : ICollection<T>, new()
            => dbDataReader.AddRange(
                collection: new TCollection(),
                constructor: constructor);
        public static TCollection GetResultCollection<TCollection, T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, T> func)
            where TCollection : ICollection<T>, new()
            => dbDataReader.AddResultRange(
                collection: new TCollection(),
                func: func);
        public static List<T> GetResultList<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null)
            => dbDataReader.GetResultCollection<List<T>, T>(
                constructorByIndex: constructorByIndex);
        public static List<T> GetResultList<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor)
            => dbDataReader.GetResultCollection<List<T>, T>(
                constructor: constructor);
        public static List<T> GetResultList<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, T> func)
            => dbDataReader.GetResultCollection<List<T>, T>(
                func: func);
        public static Dictionary<int, T> GetResultDictionary<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null)
            => dbDataReader.GetResultCollection<Dictionary<int, T>, KeyValuePair<int, T>>(
                constructorByIndex: (dbDataReader, resultIndex) => dbDataReader.GetKeyValuePairValueTuple<T>(
                    constructorByIndex: constructorByIndex,
                    resultIndex: resultIndex,
                    globalIndex: 0,
                    index: 0,
                    allowAggregateResult: true));
        public static Dictionary<int, T> GetResultDictionary<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor)
            => dbDataReader.GetResultDictionary<T>(
                constructorByIndex: constructor is null ? null : (dbDataReader, _) => constructor(dbDataReader));
        public static Dictionary<int, T> GetResultDictionary<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, T> func)
            => dbDataReader.GetResultDictionary<T>(
                constructorByIndex: (dbDataReader, _) => ValueTuple.Create(true, func(dbDataReader)));
        public static Dictionary<int, Dictionary<int, Dictionary<string, object?>>> GetRawDatabase(
            this DbDataReader dbDataReader)
            => GetResultDictionary(
                dbDataReader: dbDataReader,
                func: GetRawTable);
        #endregion
        #endregion
        #region Async
        #region TakeOrSkipAsync
        /// <summary>
        /// It is used by default in the "Get[CollectionTypeName]Async" methods
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dbDataReader"></param>
        /// <param name="func"></param>
        /// <param name="fieldIndex"></param>
        /// <param name="index"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<ValueTuple<bool, T>> TakeFieldValueOrSkipIfDbNullOrOutOfRangeAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, CancellationToken, Task<T>> func,
            int fieldIndex,
            int resultIndex = default,
            int globalIndex = default,
            int index = default,
            CancellationToken cancellationToken = default)
            => dbDataReader.FieldCount <= fieldIndex && await dbDataReader.IsDBNullAsync(fieldIndex, cancellationToken) ? ValueTuple.Create(false, default(T)!) : ValueTuple.Create(true, await func(dbDataReader, fieldIndex, cancellationToken));
        public static async Task<ValueTuple<bool, T>> TakeFieldValueOrSkipIfDbNullOrOutOfRangeAsync<T>(
            this DbDataReader dbDataReader,
            int fieldIndex,
            int resultIndex = default,
            int globalIndex = default,
            int index = default,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFieldValueOrSkipIfDbNullOrOutOfRangeAsync(
                func: async (dbDataReader, fieldIndex, cancellationToken) => await dbDataReader.GetFieldValueAsync<T>(fieldIndex, cancellationToken),
                fieldIndex: fieldIndex,
                resultIndex: resultIndex,
                globalIndex: globalIndex,
                index: index,
                cancellationToken: cancellationToken);
        public static async Task<ValueTuple<bool, T>> TakeFieldValueOrSkipIfDbNullOrOutOfRangeAsync<T>(
            this DbDataReader dbDataReader,
            int fieldIndex,
            int resultIndex = default,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFieldValueOrSkipIfDbNullOrOutOfRangeAsync<T>(
                fieldIndex: fieldIndex,
                resultIndex: resultIndex,
                globalIndex: default,
                index: default,
                cancellationToken: cancellationToken);
        public static async Task<ValueTuple<bool, T>> TakeFieldValueOrSkipIfDbNullOrOutOfRangeAsync<T>(
            this DbDataReader dbDataReader,
            int fieldIndex,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFieldValueOrSkipIfDbNullOrOutOfRangeAsync<T>(
                fieldIndex: fieldIndex,
                resultIndex: default,
                globalIndex: default,
                index: default,
                cancellationToken: cancellationToken);
        public static async Task<ValueTuple<bool, T>> TakeFirstFieldValueOrSkipIfDbNullOrOutOfRangeAsync<T>(
            this DbDataReader dbDataReader,
            int resultIndex = default,
            int globalIndex = default,
            int index = default,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFieldValueOrSkipIfDbNullOrOutOfRangeAsync<T>(
                fieldIndex: default,
                resultIndex: default,
                globalIndex: default,
                index: default,
                cancellationToken: cancellationToken);
        public static async Task<ValueTuple<bool, T>> TakeFirstFieldValueOrSkipIfDbNullOrOutOfRangeAsync<T>(
            this DbDataReader dbDataReader,
            int resultIndex = default,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFieldValueOrSkipIfDbNullOrOutOfRangeAsync<T>(
                fieldIndex: default,
                resultIndex: default,
                globalIndex: default,
                index: default,
                cancellationToken: cancellationToken);
        public static async Task<ValueTuple<bool, T>> TakeFirstFieldValueOrSkipIfDbNullOrOutOfRangeAsync<T>(
            this DbDataReader dbDataReader,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFieldValueOrSkipIfDbNullOrOutOfRangeAsync<T>(
                fieldIndex: default,
                resultIndex: default,
                globalIndex: default,
                index: default,
                cancellationToken: cancellationToken);
        #endregion
        #region TakeOrDefaultAsync
        public static async Task<ValueTuple<bool, T?>> TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, CancellationToken, Task<T>> func,
            int fieldIndex,
            int resultIndex = default,
            int globalIndex = default,
            int index = default,
            CancellationToken cancellationToken = default)
            => dbDataReader.FieldCount <= fieldIndex && await dbDataReader.IsDBNullAsync(fieldIndex, cancellationToken) ? ValueTuple.Create(true, default(T)) : ValueTuple.Create<bool, T?>(true, await func(dbDataReader, fieldIndex, cancellationToken));
        public static async Task<ValueTuple<bool, T?>> TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T>(
            this DbDataReader dbDataReader,
            int fieldIndex,
            int resultIndex = default,
            int globalIndex = default,
            int index = default,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync(
                func: (dbDataReader, fieldIndex, cancellationToken) => dbDataReader.GetFieldValueAsync<T>(fieldIndex, cancellationToken),
                fieldIndex: fieldIndex,
                resultIndex: resultIndex,
                globalIndex: globalIndex,
                index: index,
                cancellationToken: cancellationToken);
        public static async Task<ValueTuple<bool, T?>> TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T>(
            this DbDataReader dbDataReader,
            int fieldIndex,
            int resultIndex = default,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T>(
                fieldIndex: fieldIndex,
                resultIndex: resultIndex,
                globalIndex: default,
                index: default,
                cancellationToken: cancellationToken);
        public static async Task<ValueTuple<bool, T?>> TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T>(
            this DbDataReader dbDataReader,
            int fieldIndex,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T>(
                fieldIndex: fieldIndex,
                resultIndex: default,
                globalIndex: default,
                index: default,
                cancellationToken: cancellationToken);
        public static async Task<ValueTuple<bool, T?>> TakeFirstFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T>(
            this DbDataReader dbDataReader,
            int resultIndex = default,
            int globalIndex = default,
            int index = default,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T>(
                fieldIndex: default,
                resultIndex: default,
                globalIndex: default,
                index: default,
                cancellationToken: cancellationToken);
        public static async Task<ValueTuple<bool, T?>> TakeFirstFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T>(
            this DbDataReader dbDataReader,
            int resultIndex = default,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T>(
                fieldIndex: default,
                resultIndex: default,
                globalIndex: default,
                index: default,
                cancellationToken: cancellationToken);
        public static async Task<ValueTuple<bool, T?>> TakeFirstFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T>(
            this DbDataReader dbDataReader,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T>(
                fieldIndex: default,
                resultIndex: default,
                globalIndex: default,
                index: default,
                cancellationToken: cancellationToken);
        #endregion
        #region TakeOrThrowAsync
        public static async Task<ValueTuple<bool, T>> TakeFieldValueOrThrowIfDbNullOrOutOfRangeAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, CancellationToken, Task<T>> func,
            int fieldIndex,
            int resultIndex = default,
            int globalIndex = default,
            int index = default,
            CancellationToken cancellationToken = default)
            => dbDataReader.FieldCount <= fieldIndex && await dbDataReader.IsDBNullAsync(fieldIndex, cancellationToken) ? throw new DataException() : ValueTuple.Create(true, await func(dbDataReader, fieldIndex, cancellationToken));
        public static async Task<ValueTuple<bool, T>> TakeFieldValueOrThrowIfDbNullOrOutOfRangeAsync<T>(
            this DbDataReader dbDataReader,
            int fieldIndex,
            int resultIndex = default,
            int globalIndex = default,
            int index = default,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFieldValueOrThrowIfDbNullOrOutOfRangeAsync(
                func: (dbDataReader, fieldIndex, cancellationToken) => dbDataReader.GetFieldValueAsync<T>(fieldIndex, cancellationToken),
                fieldIndex: fieldIndex,
                resultIndex: resultIndex,
                globalIndex: globalIndex,
                index: index,
                cancellationToken: cancellationToken);
        public static async Task<ValueTuple<bool, T>> TakeFieldValueOrThrowIfDbNullOrOutOfRangeAsync<T>(
            this DbDataReader dbDataReader,
            int fieldIndex,
            int resultIndex = default,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFieldValueOrThrowIfDbNullOrOutOfRangeAsync<T>(
                fieldIndex: fieldIndex,
                resultIndex: resultIndex,
                globalIndex: default,
                index: default,
                cancellationToken: cancellationToken);
        public static async Task<ValueTuple<bool, T>> TakeFieldValueOrThrowIfDbNullOrOutOfRangeAsync<T>(
            this DbDataReader dbDataReader,
            int fieldIndex,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFieldValueOrThrowIfDbNullOrOutOfRangeAsync<T>(
                fieldIndex: fieldIndex,
                resultIndex: default,
                globalIndex: default,
                index: default,
                cancellationToken: cancellationToken);
        public static async Task<ValueTuple<bool, T>> TakeFirstFieldValueOrThrowIfDbNullOrOutOfRangeAsync<T>(
            this DbDataReader dbDataReader,
            int resultIndex = default,
            int globalIndex = default,
            int index = default,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFieldValueOrThrowIfDbNullOrOutOfRangeAsync<T>(
                fieldIndex: default,
                resultIndex: default,
                globalIndex: default,
                index: default,
                cancellationToken: cancellationToken);
        public static async Task<ValueTuple<bool, T>> TakeFirstFieldValueOrThrowIfDbNullOrOutOfRangeAsync<T>(
            this DbDataReader dbDataReader,
            int resultIndex = default,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFieldValueOrThrowIfDbNullOrOutOfRangeAsync<T>(
                fieldIndex: default,
                resultIndex: default,
                globalIndex: default,
                index: default,
                cancellationToken: cancellationToken);
        public static async Task<ValueTuple<bool, T>> TakeFirstFieldValueOrThrowIfDbNullOrOutOfRangeAsync<T>(
            this DbDataReader dbDataReader,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFieldValueOrThrowIfDbNullOrOutOfRangeAsync<T>(
                fieldIndex: default,
                resultIndex: default,
                globalIndex: default,
                index: default,
                cancellationToken: cancellationToken);
        #endregion
        public static async Task<Dictionary<string, object?>> GetRawRowAsync(
            this DbDataReader dbDataReader,
            CancellationToken cancellationToken = default)
        {
            var row = new Dictionary<string, object?>();
            for (var fieldIndex = 0; fieldIndex < dbDataReader.FieldCount; fieldIndex++)
                row[dbDataReader.GetName(fieldIndex)] = (await TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<object>(dbDataReader, fieldIndex, cancellationToken)).Item2;
            return row;
        }
        private static async Task<ValueTuple<bool, T>> InvokeConstructorAsync<T>(
            this DbDataReader dbDataReader,
            Delegate? constructorByIndex,
            int resultIndex,
            int globalIndex,
            int index,
            bool allowAggregateResult,
            CancellationToken cancellationToken)
            => constructorByIndex switch
            {
                Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>> func4 => allowAggregateResult
                    ? await func4(dbDataReader, resultIndex, cancellationToken)
                    : await func4(dbDataReader, index, cancellationToken),
                Func<DbDataReader, int, int, int, CancellationToken, Task<ValueTuple<bool, T>>> func6 =>
                    await func6(dbDataReader, resultIndex, globalIndex, index, cancellationToken),
                null => await dbDataReader.TakeFirstFieldValueOrSkipIfDbNullOrOutOfRangeAsync<T>(cancellationToken),
                _ => throw new InvalidCastException(nameof(constructorByIndex)),
            };
        private static async Task<TCollection> InternalAddResultRangeAsync<TCollection, T>(
            this DbDataReader dbDataReader,
            TCollection collection,
            Delegate? constructorByIndex,
            bool allowNextResult,
            bool allowAggregateResult,
            CancellationToken cancellationToken)
            where TCollection : ICollection<T>
        {
            var resultIndex = 0;
            var globalIndex = 0;
            var index = globalIndex;
            do
            {
                if (allowAggregateResult)
                {
                    if (constructorByIndex is null)
                        await dbDataReader.ReadAsync(cancellationToken);
                    var result = await dbDataReader.InvokeConstructorAsync<T>(constructorByIndex, resultIndex, globalIndex++, index, allowAggregateResult, cancellationToken);
                    if (result.Item1)
                        collection.Add(result.Item2);
                }
                else
                {
                    index = 0;
                    while (await dbDataReader.ReadAsync(cancellationToken))
                    {
                        var result = await dbDataReader.InvokeConstructorAsync<T>(constructorByIndex, resultIndex, globalIndex++, index++, allowAggregateResult, cancellationToken);
                        if (result.Item1)
                            collection.Add(result.Item2);
                    }
                }
                resultIndex++;
            }
            while (allowNextResult && await dbDataReader.NextResultAsync(cancellationToken));
            return collection;
        }
        private static async Task<ValueTuple<bool, KeyValuePair<int, T>>> GetKeyValuePairValueTupleAsync<T>(
            this DbDataReader dbDataReader,
            Delegate? constructorByIndex,
            int resultIndex,
            int globalIndex,
            int index,
            bool allowAggregateResult,
            CancellationToken cancellationToken)
        {
            var result = await dbDataReader.InvokeConstructorAsync<T>(constructorByIndex, resultIndex, globalIndex, index, allowAggregateResult, cancellationToken);
            return ValueTuple.Create(result.Item1, KeyValuePair.Create(allowAggregateResult ? resultIndex : index, result.Item2));
        }
        #region RangeAsync
        public static async Task<TCollection> AddRangeAsync<TCollection, T>(
            this DbDataReader dbDataReader,
            TCollection collection,
            Func<DbDataReader, int, int, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            bool allowNextResult = false,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>
            => await dbDataReader.InternalAddResultRangeAsync<TCollection, T>(
                collection: collection,
                constructorByIndex: constructorByIndex,
                allowNextResult: allowNextResult,
                allowAggregateResult: false,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> AddRangeAsync<TCollection, T>(
            this DbDataReader dbDataReader,
            TCollection collection,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            bool allowNextResult = false,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>
            => await dbDataReader.AddRangeAsync<TCollection, T>(
                collection: collection,
                constructorByIndex: constructor is null ? null : async (dbDataReader, _, _, _, cancellationToken) => await constructor(dbDataReader, cancellationToken),
                allowNextResult: allowNextResult,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> AddRangeAsync<TCollection, T>(
            this DbDataReader dbDataReader,
            TCollection collection,
            Func<DbDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex,
            bool allowNextResult = false,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>
            => await dbDataReader.AddRangeAsync<TCollection, T>(
                collection: collection,
                constructorByIndex: constructorByIndex is null ? null : (dbDataReader, resultIndex, globalIndex, index, _) => Task.FromResult(constructorByIndex(dbDataReader, resultIndex, globalIndex, index)),
                allowNextResult: allowNextResult,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> AddRangeAsync<TCollection, T>(
            this DbDataReader dbDataReader,
            TCollection collection,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            bool allowNextResult = false,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>
            => await dbDataReader.AddRangeAsync<TCollection, T>(
                collection: collection,
                constructorByIndex: constructor is null ? null : (dbDataReader, _, _, _) => constructor(dbDataReader),
                allowNextResult: allowNextResult,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetCollectionAsync<TCollection, T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, int, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            bool allowNextResult = false,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbDataReader.AddRangeAsync(
                collection: new TCollection(),
                constructorByIndex: constructorByIndex,
                allowNextResult: allowNextResult,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetCollectionAsync<TCollection, T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            bool allowNextResult = false,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbDataReader.AddRangeAsync(
                collection: new TCollection(),
                constructor: constructor,
                allowNextResult: allowNextResult,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetCollectionAsync<TCollection, T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex,
            bool allowNextResult = false,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbDataReader.AddRangeAsync(
                collection: new TCollection(),
                constructorByIndex: constructorByIndex,
                allowNextResult: allowNextResult,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetCollectionAsync<TCollection, T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            bool allowNextResult = false,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbDataReader.AddRangeAsync(
                collection: new TCollection(),
                constructor: constructor,
                allowNextResult: allowNextResult,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetListAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, int, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            bool allowNextResult = false,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetCollectionAsync<List<T>, T>(
                constructorByIndex: constructorByIndex,
                allowNextResult: allowNextResult,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetListAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            bool allowNextResult = false,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetCollectionAsync<List<T>, T>(
                constructor: constructor,
                allowNextResult: allowNextResult,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetListAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex,
            bool allowNextResult = false,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetCollectionAsync<List<T>, T>(
                constructorByIndex: constructorByIndex,
                allowNextResult: allowNextResult,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetListAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            bool allowNextResult = false,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetCollectionAsync<List<T>, T>(
                constructor: constructor,
                allowNextResult: allowNextResult,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetDictionaryAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, int, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            bool allowNextResult = false,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetCollectionAsync<Dictionary<int, T>, KeyValuePair<int, T>>(
                constructorByIndex: async (dbDataReader, resultIndex, globalIndex, index, cancellationToken) => await dbDataReader.GetKeyValuePairValueTupleAsync<T>(
                    constructorByIndex: constructorByIndex,
                    resultIndex: resultIndex,
                    globalIndex: globalIndex,
                    index: index,
                    allowAggregateResult: false,
                    cancellationToken: cancellationToken),
                allowNextResult: allowNextResult,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetDictionaryAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            bool allowNextResult = false,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetDictionaryAsync<T>(
                constructorByIndex: constructor is null ? null : (dbDataReader, _, _, _, cancellationToken) => constructor(dbDataReader, cancellationToken),
                allowNextResult: allowNextResult,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetDictionaryAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex,
            bool allowNextResult = false,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetDictionaryAsync<T>(
                constructorByIndex: constructorByIndex is null ? null : (dbDataReader, resultIndex, globalIndex, index, _) => Task.FromResult(constructorByIndex(dbDataReader, resultIndex, globalIndex, index)),
                allowNextResult: allowNextResult,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetDictionaryAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            bool allowNextResult = false,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetDictionaryAsync<T>(
                constructorByIndex: constructor is null ? null : (dbDataReader, _, _, _) => constructor(dbDataReader),
                allowNextResult: allowNextResult,
                cancellationToken: cancellationToken);
        public static async Task<DataTable> GetDataTableAsync(
            this DbDataReader dbDataReader,
            CancellationToken cancellationToken = default)
        {
            var dataTable = new DataTable();
            while (await dbDataReader.ReadAsync(cancellationToken))
            {
                var dataRow = dataTable.NewRow();
                for (var fieldIndex = 0; fieldIndex < dbDataReader.FieldCount; fieldIndex++)
                {
                    if (dataTable.Columns.Count == fieldIndex)
                        dataTable.Columns.Add(dbDataReader.GetName(fieldIndex), dbDataReader.GetFieldType(fieldIndex));
                    if (!await dbDataReader.IsDBNullAsync(fieldIndex, cancellationToken))
                        dataRow[fieldIndex] = await dbDataReader.GetFieldValueAsync<object>(fieldIndex, cancellationToken);
                }
                dataTable.Rows.Add(dataRow);
            }
            return dataTable;
        }
        public static async Task<Dictionary<int, Dictionary<string, object?>>> GetRawTableAsync(
            this DbDataReader dbDataReader,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetDictionaryAsync(
                constructorByIndex: async (dbDataReader, resultIndex, globalIndex, index, cancellationToken) => ValueTuple.Create(true, await dbDataReader.GetRawRowAsync(cancellationToken)),
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?>>> GetTuplesAsync<T1, T2>(
            this DbDataReader dbDataReader,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetListAsync(
                constructor: async (dbDataReader, cancellationToken) => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T1>(0, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T2>(1, cancellationToken)).Item2)),
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?>>> GetTuplesAsync<T1, T2, T3>(
            this DbDataReader dbDataReader,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetListAsync(
                constructor: async (dbDataReader, cancellationToken) => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T1>(0, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T2>(1, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T3>(2, cancellationToken)).Item2)),
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?>>> GetTuplesAsync<T1, T2, T3, T4>(
            this DbDataReader dbDataReader,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetListAsync(
                constructor: async (dbDataReader, cancellationToken) => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T1>(0, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T2>(1, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T3>(2, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T4>(3, cancellationToken)).Item2)),
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?>>> GetTuplesAsync<T1, T2, T3, T4, T5>(
            this DbDataReader dbDataReader,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetListAsync(
                constructor: async (dbDataReader, cancellationToken) => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T1>(0, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T2>(1, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T3>(2, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T4>(3, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T5>(4, cancellationToken)).Item2)),
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6>(
            this DbDataReader dbDataReader,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetListAsync(
                constructor: async (dbDataReader, cancellationToken) => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T1>(0, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T2>(1, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T3>(2, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T4>(3, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T5>(4, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T6>(5, cancellationToken)).Item2)),
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7>(
            this DbDataReader dbDataReader,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetListAsync(
                constructor: async (dbDataReader, cancellationToken) => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T1>(0, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T2>(1, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T3>(2, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T4>(3, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T5>(4, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T6>(5, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T7>(6, cancellationToken)).Item2)),
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(
            this DbDataReader dbDataReader,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetListAsync(
                constructor: async (dbDataReader, cancellationToken) => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T1>(0, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T2>(1, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T3>(2, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T4>(3, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T5>(4, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T6>(5, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T7>(6, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T8>(7, cancellationToken)).Item2)),
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this DbDataReader dbDataReader,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetListAsync(
                constructor: async (dbDataReader, cancellationToken) => ValueTuple.Create(
                    true,
                    new Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>(
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T1>(0, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T2>(1, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T3>(2, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T4>(3, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T5>(4, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T6>(5, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T7>(6, cancellationToken)).Item2,
                        Tuple.Create(
                            (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T8>(7, cancellationToken)).Item2,
                            (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T9>(8, cancellationToken)).Item2))),
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this DbDataReader dbDataReader,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetListAsync(
                constructor: async (dbDataReader, cancellationToken) => ValueTuple.Create(
                    true,
                    new Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>(
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T1>(0, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T2>(1, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T3>(2, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T4>(3, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T5>(4, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T6>(5, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T7>(6, cancellationToken)).Item2,
                        Tuple.Create(
                            (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T8>(7, cancellationToken)).Item2,
                            (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T9>(8, cancellationToken)).Item2,
                            (await dbDataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync<T10>(9, cancellationToken)).Item2))),
                cancellationToken: cancellationToken);
        #endregion
        #region ResultRangeAsync
        public static async Task<TCollection> AddResultRangeAsync<TCollection, T>(
            this DbDataReader dbDataReader,
            TCollection collection,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>
            => await dbDataReader.InternalAddResultRangeAsync<TCollection, T>(
                collection: collection,
                constructorByIndex: constructorByIndex,
                allowNextResult: true,
                allowAggregateResult: true,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> AddResultRangeAsync<TCollection, T>(
            this DbDataReader dbDataReader,
            TCollection collection,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>
            => await dbDataReader.AddResultRangeAsync<TCollection, T>(
                collection,
                constructorByIndex: constructor is null ? null : (dbDataReader, _, cancellationToken) => constructor(dbDataReader, cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<TCollection> AddResultRangeAsync<TCollection, T>(
            this DbDataReader dbDataReader,
            TCollection collection,
            Func<DbDataReader, CancellationToken, Task<T>> func,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbDataReader.AddResultRangeAsync(
                collection,
                constructorByIndex: async (dbDataReader, _, cancellationToken) => ValueTuple.Create(true, await func(dbDataReader, cancellationToken)));
        public static async Task<TCollection> GetResultCollectionAsync<TCollection, T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbDataReader.AddResultRangeAsync(
                collection: new TCollection(),
                constructorByIndex: constructorByIndex,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetResultCollectionAsync<TCollection, T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbDataReader.AddResultRangeAsync(
                collection: new TCollection(),
                constructor: constructor,
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetResultCollectionAsync<TCollection, T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, CancellationToken, Task<T>> func,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbDataReader.AddResultRangeAsync(
                collection: new TCollection(),
                func: func,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetResultListAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetResultCollectionAsync<List<T>, T>(
                constructorByIndex: constructorByIndex,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetResultListAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetResultCollectionAsync<List<T>, T>(
                constructor: constructor,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetResultListAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, CancellationToken, Task<T>> func,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetResultCollectionAsync<List<T>, T>(
                func: func,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetResultDictionaryAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetResultCollectionAsync<Dictionary<int, T>, KeyValuePair<int, T>>(
                constructorByIndex: async (dbDataReader, resultIndex, cancellationToken) => await dbDataReader.GetKeyValuePairValueTupleAsync<T>(
                    constructorByIndex: constructorByIndex,
                    resultIndex: resultIndex,
                    globalIndex: 0,
                    index: 0,
                    allowAggregateResult: true,
                    cancellationToken: cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetResultDictionaryAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetResultDictionaryAsync<T>(
                constructorByIndex: constructor is null ? null : async (dbDataReader, _, cancellationToken) => await constructor(dbDataReader, cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetResultDictionaryAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, CancellationToken, Task<T>> func,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetResultDictionaryAsync<T>(
                constructorByIndex: async (dbDataReader, _, cancellationToken) => ValueTuple.Create(true, await func(dbDataReader, cancellationToken)),
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, Dictionary<int, Dictionary<string, object?>>>> GetRawDatabaseAsync(
            this DbDataReader dbDataReader,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetResultDictionaryAsync(
                func: GetRawTableAsync,
                cancellationToken: cancellationToken);
        #endregion
        #endregion
    }
}