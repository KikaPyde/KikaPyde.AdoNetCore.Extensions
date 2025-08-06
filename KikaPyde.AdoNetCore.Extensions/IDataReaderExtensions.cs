using System.Data;

namespace KikaPyde.AdoNetCore.Extensions
{
    public static partial class AdoNetCoreHelper
    {
        private static T GetFieldValue<T>(
            this IDataReader dataReader,
            int i)
            => (T)dataReader.GetValue(i);
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
        public static ValueTuple<bool, T> TakeFieldValueOrSkipIfDbNull<T>(
            this IDataReader dataReader,
            Func<IDataReader, int, T> func,
            int fieldIndex,
            int index)
            => dataReader.FieldCount <= fieldIndex && dataReader.IsDBNull(fieldIndex) ? ValueTuple.Create(false, default(T)!) : ValueTuple.Create(true, func(dataReader, fieldIndex));
        public static ValueTuple<bool, T> TakeFieldValueOrSkipIfDbNull<T>(
            this IDataReader dataReader,
            int fieldIndex,
            int index)
            => dataReader.TakeFieldValueOrSkipIfDbNull(
                func: (x, y) => x.GetFieldValue<T>(y),
                fieldIndex: fieldIndex,
                index: index);
        public static ValueTuple<bool, T> TakeFieldValueOrSkipIfDbNull<T>(
            this IDataReader dataReader,
            int fieldIndex)
            => dataReader.TakeFieldValueOrSkipIfDbNull<T>(
                fieldIndex: fieldIndex,
                index: 0);
        public static ValueTuple<bool, T> TakeFirstFieldValueOrSkipIfDbNull<T>(
            this IDataReader dataReader,
            int index)
            => dataReader.TakeFieldValueOrSkipIfDbNull<T>(
                fieldIndex: 0,
                index: index);
        public static ValueTuple<bool, T> TakeFirstFieldValueOrSkipIfDbNull<T>(
            this IDataReader dataReader)
            => dataReader.TakeFirstFieldValueOrSkipIfDbNull<T>(
                index: 0);
        #endregion
        #region TakeOrDefault
        public static ValueTuple<bool, T?> TakeFieldValueOrDefaultIfDbNull<T>(
            this IDataReader dataReader,
            Func<IDataReader, int, T> func,
            int fieldIndex,
            int index)
            => dataReader.FieldCount <= fieldIndex && dataReader.IsDBNull(fieldIndex) ? ValueTuple.Create(true, default(T)) : ValueTuple.Create<bool, T?>(true, func(dataReader, fieldIndex));
        public static ValueTuple<bool, T?> TakeFieldValueOrDefaultIfDbNull<T>(
            this IDataReader dataReader,
            int fieldIndex,
            int index)
            => dataReader.TakeFieldValueOrDefaultIfDbNull(
                func: (x, y) => x.GetFieldValue<T?>(y),
                fieldIndex: fieldIndex,
                index: index);
        public static ValueTuple<bool, T?> TakeFieldValueOrDefaultIfDbNull<T>(
            this IDataReader dataReader,
            int fieldIndex)
            => dataReader.TakeFieldValueOrDefaultIfDbNull<T>(
                fieldIndex: fieldIndex,
                index: 0);
        public static ValueTuple<bool, T?> TakeFirstFieldValueOrDefaultIfDbNull<T>(
            this IDataReader dataReader,
            int index)
            => dataReader.TakeFieldValueOrDefaultIfDbNull<T>(
                fieldIndex: 0,
                index: index);
        public static ValueTuple<bool, T?> TakeFirstFieldValueOrDefaultIfDbNull<T>(
            this IDataReader dataReader)
            => dataReader.TakeFirstFieldValueOrDefaultIfDbNull<T>(
                index: 0);
        #endregion
        #region TakeOrThrow
        public static ValueTuple<bool, T> TakeFieldValueOrThrowIfDbNull<T>(
            this IDataReader dataReader,
            Func<IDataReader, int, T> func,
            int fieldIndex,
            int index)
            => dataReader.FieldCount <= fieldIndex && dataReader.IsDBNull(fieldIndex) ? throw new DataException() : ValueTuple.Create(true, func(dataReader, fieldIndex));
        public static ValueTuple<bool, T> TakeFieldValueOrThrowIfDbNull<T>(
            this IDataReader dataReader,
            int fieldIndex,
            int index)
            => dataReader.TakeFieldValueOrThrowIfDbNull(
                func: (x, y) => x.GetFieldValue<T>(y),
                fieldIndex: fieldIndex,
                index: index);
        public static ValueTuple<bool, T> TakeFieldValueOrThrowIfDbNull<T>(
            this IDataReader dataReader,
            int fieldIndex)
            => dataReader.TakeFieldValueOrThrowIfDbNull<T>(
                fieldIndex: fieldIndex,
                index: 0);
        public static ValueTuple<bool, T> TakeFirstFieldValueOrThrowIfDbNull<T>(
            this IDataReader dataReader,
            int index)
            => dataReader.TakeFieldValueOrThrowIfDbNull<T>(
                fieldIndex: 0,
                index: index);
        public static ValueTuple<bool, T> TakeFirstFieldValueOrThrowIfDbNull<T>(
            this IDataReader dataReader)
            => dataReader.TakeFirstFieldValueOrThrowIfDbNull<T>(
                index: 0);
        #endregion
        public static Dictionary<string, object?> GetRawRow(
            this IDataReader dataReader)
        {
            var row = new Dictionary<string, object?>();
            for (var fieldIndex = 0; fieldIndex < dataReader.FieldCount; fieldIndex++)
                row[dataReader.GetName(fieldIndex)] = dataReader.TakeFieldValueOrDefaultIfDbNull<object>(fieldIndex);
            return row;
        }
        public static TCollection AddRange<TCollection, T>(
            this IDataReader dataReader,
            TCollection collection,
            Func<IDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null)
            where TCollection : ICollection<T>
        {
            var index = 0;
            while (dataReader.Read())
            {
                var r = constructorByIndex is null ? dataReader.TakeFirstFieldValueOrSkipIfDbNull<T>(index) : constructorByIndex(dataReader, index++);
                if (r.Item1)
                    collection.Add(r.Item2);
            }
            return collection;
        }
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
        {
            var result = new TCollection();
            dataReader.AddRange(result, constructorByIndex);
            return result;
        }
        public static TCollection GetCollection<TCollection, T>(
            this IDataReader dataReader,
            Func<IDataReader, ValueTuple<bool, T>>? constructor)
            where TCollection : ICollection<T>, new()
            => dataReader.GetCollection<TCollection, T>(
                constructorByIndex: constructor is null ? null : (dataReader, index) => constructor(dataReader));
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
                constructorByIndex: (dataReader, index) =>
                {
                    if (constructorByIndex is null)
                    {
                        var r = dataReader.TakeFirstFieldValueOrSkipIfDbNull<T>(index);
                        return ValueTuple.Create(r.Item1, KeyValuePair.Create(index, r.Item2));
                    }
                    else
                    {
                        var r = constructorByIndex(dataReader, index);
                        return ValueTuple.Create(r.Item1, KeyValuePair.Create(index, r.Item2));
                    }
                });
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
                constructorByIndex: (dataReader, index) => ValueTuple.Create(true, dataReader.GetRawRow()));
        public static List<Tuple<T1?, T2?>> GetTuples<T1, T2>(
            this IDataReader dataReader)
            => dataReader.GetList(
                constructor: dataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T1>(0).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T2>(1).Item2)));
        public static List<Tuple<T1?, T2?, T3?>> GetTuples<T1, T2, T3>(
            this IDataReader dataReader)
            => dataReader.GetList(
                constructor: dataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T1>(0).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T2>(1).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T3>(2).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?>> GetTuples<T1, T2, T3, T4>(
            this IDataReader dataReader)
            => dataReader.GetList(
                constructor: dataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T1>(0).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T2>(1).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T3>(2).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T4>(3).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?>> GetTuples<T1, T2, T3, T4, T5>(
            this IDataReader dataReader)
            => dataReader.GetList(
                constructor: dataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T1>(0).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T2>(1).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T3>(2).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T4>(3).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T5>(4).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>> GetTuples<T1, T2, T3, T4, T5, T6>(
            this IDataReader dataReader)
            => dataReader.GetList(
                constructor: dataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T1>(0).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T2>(1).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T3>(2).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T4>(3).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T5>(4).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T6>(5).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>> GetTuples<T1, T2, T3, T4, T5, T6, T7>(
            this IDataReader dataReader)
            => dataReader.GetList(
                constructor: dataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T1>(0).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T2>(1).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T3>(2).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T4>(3).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T5>(4).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T6>(5).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T7>(6).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8>(
            this IDataReader dataReader)
            => dataReader.GetList(
                constructor: dataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T1>(0).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T2>(1).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T3>(2).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T4>(3).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T5>(4).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T6>(5).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T7>(6).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T8>(7).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this IDataReader dataReader)
            => dataReader.GetList(
                constructor: dataReader => ValueTuple.Create(
                    true,
                    new Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>(
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T1>(0).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T2>(1).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T3>(2).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T4>(3).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T5>(4).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T6>(5).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T7>(6).Item2,
                        Tuple.Create(
                            dataReader.TakeFieldValueOrDefaultIfDbNull<T8>(7).Item2,
                            dataReader.TakeFieldValueOrDefaultIfDbNull<T9>(8).Item2))));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this IDataReader dataReader)
            => dataReader.GetList(
                constructor: dataReader => ValueTuple.Create(
                    true,
                    new Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>(
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T1>(0).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T2>(1).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T3>(2).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T4>(3).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T5>(4).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T6>(5).Item2,
                        dataReader.TakeFieldValueOrDefaultIfDbNull<T7>(6).Item2,
                        Tuple.Create(
                            dataReader.TakeFieldValueOrDefaultIfDbNull<T8>(7).Item2,
                            dataReader.TakeFieldValueOrDefaultIfDbNull<T9>(8).Item2,
                            dataReader.TakeFieldValueOrDefaultIfDbNull<T10>(10).Item2))));
    }
}
