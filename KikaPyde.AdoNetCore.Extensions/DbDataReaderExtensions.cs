using System.Data;
using System.Data.Common;

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
        public static ValueTuple<bool, T> TakeFieldValueOrSkipIfDbNull<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, T> func,
            int fieldIndex,
            int index)
            => dbDataReader.FieldCount <= fieldIndex && dbDataReader.IsDBNull(fieldIndex) ? ValueTuple.Create(false, default(T)!) : ValueTuple.Create(true, func(dbDataReader, fieldIndex));
        public static ValueTuple<bool, T> TakeFieldValueOrSkipIfDbNull<T>(
            this DbDataReader dbDataReader,
            int fieldIndex,
            int index)
            => dbDataReader.TakeFieldValueOrSkipIfDbNull(
                func: (x, y) => x.GetFieldValue<T>(y),
                fieldIndex: fieldIndex,
                index: index);
        public static ValueTuple<bool, T> TakeFieldValueOrSkipIfDbNull<T>(
            this DbDataReader dbDataReader,
            int fieldIndex)
            => dbDataReader.TakeFieldValueOrSkipIfDbNull<T>(
                fieldIndex: fieldIndex,
                index: 0);
        public static ValueTuple<bool, T> TakeFirstFieldValueOrSkipIfDbNull<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, T> func,
            int index)
            => dbDataReader.TakeFieldValueOrSkipIfDbNull<T>(
                func: (x, y) => func(x),
                fieldIndex: 0,
                index: index);
        public static ValueTuple<bool, T> TakeFirstFieldValueOrSkipIfDbNull<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, T> func)
            => dbDataReader.TakeFirstFieldValueOrSkipIfDbNull<T>(
                func: func,
                index: 0);
        public static ValueTuple<bool, T> TakeFirstFieldValueOrSkipIfDbNull<T>(
            this DbDataReader dbDataReader,
            int index)
            => dbDataReader.TakeFieldValueOrSkipIfDbNull<T>(
                fieldIndex: 0,
                index: index);
        public static ValueTuple<bool, T> TakeFirstFieldValueOrSkipIfDbNull<T>(
            this DbDataReader dbDataReader)
            => dbDataReader.TakeFirstFieldValueOrSkipIfDbNull<T>(
                index: 0);
        #endregion
        #region TakeOrDefault
        public static ValueTuple<bool, T?> TakeFieldValueOrDefaultIfDbNull<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, T> func,
            int fieldIndex,
            int index)
            => dbDataReader.FieldCount <= fieldIndex && dbDataReader.IsDBNull(fieldIndex) ? ValueTuple.Create(true, default(T)) : ValueTuple.Create<bool, T?>(true, func(dbDataReader, fieldIndex));
        public static ValueTuple<bool, T?> TakeFieldValueOrDefaultIfDbNull<T>(
            this DbDataReader dbDataReader,
            int fieldIndex,
            int index)
            => dbDataReader.TakeFieldValueOrDefaultIfDbNull(
                func: (x, y) => x.GetFieldValue<T?>(y),
                fieldIndex: fieldIndex,
                index: index);
        public static ValueTuple<bool, T?> TakeFieldValueOrDefaultIfDbNull<T>(
            this DbDataReader dbDataReader,
            int fieldIndex)
            => dbDataReader.TakeFieldValueOrDefaultIfDbNull<T>(
                fieldIndex: fieldIndex,
                index: 0);
        public static ValueTuple<bool, T?> TakeFirstFieldValueOrDefaultIfDbNull<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, T> func,
            int index)
            => dbDataReader.TakeFieldValueOrDefaultIfDbNull(
                func: (x, y) => func(x),
                fieldIndex: 0,
                index: index);
        public static ValueTuple<bool, T?> TakeFirstFieldValueOrDefaultIfDbNull<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, T> func)
            => dbDataReader.TakeFirstFieldValueOrDefaultIfDbNull<T>(
                func: func,
                index: 0);
        public static ValueTuple<bool, T?> TakeFirstFieldValueOrDefaultIfDbNull<T>(
            this DbDataReader dbDataReader,
            int index)
            => dbDataReader.TakeFieldValueOrDefaultIfDbNull<T>(
                fieldIndex: 0,
                index: index);
        public static ValueTuple<bool, T?> TakeFirstFieldValueOrDefaultIfDbNull<T>(
            this DbDataReader dbDataReader)
            => dbDataReader.TakeFirstFieldValueOrDefaultIfDbNull<T>(
                index: 0);
        #endregion
        #region TakeOrThrow
        public static ValueTuple<bool, T> TakeFieldValueOrThrowIfDbNull<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, T> func,
            int fieldIndex,
            int index)
            => dbDataReader.FieldCount <= fieldIndex && dbDataReader.IsDBNull(fieldIndex) ? throw new DataException() : ValueTuple.Create(true, func(dbDataReader, fieldIndex));
        public static ValueTuple<bool, T> TakeFieldValueOrThrowIfDbNull<T>(
            this DbDataReader dbDataReader,
            int fieldIndex,
            int index)
            => dbDataReader.TakeFieldValueOrThrowIfDbNull(
                func: (x, y) => x.GetFieldValue<T>(y),
                fieldIndex: fieldIndex,
                index: index);
        public static ValueTuple<bool, T> TakeFieldValueOrThrowIfDbNull<T>(
            this DbDataReader dbDataReader,
            int fieldIndex)
            => dbDataReader.TakeFieldValueOrThrowIfDbNull<T>(
                fieldIndex: fieldIndex,
                index: 0);
        public static ValueTuple<bool, T> TakeFirstFieldValueOrThrowIfDbNull<T>(
            this DbDataReader dbDataReader,
            int index)
            => dbDataReader.TakeFieldValueOrThrowIfDbNull<T>(
                fieldIndex: 0,
                index: index);
        public static ValueTuple<bool, T> TakeFirstFieldValueOrThrowIfDbNull<T>(
            this DbDataReader dbDataReader)
            => dbDataReader.TakeFirstFieldValueOrThrowIfDbNull<T>(
                index: 0);
        #endregion
        public static Dictionary<string, object?> GetRawRow(
            this DbDataReader dbDataReader)
        {
            var row = new Dictionary<string, object?>();
            for (var fieldIndex = 0; fieldIndex < dbDataReader.FieldCount; fieldIndex++)
                row[dbDataReader.GetName(fieldIndex)] = dbDataReader.TakeFieldValueOrDefaultIfDbNull<object>(fieldIndex);
            return row;
        }
        public static TCollection AddRange<TCollection, T>(
            this DbDataReader dbDataReader,
            TCollection collection,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null)
            where TCollection : ICollection<T>
        {
            var index = 0;
            while (dbDataReader.Read())
            {
                var r = constructorByIndex is null ? dbDataReader.TakeFirstFieldValueOrSkipIfDbNull<T>(index) : constructorByIndex(dbDataReader, index++);
                if (r.Item1)
                    collection.Add(r.Item2);
            }
            return collection;
        }
        public static TCollection AddRange<TCollection, T>(
            this DbDataReader dbDataReader,
            TCollection collection,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor)
            where TCollection : ICollection<T>
            => dbDataReader.AddRange<TCollection, T>(
                collection: collection,
                constructorByIndex: constructor is null ? null : (x, _) => constructor(x));
        public static TCollection GetCollection<TCollection, T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null)
            where TCollection : ICollection<T>, new()
        {
            var result = new TCollection();
            dbDataReader.AddRange(result, constructorByIndex);
            return result;
        }
        public static TCollection GetCollection<TCollection, T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor)
            where TCollection : ICollection<T>, new()
            => dbDataReader.GetCollection<TCollection, T>(
                constructorByIndex: constructor is null ? null : (dbDataReader, index) => constructor(dbDataReader));
        public static List<T> GetList<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null)
            => dbDataReader.GetCollection<List<T>, T>(
                constructorByIndex: constructorByIndex);
        public static List<T> GetList<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor)
            => dbDataReader.GetCollection<List<T>, T>(
                constructor: constructor);
        public static Dictionary<int, T> GetDictionary<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex = null)
            => dbDataReader.GetCollection<Dictionary<int, T>, KeyValuePair<int, T>>(
                constructorByIndex: (dbDataReader, index) =>
                {
                    if (constructorByIndex is null)
                    {
                        var r = dbDataReader.TakeFirstFieldValueOrSkipIfDbNull<T>(index);
                        return ValueTuple.Create(r.Item1, KeyValuePair.Create(index, r.Item2));
                    }
                    else
                    {
                        var r = constructorByIndex(dbDataReader, index);
                        return ValueTuple.Create(r.Item1, KeyValuePair.Create(index, r.Item2));
                    }
                });
        public static Dictionary<int, T> GetDictionary<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor)
            => dbDataReader.GetDictionary<T>(
                constructorByIndex: constructor is null ? null : (x, _) => constructor(x));
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
                constructorByIndex: (dbDataReader, index) => ValueTuple.Create(true, dbDataReader.GetRawRow()));
        public static List<Tuple<T1?, T2?>> GetTuples<T1, T2>(
            this DbDataReader dbDataReader)
            => dbDataReader.GetList(
                constructor: dbDataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T1>(0).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T2>(1).Item2)));
        public static List<Tuple<T1?, T2?, T3?>> GetTuples<T1, T2, T3>(
            this DbDataReader dbDataReader)
            => dbDataReader.GetList(
                constructor: dbDataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T1>(0).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T2>(1).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T3>(2).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?>> GetTuples<T1, T2, T3, T4>(
            this DbDataReader dbDataReader)
            => dbDataReader.GetList(
                constructor: dbDataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T1>(0).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T2>(1).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T3>(2).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T4>(3).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?>> GetTuples<T1, T2, T3, T4, T5>(
            this DbDataReader dbDataReader)
            => dbDataReader.GetList(
                constructor: dbDataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T1>(0).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T2>(1).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T3>(2).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T4>(3).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T5>(4).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>> GetTuples<T1, T2, T3, T4, T5, T6>(
            this DbDataReader dbDataReader)
            => dbDataReader.GetList(
                constructor: dbDataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T1>(0).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T2>(1).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T3>(2).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T4>(3).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T5>(4).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T6>(5).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>> GetTuples<T1, T2, T3, T4, T5, T6, T7>(
            this DbDataReader dbDataReader)
            => dbDataReader.GetList(
                constructor: dbDataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T1>(0).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T2>(1).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T3>(2).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T4>(3).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T5>(4).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T6>(5).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T7>(6).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8>(
            this DbDataReader dbDataReader)
            => dbDataReader.GetList(
                constructor: dbDataReader => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T1>(0).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T2>(1).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T3>(2).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T4>(3).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T5>(4).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T6>(5).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T7>(6).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T8>(7).Item2)));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this DbDataReader dbDataReader)
            => dbDataReader.GetList(
                constructor: dbDataReader => ValueTuple.Create(
                    true,
                    new Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>(
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T1>(0).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T2>(1).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T3>(2).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T4>(3).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T5>(4).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T6>(5).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T7>(6).Item2,
                        Tuple.Create(
                            dbDataReader.TakeFieldValueOrDefaultIfDbNull<T8>(7).Item2,
                            dbDataReader.TakeFieldValueOrDefaultIfDbNull<T9>(8).Item2))));
        public static List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>> GetTuples<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this DbDataReader dbDataReader)
            => dbDataReader.GetList(
                constructor: dbDataReader => ValueTuple.Create(
                    true,
                    new Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>(
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T1>(0).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T2>(1).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T3>(2).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T4>(3).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T5>(4).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T6>(5).Item2,
                        dbDataReader.TakeFieldValueOrDefaultIfDbNull<T7>(6).Item2,
                        Tuple.Create(
                            dbDataReader.TakeFieldValueOrDefaultIfDbNull<T8>(7).Item2,
                            dbDataReader.TakeFieldValueOrDefaultIfDbNull<T9>(8).Item2,
                            dbDataReader.TakeFieldValueOrDefaultIfDbNull<T10>(10).Item2))));
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
        public static async Task<ValueTuple<bool, T>> TakeFieldValueOrSkipIfDbNullAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, CancellationToken, Task<T>> func,
            int fieldIndex,
            int index,
            CancellationToken cancellationToken = default)
            => dbDataReader.FieldCount <= fieldIndex && await dbDataReader.IsDBNullAsync(fieldIndex, cancellationToken) ? ValueTuple.Create(false, default(T)!) : ValueTuple.Create(true, await func(dbDataReader, fieldIndex, cancellationToken));
        public static async Task<ValueTuple<bool, T>> TakeFieldValueOrSkipIfDbNullAsync<T>(
            this DbDataReader dbDataReader,
            int fieldIndex,
            int index,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFieldValueOrSkipIfDbNullAsync(
                func: async (x, y, z) => await x.GetFieldValueAsync<T>(y, z),
                fieldIndex: fieldIndex,
                index: index,
                cancellationToken: cancellationToken);
        public static async Task<ValueTuple<bool, T>> TakeFieldValueOrSkipIfDbNullAsync<T>(
            this DbDataReader dbDataReader,
            int fieldIndex,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFieldValueOrSkipIfDbNullAsync<T>(
                fieldIndex: fieldIndex,
                index: 0,
                cancellationToken: cancellationToken);
        public static async Task<ValueTuple<bool, T>> TakeFirstFieldValueOrSkipIfDbNullAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, CancellationToken, Task<T>> func,
            int index,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFieldValueOrSkipIfDbNullAsync(
                func: (x, y, z) => func(x, z),
                fieldIndex: 0,
                index: index,
                cancellationToken: cancellationToken);
        public static async Task<ValueTuple<bool, T>> TakeFirstFieldValueOrSkipIfDbNullAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, CancellationToken, Task<T>> func,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFirstFieldValueOrSkipIfDbNullAsync<T>(
                func: func,
                index: 0,
                cancellationToken: cancellationToken);
        public static async Task<ValueTuple<bool, T>> TakeFirstFieldValueOrSkipIfDbNullAsync<T>(
            this DbDataReader dbDataReader,
            int index,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFieldValueOrSkipIfDbNullAsync<T>(
                fieldIndex: 0,
                index: index,
                cancellationToken: cancellationToken);
        public static async Task<ValueTuple<bool, T>> TakeFirstFieldValueOrSkipIfDbNullAsync<T>(
            this DbDataReader dbDataReader,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFirstFieldValueOrSkipIfDbNullAsync<T>(
                index: 0,
                cancellationToken: cancellationToken);
        #endregion
        #region TakeOrDefaultAsync
        public static async Task<ValueTuple<bool, T?>> TakeFieldValueOrDefaultIfDbNullAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, CancellationToken, Task<T>> func,
            int fieldIndex,
            int index,
            CancellationToken cancellationToken = default)
            => dbDataReader.FieldCount <= fieldIndex && await dbDataReader.IsDBNullAsync(fieldIndex, cancellationToken) ? ValueTuple.Create(true, default(T)) : ValueTuple.Create<bool, T?>(true, await func(dbDataReader, fieldIndex, cancellationToken));
        public static async Task<ValueTuple<bool, T?>> TakeFieldValueOrDefaultIfDbNullAsync<T>(
            this DbDataReader dbDataReader,
            int fieldIndex,
            int index,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync(
                func: (x, y, z) => x.GetFieldValueAsync<T?>(y, z),
                fieldIndex: fieldIndex,
                index: index,
                cancellationToken: cancellationToken);
        public static async Task<ValueTuple<bool, T?>> TakeFieldValueOrDefaultIfDbNullAsync<T>(
            this DbDataReader dbDataReader,
            int fieldIndex,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T>(
                fieldIndex: fieldIndex,
                index: 0,
                cancellationToken: cancellationToken);
        public static async Task<ValueTuple<bool, T?>> TakeFirstFieldValueOrDefaultIfDbNullAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, CancellationToken, Task<T>> func,
            int index,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T>(
                func: (x, y, z) => func(x, z),
                fieldIndex: 0,
                index: index,
                cancellationToken: cancellationToken);
        public static async Task<ValueTuple<bool, T?>> TakeFirstFieldValueOrDefaultIfDbNullAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, CancellationToken, Task<T>> func,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFirstFieldValueOrDefaultIfDbNullAsync<T>(
                func: func,
                index: 0,
                cancellationToken: cancellationToken);
        public static async Task<ValueTuple<bool, T?>> TakeFirstFieldValueOrDefaultIfDbNullAsync<T>(
            this DbDataReader dbDataReader,
            int index,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T>(
                fieldIndex: 0,
                index: index,
                cancellationToken: cancellationToken);
        public static async Task<ValueTuple<bool, T?>> TakeFirstFieldValueOrDefaultIfDbNullAsync<T>(
            this DbDataReader dbDataReader,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFirstFieldValueOrDefaultIfDbNullAsync<T>(
                index: 0,
                cancellationToken: cancellationToken);
        #endregion
        #region TakeOrThrowAsync
        public static async Task<ValueTuple<bool, T>> TakeFieldValueOrThrowIfDbNullAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, CancellationToken, Task<T>> func,
            int fieldIndex,
            int index,
            CancellationToken cancellationToken = default)
            => dbDataReader.FieldCount <= fieldIndex && await dbDataReader.IsDBNullAsync(fieldIndex, cancellationToken) ? throw new DataException() : ValueTuple.Create(true, await func(dbDataReader, fieldIndex, cancellationToken));
        public static async Task<ValueTuple<bool, T>> TakeFieldValueOrThrowIfDbNullAsync<T>(
            this DbDataReader dbDataReader,
            int fieldIndex,
            int index,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFieldValueOrThrowIfDbNullAsync(
                func: (x, y, z) => x.GetFieldValueAsync<T>(y, z),
                fieldIndex: fieldIndex,
                index: index,
                cancellationToken: cancellationToken);
        public static async Task<ValueTuple<bool, T>> TakeFieldValueOrThrowIfDbNullAsync<T>(
            this DbDataReader dbDataReader,
            int fieldIndex,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFieldValueOrThrowIfDbNullAsync<T>(
                fieldIndex: fieldIndex,
                index: 0,
                cancellationToken: cancellationToken);
        public static async Task<ValueTuple<bool, T>> TakeFirstFieldValueOrThrowIfDbNullAsync<T>(
            this DbDataReader dbDataReader,
            int index,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFieldValueOrThrowIfDbNullAsync<T>(
                fieldIndex: 0,
                index: index,
                cancellationToken: cancellationToken);
        public static async Task<ValueTuple<bool, T>> TakeFirstFieldValueOrThrowIfDbNullAsync<T>(
            this DbDataReader dbDataReader,
            CancellationToken cancellationToken = default)
            => await dbDataReader.TakeFirstFieldValueOrThrowIfDbNullAsync<T>(
                index: 0,
                cancellationToken: cancellationToken);
        #endregion
        public static async Task<Dictionary<string, object?>> GetRawRowAsync(
            this DbDataReader dbDataReader,
            CancellationToken cancellationToken = default)
        {
            var row = new Dictionary<string, object?>();
            for (var fieldIndex = 0; fieldIndex < dbDataReader.FieldCount; fieldIndex++)
                row[dbDataReader.GetName(fieldIndex)] = await TakeFieldValueOrDefaultIfDbNullAsync<object>(dbDataReader, fieldIndex, cancellationToken);
            return row;
        }
        public static async Task<TCollection> AddRangeAsync<TCollection, T>(
            this DbDataReader dbDataReader,
            TCollection collection,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>
        {
            var index = 0;
            while (await dbDataReader.ReadAsync(cancellationToken))
            {
                var r = constructorByIndex is null ? await dbDataReader.TakeFirstFieldValueOrSkipIfDbNullAsync<T>(index, cancellationToken) : await constructorByIndex(dbDataReader, index, cancellationToken);
                if (r.Item1)
                    collection.Add(r.Item2);
            }
            return collection;
        }
        public static async Task<TCollection> AddRangeAsync<TCollection, T>(
            this DbDataReader dbDataReader,
            TCollection collection,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>
            => await dbDataReader.AddRangeAsync<TCollection, T>(
                collection: collection,
                constructorByIndex: constructor is null ? null : async (x, _, y) => await constructor(x, y),
                cancellationToken: cancellationToken);
        public static async Task<TCollection> AddRangeAsync<TCollection, T>(
            this DbDataReader dbDataReader,
            TCollection collection,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>
            => await dbDataReader.AddRangeAsync<TCollection, T>(
                collection: collection,
                constructorByIndex: constructorByIndex is null ? null : (x, y, z) => Task.FromResult(constructorByIndex(x, y)),
                cancellationToken: cancellationToken);
        public static async Task<TCollection> AddRangeAsync<TCollection, T>(
            this DbDataReader dbDataReader,
            TCollection collection,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>
            => await dbDataReader.AddRangeAsync<TCollection, T>(
                collection: collection,
                constructorByIndex: constructor is null ? null : (x, _) => constructor(x),
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetCollectionAsync<TCollection, T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
        {
            var result = new TCollection();
            await dbDataReader.AddRangeAsync(result, constructorByIndex);
            return result;
        }
        public static async Task<TCollection> GetCollectionAsync<TCollection, T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbDataReader.GetCollectionAsync<TCollection, T>(
                constructorByIndex: constructor is null ? null : (dbDataReader, index, cancellationToken) => constructor(dbDataReader, cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<TCollection> GetCollectionAsync<TCollection, T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbDataReader.GetCollectionAsync<TCollection, T>(
                constructorByIndex: constructorByIndex is null ? null : (x, y, z) => Task.FromResult(constructorByIndex(x, y)));
        public static async Task<TCollection> GetCollectionAsync<TCollection, T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            CancellationToken cancellationToken = default)
            where TCollection : ICollection<T>, new()
            => await dbDataReader.GetCollectionAsync<TCollection, T>(
                constructorByIndex: constructor is null ? null : (x, y, z) => Task.FromResult(constructor(x)),
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetListAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetCollectionAsync<List<T>, T>(
                constructorByIndex: constructorByIndex,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetListAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetCollectionAsync<List<T>, T>(
                constructor: constructor,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetListAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetCollectionAsync<List<T>, T>(
                constructorByIndex: constructorByIndex,
                cancellationToken: cancellationToken);
        public static async Task<List<T>> GetListAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetCollectionAsync<List<T>, T>(
                constructor: constructor,
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetDictionaryAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, CancellationToken, Task<ValueTuple<bool, T>>>? constructorByIndex = null,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetCollectionAsync<Dictionary<int, T>, KeyValuePair<int, T>>(
                constructorByIndex: async (dbDataReader, index, cancellationToken) =>
                {
                    if (constructorByIndex is null)
                    {
                        return ValueTuple.Create(await dbDataReader.IsDBNullAsync(0, cancellationToken), KeyValuePair.Create(index, (T)dbDataReader.GetValue(0)));
                    }
                    else
                    {
                        var r = await constructorByIndex(dbDataReader, index, cancellationToken);
                        return ValueTuple.Create(r.Item1, KeyValuePair.Create(index, r.Item2));
                    }
                },
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetDictionaryAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, CancellationToken, Task<ValueTuple<bool, T>>>? constructor,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetDictionaryAsync<T>(
                constructorByIndex: constructor is null ? null : (dbDataReader, index, cancellationToken) => constructor(dbDataReader, cancellationToken),
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetDictionaryAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, int, ValueTuple<bool, T>>? constructorByIndex,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetDictionaryAsync<T>(
                constructorByIndex: constructorByIndex is null ? null : (x, y, z) => Task.FromResult(constructorByIndex(x, y)),
                cancellationToken: cancellationToken);
        public static async Task<Dictionary<int, T>> GetDictionaryAsync<T>(
            this DbDataReader dbDataReader,
            Func<DbDataReader, ValueTuple<bool, T>>? constructor,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetDictionaryAsync<T>(
                constructorByIndex: constructor is null ? null : (x, y, z) => Task.FromResult(constructor(x)),
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
                constructorByIndex: async (dbDataReader, index, cancellationToken) => ValueTuple.Create(true, await dbDataReader.GetRawRowAsync(cancellationToken)),
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?>>> GetTuplesAsync<T1, T2>(
            this DbDataReader dbDataReader,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetListAsync(
                constructor: async (dbDataReader, cancellationToken) => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T1>(0, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T2>(1, cancellationToken)).Item2)),
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?>>> GetTuplesAsync<T1, T2, T3>(
            this DbDataReader dbDataReader,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetListAsync(
                constructor: async (dbDataReader, cancellationToken) => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T1>(0, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T2>(1, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T3>(2, cancellationToken)).Item2)),
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?>>> GetTuplesAsync<T1, T2, T3, T4>(
            this DbDataReader dbDataReader,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetListAsync(
                constructor: async (dbDataReader, cancellationToken) => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T1>(0, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T2>(1, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T3>(2, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T4>(3, cancellationToken)).Item2)),
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?>>> GetTuplesAsync<T1, T2, T3, T4, T5>(
            this DbDataReader dbDataReader,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetListAsync(
                constructor: async (dbDataReader, cancellationToken) => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T1>(0, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T2>(1, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T3>(2, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T4>(3, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T5>(4, cancellationToken)).Item2)),
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6>(
            this DbDataReader dbDataReader,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetListAsync(
                constructor: async (dbDataReader, cancellationToken) => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T1>(0, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T2>(1, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T3>(2, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T4>(3, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T5>(4, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T6>(5, cancellationToken)).Item2)),
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7>(
            this DbDataReader dbDataReader,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetListAsync(
                constructor: async (dbDataReader, cancellationToken) => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T1>(0, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T2>(1, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T3>(2, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T4>(3, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T5>(4, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T6>(5, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T7>(6, cancellationToken)).Item2)),
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?>>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8>(
            this DbDataReader dbDataReader,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetListAsync(
                constructor: async (dbDataReader, cancellationToken) => ValueTuple.Create(
                    true,
                    Tuple.Create(
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T1>(0, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T2>(1, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T3>(2, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T4>(3, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T5>(4, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T6>(5, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T7>(6, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T8>(7, cancellationToken)).Item2)),
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
            this DbDataReader dbDataReader,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetListAsync(
                constructor: async (dbDataReader, cancellationToken) => ValueTuple.Create(
                    true,
                    new Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?>>(
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T1>(0, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T2>(1, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T3>(2, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T4>(3, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T5>(4, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T6>(5, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T7>(6, cancellationToken)).Item2,
                        Tuple.Create(
                            (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T8>(7, cancellationToken)).Item2,
                            (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T9>(8, cancellationToken)).Item2))),
                cancellationToken: cancellationToken);
        public static async Task<List<Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>>> GetTuplesAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
            this DbDataReader dbDataReader,
            CancellationToken cancellationToken = default)
            => await dbDataReader.GetListAsync(
                constructor: async (dbDataReader, cancellationToken) => ValueTuple.Create(
                    true,
                    new Tuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, Tuple<T8?, T9?, T10?>>(
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T1>(0, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T2>(1, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T3>(2, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T4>(3, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T5>(4, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T6>(5, cancellationToken)).Item2,
                        (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T7>(6, cancellationToken)).Item2,
                        Tuple.Create(
                            (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T8>(7, cancellationToken)).Item2,
                            (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T9>(8, cancellationToken)).Item2,
                            (await dbDataReader.TakeFieldValueOrDefaultIfDbNullAsync<T10>(9, cancellationToken)).Item2))),
                cancellationToken: cancellationToken);
        #endregion
    }
}
