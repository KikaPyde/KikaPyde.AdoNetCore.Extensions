using Moq;
using System.Data.Common;
using Xunit;

namespace KikaPyde.AdoNetCore.Extensions.Tests
{
    public class DbConnectionStringBuilderExtensionsTests : Tests
    {
        private void CreateDbConnection(
            DbConnectionStringBuilder builder,
            DbConnectionCreatingEventArgs e)
        {
            var mock = new Mock<DbConnection>();
            e.DbConnection = mock.Object;
            e.Handled = true;
        }
        private async Task CreateDbConnectionAsync(
            DbConnectionStringBuilder builder,
            DbConnectionCreatingAsyncEventArgs e)
        {
            await Task.Delay(Delay);
            e.CancellationToken.ThrowIfCancellationRequested();
            var mock = new Mock<DbConnection>();
            e.DbConnection = mock.Object;
            e.Handled = true;
        }
        private Mock<DbConnectionStringBuilder> CreateDbConnectionStringBuilder()
        {
            var mock = new Mock<DbConnectionStringBuilder>();
            return mock;
        }
        [Fact]
        public void CreateDbConnection_NoEvents()
        {
            AdoNetCoreHelper.DbConnectionCreating -= CreateDbConnection;
            AdoNetCoreHelper.DbConnectionCreatingAsync -= CreateDbConnectionAsync;
            var builder = CreateDbConnectionStringBuilder();
            Assert.Throws<NullReferenceException>(
                () =>
                {
                    builder.Object.CreateDbConnection();
                });
        }
        [Fact]
        public void CreateDbConnection_DbConnectionCreating()
        {
            AdoNetCoreHelper.DbConnectionCreating -= CreateDbConnection;
            AdoNetCoreHelper.DbConnectionCreatingAsync -= CreateDbConnectionAsync;
            AdoNetCoreHelper.DbConnectionCreating += CreateDbConnection;
            var builder = CreateDbConnectionStringBuilder();
            Assert.NotNull(builder.Object.CreateDbConnection());
        }
        [Fact]
        public void CreateDbConnection_DbConnectionCreatingAsync()
        {
            AdoNetCoreHelper.DbConnectionCreating -= CreateDbConnection;
            AdoNetCoreHelper.DbConnectionCreatingAsync -= CreateDbConnectionAsync;
            AdoNetCoreHelper.DbConnectionCreatingAsync += CreateDbConnectionAsync;
            var builder = CreateDbConnectionStringBuilder();
            Assert.NotNull(builder.Object.CreateDbConnection());
        }
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task CreateDbConnectionAsync_NoEvents(
            bool cancel)
        {
            AdoNetCoreHelper.DbConnectionCreating -= CreateDbConnection;
            AdoNetCoreHelper.DbConnectionCreatingAsync -= CreateDbConnectionAsync;
            var builder = CreateDbConnectionStringBuilder();
            using var cts = new CancellationTokenSource();
            if (cancel)
            {
                await Assert.ThrowsAsync<NullReferenceException>(
                    async () =>
                    {
                        cts.CancelAfter(CancellationDelay);
                        await builder.Object.CreateDbConnectionAsync(cts.Token);
                    });
            }
            else
            {
                await Assert.ThrowsAsync<NullReferenceException>(
                    async () =>
                    {
                        await builder.Object.CreateDbConnectionAsync(cts.Token);
                    });
            }
        }
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task CreateDbConnectionAsync_DbConnectionCreating(
            bool cancel)
        {
            AdoNetCoreHelper.DbConnectionCreating -= CreateDbConnection;
            AdoNetCoreHelper.DbConnectionCreatingAsync -= CreateDbConnectionAsync;
            AdoNetCoreHelper.DbConnectionCreating += CreateDbConnection;
            var builder = CreateDbConnectionStringBuilder();
            using var cts = new CancellationTokenSource();
            if (cancel)
            {
                cts.CancelAfter(CancellationDelay);
                Assert.NotNull(await builder.Object.CreateDbConnectionAsync(cts.Token));
            }
            else
            {
                Assert.NotNull(await builder.Object.CreateDbConnectionAsync(cts.Token));
            }
        }
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task CreateDbConnectionAsync_DbConnectionCreatingAsync(
            bool cancel)
        {
            AdoNetCoreHelper.DbConnectionCreating -= CreateDbConnection;
            AdoNetCoreHelper.DbConnectionCreatingAsync -= CreateDbConnectionAsync;
            AdoNetCoreHelper.DbConnectionCreatingAsync += CreateDbConnectionAsync;
            var builder = CreateDbConnectionStringBuilder();
            using var cts = new CancellationTokenSource();
            if (cancel)
            {
                await Assert.ThrowsAsync<OperationCanceledException>(
                    async () =>
                    {
                        cts.CancelAfter(CancellationDelay);
                        await builder.Object.CreateDbConnectionAsync(cts.Token);
                    });
            }
            else
            {
                Assert.NotNull(await builder.Object.CreateDbConnectionAsync(cts.Token));
            }
        }
    }
}
