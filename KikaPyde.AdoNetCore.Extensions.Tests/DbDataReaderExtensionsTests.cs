using Moq;
using System.Data.Common;
using Xunit;

namespace KikaPyde.AdoNetCore.Extensions.Tests
{
    public class DbDataReaderExtensionsTests
    {
        private Mock<DbDataReader> CreateDbDataReader(
            bool hasData,
            bool outOfRange,
            bool nullable)
        {
            var mock = new Mock<DbDataReader>();
            mock.Setup(x => x.FieldCount)
                .Returns(() => !hasData || outOfRange ? 0 : 1);
            mock.Setup(x => x.IsDBNull(
                It.IsAny<int>()))
                .Returns((int _) => !hasData || outOfRange ? throw new Exception() : nullable);
            mock.Setup(x => x.IsDBNullAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
                .Returns((int _, CancellationToken _) => !hasData || outOfRange ? throw new Exception() : Task.FromResult(nullable));
            mock.Setup(x => x.GetFieldValue<bool>(
                It.IsAny<int>()))
                .Returns((int _) => !hasData || nullable || outOfRange ? throw new Exception() : true);
            mock.Setup(x => x.GetFieldValueAsync<bool>(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
                .Returns((int _, CancellationToken _) => !hasData || nullable || outOfRange ? throw new Exception() : Task.FromResult(true));
            return mock;
        }
        [Theory]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(true, true, false)]
        [InlineData(true, true, true)]
        public void TakeFieldValueOrSkipIfDbNullOrOutOfRange(
            bool hasData,
            bool outOfRange,
            bool nullable)
        {
            var dbDataReader = CreateDbDataReader(hasData, outOfRange, nullable);
            var (canTake, value) = dbDataReader.Object.TakeFieldValueOrSkipIfDbNullOrOutOfRange((x, i) => x.GetFieldValue<bool>(i), 0);
            if (hasData && !outOfRange && !nullable)
            {
                Assert.True(canTake);
                Assert.True(value);
            }
            else
            {
                Assert.False(canTake);
                Assert.False(value);
            }
        }
        [Theory]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(true, true, false)]
        [InlineData(true, true, true)]
        public void TakeFieldValueOrDefaultIfDbNullOrOutOfRange(
            bool hasData,
            bool outOfRange,
            bool nullable)
        {
            var dbDataReader = CreateDbDataReader(hasData, outOfRange, nullable);
            var (canTake, value) = dbDataReader.Object.TakeFieldValueOrDefaultIfDbNullOrOutOfRange((x, i) => x.GetFieldValue<bool>(i), 0);
            if (hasData && !outOfRange && !nullable)
            {
                Assert.True(canTake);
                Assert.True(value);
            }
            else
            {
                Assert.True(canTake);
                Assert.False(value);
            }
        }
        [Theory]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(true, true, false)]
        [InlineData(true, true, true)]
        public void TakeFieldValueOrThrowIfDbNullOrOutOfRange(
            bool hasData,
            bool outOfRange,
            bool nullable)
        {
            var dbDataReader = CreateDbDataReader(hasData, outOfRange, nullable);
            if (hasData && !outOfRange && !nullable)
            {
                var (canTake, value) = dbDataReader.Object.TakeFieldValueOrThrowIfDbNullOrOutOfRange((x, i) => x.GetFieldValue<bool>(i), 0);
                Assert.True(canTake);
                Assert.True(value);
            }
            else
            {
                Assert.Throws<Exception>(
                    () =>
                    {
                        var result = dbDataReader.Object.TakeFieldValueOrThrowIfDbNullOrOutOfRange((x, i) => x.GetFieldValue<bool>(i), 0);
                    });
            }
        }
        [Theory]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(true, true, false)]
        [InlineData(true, true, true)]
        public async Task TakeFieldValueOrSkipIfDbNullOrOutOfRangeAsync(
            bool hasData,
            bool outOfRange,
            bool nullable)
        {
            var dbDataReader = CreateDbDataReader(hasData, outOfRange, nullable);
            var (canTake, value) = await dbDataReader.Object.TakeFieldValueOrSkipIfDbNullOrOutOfRangeAsync(async (x, i, ct) => await x.GetFieldValueAsync<bool>(i, ct), 0);
            if (hasData && !outOfRange && !nullable)
            {
                Assert.True(canTake);
                Assert.True(value);
            }
            else
            {
                Assert.False(canTake);
                Assert.False(value);
            }
        }
        [Theory]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(true, true, false)]
        [InlineData(true, true, true)]
        public async Task TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync(
            bool hasData,
            bool outOfRange,
            bool nullable)
        {
            var dbDataReader = CreateDbDataReader(hasData, outOfRange, nullable);
            var (canTake, value) = await dbDataReader.Object.TakeFieldValueOrDefaultIfDbNullOrOutOfRangeAsync(async (x, i, ct) => await x.GetFieldValueAsync<bool>(i, ct), 0);
            if (hasData && !outOfRange && !nullable)
            {
                Assert.True(canTake);
                Assert.True(value);
            }
            else
            {
                Assert.True(canTake);
                Assert.False(value);
            }
        }
        [Theory]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(true, true, false)]
        [InlineData(true, true, true)]
        public async Task TakeFieldValueOrThrowIfDbNullOrOutOfRangeAsync(
            bool hasData,
            bool outOfRange,
            bool nullable)
        {
            var dbDataReader = CreateDbDataReader(hasData, outOfRange, nullable);
            if (hasData && !outOfRange && !nullable)
            {
                var (canTake, value) = await dbDataReader.Object.TakeFieldValueOrThrowIfDbNullOrOutOfRangeAsync(async (x, i, ct) => await x.GetFieldValueAsync<bool>(i, ct), 0);
                Assert.True(canTake);
                Assert.True(value);
            }
            else
            {
                await Assert.ThrowsAsync<Exception>(
                    async () =>
                    {
                        var result = await dbDataReader.Object.TakeFieldValueOrThrowIfDbNullOrOutOfRangeAsync(async (x, i, ct) => await x.GetFieldValueAsync<bool>(i, ct), 0);
                    });
            }
        }
    }
}
