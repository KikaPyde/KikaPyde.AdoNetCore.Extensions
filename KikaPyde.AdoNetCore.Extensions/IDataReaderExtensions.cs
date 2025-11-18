using System;
using System.Collections.Generic;
using System.Data;

namespace KikaPyde.AdoNetCore.Extensions
{
    public static partial class AdoNetCoreHelper
    {
        private static T GetFieldValue<T>(
            this IDataReader dataReader,
            int i)
            => (T)dataReader.GetValue(i);
        private static bool CheckIfDbNullOrOutOfRange(
            this IDataReader dataReader,
            int fieldIndex)
            => fieldIndex >= dataReader.FieldCount || dataReader.IsDBNull(fieldIndex);
        #region TakeOrSkip
        /// <summary>
        /// It is used by default in the "Get[CollectionTypeName]" methods
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataReader"></param>
        /// <param name="func"></param>
        /// <param name="fieldIndex"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static ValueTuple<bool, T> TakeFieldValueOrSkipIfDbNullOrOutOfRange<T>(
            this IDataReader dataReader,
            Func<IDataReader, int, T> func,
            int fieldIndex,
            int resultIndex = default,
            int globalIndex = default,
            int index = default)
            => dataReader.CheckIfDbNullOrOutOfRange(fieldIndex) ? ValueTuple.Create(false, default(T)!) : ValueTuple.Create(true, func(dataReader, fieldIndex));
        public static ValueTuple<bool, T> TakeFieldValueOrSkipIfDbNullOrOutOfRange<T>(
            this IDataReader dataReader,
            int fieldIndex,
            int resultIndex = default,
            int globalIndex = default,
            int index = default)
            => dataReader.TakeFieldValueOrSkipIfDbNullOrOutOfRange(
                func: (dataReader, fieldIndex) => dataReader.GetFieldValue<T>(fieldIndex),
                fieldIndex: fieldIndex,
                resultIndex: resultIndex,
                globalIndex: globalIndex,
                index: index);
        public static ValueTuple<bool, T> TakeFieldValueOrSkipIfDbNullOrOutOfRange<T>(
            this IDataReader dataReader,
            int fieldIndex,
            int resultIndex = default)
            => dataReader.TakeFieldValueOrSkipIfDbNullOrOutOfRange<T>(
                fieldIndex: fieldIndex,
                resultIndex: resultIndex,
                globalIndex: default,
                index: default);
        public static ValueTuple<bool, T> TakeFieldValueOrSkipIfDbNullOrOutOfRange<T>(
            this IDataReader dataReader,
            int fieldIndex)
            => dataReader.TakeFieldValueOrSkipIfDbNullOrOutOfRange<T>(
                fieldIndex: fieldIndex,
                resultIndex: default,
                globalIndex: default,
                index: default);
        public static ValueTuple<bool, T> TakeFirstFieldValueOrSkipIfDbNullOrOutOfRange<T>(
            this IDataReader dataReader,
            int resultIndex = default,
            int globalIndex = default,
            int index = default)
            => dataReader.TakeFieldValueOrSkipIfDbNullOrOutOfRange<T>(
                fieldIndex: default,
                resultIndex: default,
                globalIndex: default,
                index: default);
        public static ValueTuple<bool, T> TakeFirstFieldValueOrSkipIfDbNullOrOutOfRange<T>(
            this IDataReader dataReader,
            int resultIndex = default)
            => dataReader.TakeFieldValueOrSkipIfDbNullOrOutOfRange<T>(
                fieldIndex: default,
                resultIndex: default,
                globalIndex: default,
                index: default);
        public static ValueTuple<bool, T> TakeFirstFieldValueOrSkipIfDbNullOrOutOfRange<T>(
            this IDataReader dataReader)
            => dataReader.TakeFieldValueOrSkipIfDbNullOrOutOfRange<T>(
                fieldIndex: default,
                resultIndex: default,
                globalIndex: default,
                index: default);
        #endregion
        #region TakeOrDefault
        public static ValueTuple<bool, T?> TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T>(
            this IDataReader dataReader,
            Func<IDataReader, int, T> func,
            int fieldIndex,
            int resultIndex = default,
            int globalIndex = default,
            int index = default)
            => dataReader.CheckIfDbNullOrOutOfRange(fieldIndex) ? ValueTuple.Create(true, default(T)) : ValueTuple.Create(true, func(dataReader, fieldIndex));
        public static ValueTuple<bool, T?> TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T>(
            this IDataReader dataReader,
            int fieldIndex,
            int resultIndex = default,
            int globalIndex = default,
            int index = default)
            => dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange(
                func: (dataReader, fieldIndex) => dataReader.GetFieldValue<T?>(fieldIndex),
                fieldIndex: fieldIndex,
                resultIndex: resultIndex,
                globalIndex: globalIndex,
                index: index);
        public static ValueTuple<bool, T?> TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T>(
            this IDataReader dataReader,
            int fieldIndex,
            int resultIndex = default)
            => dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T>(
                fieldIndex: fieldIndex,
                resultIndex: resultIndex,
                globalIndex: default,
                index: default);
        public static ValueTuple<bool, T?> TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T>(
            this IDataReader dataReader,
            int fieldIndex)
            => dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T>(
                fieldIndex: fieldIndex,
                resultIndex: default,
                globalIndex: default,
                index: default);
        public static ValueTuple<bool, T?> TakeFirstFieldValueOrDefaultIfDbNullOrOutOfRange<T>(
            this IDataReader dataReader,
            int resultIndex = default,
            int globalIndex = default,
            int index = default)
            => dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T>(
                fieldIndex: default,
                resultIndex: default,
                globalIndex: default,
                index: default);
        public static ValueTuple<bool, T?> TakeFirstFieldValueOrDefaultIfDbNullOrOutOfRange<T>(
            this IDataReader dataReader,
            int resultIndex = default)
            => dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T>(
                fieldIndex: default,
                resultIndex: default,
                globalIndex: default,
                index: default);
        public static ValueTuple<bool, T?> TakeFirstFieldValueOrDefaultIfDbNullOrOutOfRange<T>(
            this IDataReader dataReader)
            => dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T>(
                fieldIndex: default,
                resultIndex: default,
                globalIndex: default,
                index: default);
        #endregion
        #region TakeOrThrow
        public static ValueTuple<bool, T> TakeFieldValueOrThrowIfDbNullOrOutOfRange<T>(
            this IDataReader dataReader,
            Func<IDataReader, int, T> func,
            int fieldIndex,
            int resultIndex = default,
            int globalIndex = default,
            int index = default)
            => ValueTuple.Create(true, func(dataReader, fieldIndex));
        public static ValueTuple<bool, T> TakeFieldValueOrThrowIfDbNullOrOutOfRange<T>(
            this IDataReader dataReader,
            int fieldIndex,
            int resultIndex = default,
            int globalIndex = default,
            int index = default)
            => dataReader.TakeFieldValueOrThrowIfDbNullOrOutOfRange(
                func: (dataReader, fieldIndex) => dataReader.GetFieldValue<T>(fieldIndex),
                fieldIndex: fieldIndex,
                resultIndex: resultIndex,
                globalIndex: globalIndex,
                index: index);
        public static ValueTuple<bool, T> TakeFieldValueOrThrowIfDbNullOrOutOfRange<T>(
            this IDataReader dataReader,
            int fieldIndex,
            int resultIndex = default)
            => dataReader.TakeFieldValueOrThrowIfDbNullOrOutOfRange<T>(
                fieldIndex: fieldIndex,
                resultIndex: resultIndex,
                globalIndex: default,
                index: default);
        public static ValueTuple<bool, T> TakeFieldValueOrThrowIfDbNullOrOutOfRange<T>(
            this IDataReader dataReader,
            int fieldIndex)
            => dataReader.TakeFieldValueOrThrowIfDbNullOrOutOfRange<T>(
                fieldIndex: fieldIndex,
                resultIndex: default,
                globalIndex: default,
                index: default);
        public static ValueTuple<bool, T> TakeFirstFieldValueOrThrowIfDbNullOrOutOfRange<T>(
            this IDataReader dataReader,
            int resultIndex = default,
            int globalIndex = default,
            int index = default)
            => dataReader.TakeFieldValueOrThrowIfDbNullOrOutOfRange<T>(
                fieldIndex: default,
                resultIndex: default,
                globalIndex: default,
                index: default);
        public static ValueTuple<bool, T> TakeFirstFieldValueOrThrowIfDbNullOrOutOfRange<T>(
            this IDataReader dataReader,
            int resultIndex = default)
            => dataReader.TakeFieldValueOrThrowIfDbNullOrOutOfRange<T>(
                fieldIndex: default,
                resultIndex: default,
                globalIndex: default,
                index: default);
        public static ValueTuple<bool, T> TakeFirstFieldValueOrThrowIfDbNullOrOutOfRange<T>(
            this IDataReader dataReader)
            => dataReader.TakeFieldValueOrThrowIfDbNullOrOutOfRange<T>(
                fieldIndex: default,
                resultIndex: default,
                globalIndex: default,
                index: default);
        #endregion
        public static Dictionary<string, object?> GetRawRow(
            this IDataReader dataReader)
        {
            var row = new Dictionary<string, object?>();
            for (var fieldIndex = 0; fieldIndex < dataReader.FieldCount; fieldIndex++)
                row[dataReader.GetName(fieldIndex)] = dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<object>(fieldIndex).Item2;
            return row;
        }
        private static ValueTuple<bool, T> InvokeConstructor<T>(
            this IDataReader dataReader,
            Delegate? constructorByIndex,
            int resultIndex,
            int globalIndex,
            int index,
            bool allowAggregateResult)
            => constructorByIndex switch
            {
                Func<IDataReader, int, ValueTuple<bool, T>> func3 => allowAggregateResult
                    ? func3(dataReader, resultIndex)
                    : func3(dataReader, index),
                Func<IDataReader, int, int, int, ValueTuple<bool, T>> func5 when !allowAggregateResult =>
                    func5(dataReader, resultIndex, globalIndex, index),
                null => dataReader.TakeFirstFieldValueOrSkipIfDbNullOrOutOfRange<T>(),
                _ => throw new InvalidCastException(nameof(constructorByIndex)),
            };
        private static TCollection InternalAddResultRange<TCollection, T>(
            this IDataReader dataReader,
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
                        dataReader.Read();
                    var result = dataReader.InvokeConstructor<T>(constructorByIndex, resultIndex, globalIndex++, index, allowAggregateResult);
                    if (result.Item1)
                        collection.Add(result.Item2);
                }
                else
                {
                    index = 0;
                    while (dataReader.Read())
                    {
                        var result = dataReader.InvokeConstructor<T>(constructorByIndex, resultIndex, globalIndex++, index++, allowAggregateResult);
                        if (result.Item1)
                            collection.Add(result.Item2);
                    }
                }
                resultIndex++;
            }
            while (allowNextResult && dataReader.NextResult());
            return collection;
        }
        private static ValueTuple<bool, KeyValuePair<int, T>> GetKeyValuePairValueTuple<T>(
            this IDataReader dataReader,
            Delegate? constructorByIndex,
            int resultIndex,
            int globalIndex,
            int index,
            bool allowAggregateResult)
        {
            var result = dataReader.InvokeConstructor<T>(constructorByIndex, resultIndex, globalIndex, index, allowAggregateResult);
            return ValueTuple.Create(result.Item1, KeyValuePair.Create(allowAggregateResult ? resultIndex : index, result.Item2));
        }
        #region Range
        public static TCollection AddRange<TCollection, T>(
            this IDataReader dataReader,
            TCollection collection,
            Func<IDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null)
            where TCollection : ICollection<T>
            => dataReader.InternalAddResultRange<TCollection, T>(
                collection: collection,
                constructorByIndex: constructorByIndex,
                allowNextResult: false,
                allowAggregateResult: false);
        public static TCollection AddRange<TCollection, T>(
            this IDataReader dataReader,
            TCollection collection,
            Func<IDataReader, ValueTuple<bool, T>>? constructor)
            where TCollection : ICollection<T>
            => dataReader.AddRange<TCollection, T>(
                collection: collection,
                constructorByIndex: constructor is null ? null : (x, _) => constructor(x));
        public static TCollection GetCollection<TCollection, T>(
            this IDataReader dataReader,
            Func<IDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null)
            where TCollection : ICollection<T>, new()
            => dataReader.AddRange(
                collection: new TCollection(),
                constructorByIndex: constructorByIndex);
        public static TCollection GetCollection<TCollection, T>(
            this IDataReader dataReader,
            Func<IDataReader, ValueTuple<bool, T>>? constructor)
            where TCollection : ICollection<T>, new()
            => dataReader.AddRange(
                collection: new TCollection(),
                constructor: constructor);
        public static List<T> GetList<T>(
            this IDataReader dataReader,
            Func<IDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null)
            => dataReader.GetCollection<List<T>, T>(
                constructorByIndex: constructorByIndex);
        public static List<T> GetList<T>(
            this IDataReader dataReader,
            Func<IDataReader, ValueTuple<bool, T>>? constructor)
            => dataReader.GetCollection<List<T>, T>(
                constructor: constructor);
        public static Dictionary<int, T> GetDictionary<T>(
            this IDataReader dataReader,
            Func<IDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null)
            => dataReader.GetCollection<Dictionary<int, T>, KeyValuePair<int, T>>(
                constructorByIndex: (dataReader, index) => dataReader.GetKeyValuePairValueTuple<T>(
                    constructorByIndex: constructorByIndex,
                    resultIndex: default,
                    globalIndex: default,
                    index: index,
                    allowAggregateResult: false));
        public static Dictionary<int, T> GetDictionary<T>(
            this IDataReader dataReader,
            Func<IDataReader, ValueTuple<bool, T>>? constructor)
            => dataReader.GetDictionary<T>(
                constructorByIndex: constructor is null ? null : (x, _) => constructor(x));
        public static DataTable GetDataTable(
            this IDataReader dataReader)
        {
            var dataTable = new DataTable();
            while (dataReader.Read())
            {
                var dataRow = dataTable.NewRow();
                for (var fieldIndex = 0; fieldIndex < dataReader.FieldCount; fieldIndex++)
                {
                    if (dataTable.Columns.Count == fieldIndex)
                        dataTable.Columns.Add(dataReader.GetName(fieldIndex), dataReader.GetFieldType(fieldIndex));
                    if (!dataReader.IsDBNull(fieldIndex))
                        dataRow[fieldIndex] = dataReader.GetFieldValue<object>(fieldIndex);
                }
                dataTable.Rows.Add(dataRow);
            }
            return dataTable;
        }
        public static Dictionary<int, Dictionary<string, object?>> GetRawTable(
            this IDataReader dataReader)
            => dataReader.GetDictionary(
                constructor: dataReader => ValueTuple.Create(true, dataReader.GetRawRow()));
        public static List<Tuple<T1?, T2?>> GetTuples<T1, T2>(
            this IDataReader dataReader)
            => dataReader.GetList(
                constructor: dataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T1>(0).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T2>(1).Item2)));
        public static List<Tuple<T1?, T2?, T3?>> GetTuples<T1, T2, T3>(
            this IDataReader dataReader)
            => dataReader.GetList(
                constructor: dataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T1>(0).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T2>(1).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T3>(2).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?>> GetTuples<T1, T2, T3, T4>(
            this IDataReader dataReader)
            => dataReader.GetList(
                constructor: dataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T1>(0).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T2>(1).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T3>(2).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T4>(3).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?>> GetTuples<T1, T2, T3, T4, T5>(
            this IDataReader dataReader)
            => dataReader.GetList(
                constructor: dataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T1>(0).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T2>(1).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T3>(2).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T4>(3).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T5>(4).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>> GetTuples<T1, T2, T3, T4, T5, T6>(
            this IDataReader dataReader)
            => dataReader.GetList(
                constructor: dataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T1>(0).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T2>(1).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T3>(2).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T4>(3).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T5>(4).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T6>(5).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>> GetTuples<T1, T2, T3, T4, T5, T6, T7>(
            this IDataReader dataReader)
            => dataReader.GetList(
                constructor: dataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T1>(0).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T2>(1).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T3>(2).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T4>(3).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T5>(4).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T6>(5).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T7>(6).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8>(
            this IDataReader dataReader)
            => dataReader.GetList(
                constructor: dataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T1>(0).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T2>(1).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T3>(2).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T4>(3).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T5>(4).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T6>(5).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T7>(6).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T8>(7).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this IDataReader dataReader)
            => dataReader.GetList(
                constructor: dataReader => ValueTuple.Create(
                    true,
                    new Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>(
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T1>(0).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T2>(1).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T3>(2).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T4>(3).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T5>(4).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T6>(5).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T7>(6).Item2,
                        Tuple.Create(
                            dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T8>(7).Item2,
                            dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T9>(8).Item2))));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this IDataReader dataReader)
            => dataReader.GetList(
                constructor: dataReader => ValueTuple.Create(
                    true,
                    new Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>(
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T1>(0).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T2>(1).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T3>(2).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T4>(3).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T5>(4).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T6>(5).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T7>(6).Item2,
                        Tuple.Create(
                            dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T8>(7).Item2,
                            dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T9>(8).Item2,
                            dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T10>(10).Item2))));
        #endregion
        #region ResultRange
        public static TCollection AddResultRange<TCollection, T>(
            this IDataReader dataReader,
            TCollection collection,
            Func<IDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null)
            where TCollection : ICollection<T>
            => dataReader.InternalAddResultRange<TCollection, T>(
                collection: collection,
                constructorByIndex: constructorByIndex,
                allowNextResult: true,
                allowAggregateResult: true);
        public static TCollection AddResultRange<TCollection, T>(
            this IDataReader dataReader,
            TCollection collection,
            Func<IDataReader, ValueTuple<bool, T>>? constructor)
            where TCollection : ICollection<T>
            => dataReader.AddResultRange<TCollection, T>(
                collection: collection,
                constructorByIndex: constructor is null ? null : (dataReader, _) => constructor(dataReader));
        public static TCollection AddResultRange<TCollection, T>(
            this IDataReader dataReader,
            TCollection collection,
            Func<IDataReader, T> func)
            where TCollection : ICollection<T>, new()
            => dataReader.AddResultRange(
                collection,
                constructorByIndex: (dataReader, _) => ValueTuple.Create(true, func(dataReader)));
        public static TCollection GetResultCollection<TCollection, T>(
            this IDataReader dataReader,
            Func<IDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null)
            where TCollection : ICollection<T>, new()
            => dataReader.AddResultRange(
                collection: new TCollection(),
                constructorByIndex: constructorByIndex);
        public static TCollection GetResultCollection<TCollection, T>(
            this IDataReader dataReader,
            Func<IDataReader, ValueTuple<bool, T>>? constructor)
            where TCollection : ICollection<T>, new()
            => dataReader.AddRange(
                collection: new TCollection(),
                constructor: constructor);
        public static TCollection GetResultCollection<TCollection, T>(
            this IDataReader dataReader,
            Func<IDataReader, T> func)
            where TCollection : ICollection<T>, new()
            => dataReader.AddResultRange(
                collection: new TCollection(),
                func: func);
        public static List<T> GetResultList<T>(
            this IDataReader dataReader,
            Func<IDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null)
            => dataReader.GetResultCollection<List<T>, T>(
                constructorByIndex: constructorByIndex);
        public static List<T> GetResultList<T>(
            this IDataReader dataReader,
            Func<IDataReader, ValueTuple<bool, T>>? constructor)
            => dataReader.GetResultCollection<List<T>, T>(
                constructor: constructor);
        public static List<T> GetResultList<T>(
            this IDataReader dataReader,
            Func<IDataReader, T> func)
            => dataReader.GetResultCollection<List<T>, T>(
                func: func);
        public static Dictionary<int, T> GetResultDictionary<T>(
            this IDataReader dataReader,
            Func<IDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null)
            => dataReader.GetResultCollection<Dictionary<int, T>, KeyValuePair<int, T>>(
                constructorByIndex: (dataReader, resultIndex) => dataReader.GetKeyValuePairValueTuple<T>(
                    constructorByIndex: constructorByIndex,
                    resultIndex: resultIndex,
                    globalIndex: 0,
                    index: 0,
                    allowAggregateResult: true));
        public static Dictionary<int, T> GetResultDictionary<T>(
            this IDataReader dataReader,
            Func<IDataReader, ValueTuple<bool, T>>? constructor)
            => dataReader.GetResultDictionary<T>(
                constructorByIndex: constructor is null ? null : (dataReader, _) => constructor(dataReader));
        public static Dictionary<int, T> GetResultDictionary<T>(
            this IDataReader dataReader,
            Func<IDataReader, T> func)
            => dataReader.GetResultDictionary<T>(
                constructorByIndex: (dataReader, _) => ValueTuple.Create(true, func(dataReader)));
        public static Dictionary<int, Dictionary<int, Dictionary<string, object?>>> GetRawDatabase(
            this IDataReader dataReader)
            => GetResultDictionary(
                dataReader: dataReader,
                func: GetRawTable);
        #endregion
        #region GlobalRange
        public static TCollection AddGlobalRange<TCollection, T>(
            this IDataReader dataReader,
            TCollection collection,
            Func<IDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex = null)
            where TCollection : ICollection<T>
            => dataReader.InternalAddResultRange<TCollection, T>(
                collection: collection,
                constructorByIndex: constructorByIndex,
                allowNextResult: true,
                allowAggregateResult: false);
        public static TCollection AddGlobalRange<TCollection, T>(
            this IDataReader dataReader,
            TCollection collection,
            Func<IDataReader, ValueTuple<bool, T>>? constructor)
            where TCollection : ICollection<T>
            => dataReader.AddGlobalRange<TCollection, T>(
                collection: collection,
                constructorByIndex: constructor is null ? null : (x, _, _, _) => constructor(x));
        public static TCollection GetGlobalCollection<TCollection, T>(
            this IDataReader dataReader,
            Func<IDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex = null)
            where TCollection : ICollection<T>, new()
            => dataReader.AddGlobalRange(
                collection: new TCollection(),
                constructorByIndex: constructorByIndex);
        public static TCollection GetGlobalCollection<TCollection, T>(
            this IDataReader dataReader,
            Func<IDataReader, ValueTuple<bool, T>>? constructor)
            where TCollection : ICollection<T>, new()
            => dataReader.AddGlobalRange(
                collection: new TCollection(),
                constructor: constructor);
        public static List<T> GetGlobalList<T>(
            this IDataReader dataReader,
            Func<IDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex = null)
            => dataReader.GetGlobalCollection<List<T>, T>(
                constructorByIndex: constructorByIndex);
        public static List<T> GetGlobalList<T>(
            this IDataReader dataReader,
            Func<IDataReader, ValueTuple<bool, T>>? constructor)
            => dataReader.GetGlobalCollection<List<T>, T>(
                constructor: constructor);
        public static Dictionary<int, T> GetGlobalDictionary<T>(
            this IDataReader dataReader,
            Func<IDataReader, int, int, int, ValueTuple<bool, T>>? constructorByIndex = null)
            => dataReader.GetGlobalCollection<Dictionary<int, T>, KeyValuePair<int, T>>(
                constructorByIndex: (dataReader, resultIndex, globalIndex, index) => dataReader.GetKeyValuePairValueTuple<T>(
                    constructorByIndex: constructorByIndex,
                    resultIndex: resultIndex,
                    globalIndex: globalIndex,
                    index: index,
                    allowAggregateResult: false));
        public static Dictionary<int, T> GetGlobalDictionary<T>(
            this IDataReader dataReader,
            Func<IDataReader, ValueTuple<bool, T>>? constructor)
            => dataReader.GetGlobalDictionary<T>(
                constructorByIndex: constructor is null ? null : (x, _, _, _) => constructor(x));
        public static Dictionary<int, Dictionary<string, object?>> GetGlobalRawTable(
            this IDataReader dataReader)
            => dataReader.GetGlobalDictionary(
                constructor: dataReader => ValueTuple.Create(true, dataReader.GetRawRow()));
        public static List<Tuple<T1?, T2?>> GetGlobalTuples<T1, T2>(
            this IDataReader dataReader)
            => dataReader.GetGlobalList(
                constructor: dataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T1>(0).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T2>(1).Item2)));
        public static List<Tuple<T1?, T2?, T3?>> GetGlobalTuples<T1, T2, T3>(
            this IDataReader dataReader)
            => dataReader.GetGlobalList(
                constructor: dataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T1>(0).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T2>(1).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T3>(2).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?>> GetGlobalTuples<T1, T2, T3, T4>(
            this IDataReader dataReader)
            => dataReader.GetGlobalList(
                constructor: dataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T1>(0).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T2>(1).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T3>(2).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T4>(3).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?>> GetGlobalTuples<T1, T2, T3, T4, T5>(
            this IDataReader dataReader)
            => dataReader.GetGlobalList(
                constructor: dataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T1>(0).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T2>(1).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T3>(2).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T4>(3).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T5>(4).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>> GetGlobalTuples<T1, T2, T3, T4, T5, T6>(
            this IDataReader dataReader)
            => dataReader.GetGlobalList(
                constructor: dataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T1>(0).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T2>(1).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T3>(2).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T4>(3).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T5>(4).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T6>(5).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>> GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7>(
            this IDataReader dataReader)
            => dataReader.GetGlobalList(
                constructor: dataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T1>(0).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T2>(1).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T3>(2).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T4>(3).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T5>(4).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T6>(5).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T7>(6).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>> GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7, T8>(
            this IDataReader dataReader)
            => dataReader.GetGlobalList(
                constructor: dataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T1>(0).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T2>(1).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T3>(2).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T4>(3).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T5>(4).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T6>(5).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T7>(6).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T8>(7).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>> GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this IDataReader dataReader)
            => dataReader.GetGlobalList(
                constructor: dataReader => ValueTuple.Create(
                    true,
                    new Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>(
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T1>(0).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T2>(1).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T3>(2).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T4>(3).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T5>(4).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T6>(5).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T7>(6).Item2,
                        Tuple.Create(
                            dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T8>(7).Item2,
                            dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T9>(8).Item2))));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>> GetGlobalTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this IDataReader dataReader)
            => dataReader.GetGlobalList(
                constructor: dataReader => ValueTuple.Create(
                    true,
                    new Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>(
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T1>(0).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T2>(1).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T3>(2).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T4>(3).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T5>(4).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T6>(5).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T7>(6).Item2,
                        Tuple.Create(
                            dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T8>(7).Item2,
                            dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T9>(8).Item2,
                            dataReader.TakeFieldValueOrDefaultIfDbNullOrOutOfRange<T10>(10).Item2))));
        #endregion
    }
}
