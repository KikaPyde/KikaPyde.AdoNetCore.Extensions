using Moq;
using System.Data.Common;
using System.Text;
using Xunit;

namespace KikaPyde.AdoNetCore.Extensions.Tests
{
    public class DbConnectionExtensionsTests : Tests
    {
        private Mock<DbConnection> CreateDbConnection()
        {
            var mock = new Mock<DbConnection>();
            mock.Setup(x => x.State).Returns(System.Data.ConnectionState.Open);
            return mock;
        }
        [Fact]
        public void Using_TryFunc_Success_NoCatchFunc()
        {
            var dbConnection = CreateDbConnection();
            var tryFuncCalled = false;
            var result = dbConnection.Object.Using(
                x =>
                {
                    tryFuncCalled = true;
                    return true;
                });
            Assert.True(tryFuncCalled);
            Assert.True(result);
        }
        [Fact]
        public void Using_TryFunc_Fail_NoCatchFunc()
        {
            var dbConnection = CreateDbConnection();
            var tryFuncCalled = false;
            var result = (bool?)null;
            Assert.Throws<TryFuncException>(
                () =>
                {
                    result = dbConnection.Object.Using<bool>(
                        x =>
                        {
                            tryFuncCalled = true;
                            throw new TryFuncException();
                        });
                });
            Assert.True(tryFuncCalled);
            Assert.Null(result);
        }
        [Fact]
        public void Using_TryFunc_Fail_CatchFunc_Success()
        {
            var dbConnection = CreateDbConnection();
            var tryFuncCalled = false;
            var catchFuncCalled = false;
            var result = dbConnection.Object.Using(
                x =>
                {
                    tryFuncCalled = true;
                    throw new TryFuncException();
                },
                (x, e) =>
                {
                    catchFuncCalled = true;
                    return true;
                });
            Assert.True(tryFuncCalled);
            Assert.True(catchFuncCalled);
            Assert.True(result);
        }
        [Fact]
        public void Using_TryFunc_Fail_CatchFunc_Fail()
        {
            var dbConnection = CreateDbConnection();
            var tryFuncCalled = false;
            var catchFuncCalled = false;
            var result = (bool?)null;
            Assert.Throws<AggregateException>(
                () =>
                {
                    result = dbConnection.Object.Using<bool>(
                        x =>
                        {
                            tryFuncCalled = true;
                            throw new TryFuncException();
                        },
                        (x, e) =>
                        {
                            catchFuncCalled = true;
                            throw new CatchFuncException();
                        });
                },
                x =>
                {
                    var count = x.InnerExceptions.Count;
                    if (count == 2)
                    {
                        var result = new StringBuilder();
                        var tryFuncException = x.InnerExceptions[0] as TryFuncException;
                        var catchFuncException = x.InnerExceptions[1] as CatchFuncException;
                        if (tryFuncException is null)
                            result.AppendLine("Invalid exception for the tryFunc");
                        if (catchFuncException is null)
                            result.AppendLine("Invalid exception for the catchFunc");
                        return result.Length == 0
                            ? null
                            : result.ToString();
                    }
                    return "Invalid exception count";
                });
            Assert.True(tryFuncCalled);
            Assert.True(catchFuncCalled);
            Assert.Null(result);
        }
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task UsingAsync_TryFunc_Success_NoCatchFunc(
            bool cancel)
        {
            var dbConnection = CreateDbConnection();
            using var cts = new CancellationTokenSource();
            var tryFuncCalled = false;
            var result = (bool?)null;
            async Task test()
            {
                if (cancel)
                    cts.CancelAfter(CancellationDelay);
                result = await dbConnection.Object.UsingAsync(
                    async (_, ct) =>
                    {
                        tryFuncCalled = true;
                        if (cancel)
                            await Task.Delay(Delay, ct);
                        return await Task.FromResult(true);
                    },
                    cancellationToken: cts.Token);
            }
            if (cancel)
            {
                await Assert.ThrowsAsync<TaskCanceledException>(test);
                Assert.True(tryFuncCalled);
                Assert.Null(result);
            }
            else
            {
                await test();
                Assert.True(tryFuncCalled);
                Assert.True(result);
            }
        }
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task UsingAsync_TryFunc_Fail_NoCatchFunc(
            bool cancel)
        {
            var dbConnection = CreateDbConnection();
            using var cts = new CancellationTokenSource();
            var tryFuncCalled = false;
            var result = (bool?)null;
            async Task test()
            {
                if (cancel)
                    cts.CancelAfter(CancellationDelay);
                result = await dbConnection.Object.UsingAsync<bool>(
                    tryFunc: async (_, ct) =>
                    {
                        tryFuncCalled = true;
                        if (cancel)
                            await Task.Delay(Delay, ct);
                        throw new TryFuncException();
                    },
                    cancellationToken: cts.Token);
            }
            if (cancel)
                await Assert.ThrowsAsync<TaskCanceledException>(test);
            else
                await Assert.ThrowsAsync<TryFuncException>(test);
            Assert.True(tryFuncCalled);
            Assert.Null(result);
        }
        [Theory]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(true, true, false)]
        [InlineData(true, false, true)]
        [InlineData(true, true, true)]
        public async Task UsingAsync_TryFunc_Fail_CatchFunc_Success(
            bool cancel,
            bool delayTryFunc,
            bool delayCatchFunc)
        {
            var dbConnection = CreateDbConnection();
            using var cts = new CancellationTokenSource();
            var tryFuncCalled = false;
            var catchFuncCalled = false;
            var result = (bool?)null;
            async Task test()
            {
                if (cancel)
                    cts.CancelAfter(CancellationDelay);
                result = await dbConnection.Object.UsingAsync(
                    tryFunc: async (_, ct) =>
                    {
                        tryFuncCalled = true;
                        if (delayTryFunc)
                            await Task.Delay(Delay, ct);
                        throw new TryFuncException();
                    },
                    catchFunc: async (_, _, ct) =>
                    {
                        catchFuncCalled = true;
                        if (delayCatchFunc)
                            await Task.Delay(Delay, ct);
                        return true;
                    },
                    cancellationToken: cts.Token);
            }
            async Task testThrows()
            {
                await Assert.ThrowsAsync<AggregateException>(test);
            }
            if (cancel)
            {
                if (delayTryFunc && delayCatchFunc)
                {
                    await testThrows();
                    Assert.True(tryFuncCalled);
                    Assert.True(catchFuncCalled);
                    Assert.Null(result);
                }
                else if (delayTryFunc)
                {
                    await test();
                    Assert.True(tryFuncCalled);
                    Assert.True(catchFuncCalled);
                    Assert.True(result);
                }
                else if (delayCatchFunc)
                {
                    await testThrows();
                    Assert.True(tryFuncCalled);
                    Assert.True(catchFuncCalled);
                    Assert.Null(result);
                }
                else
                {
                    await test();
                    Assert.True(tryFuncCalled);
                    Assert.True(catchFuncCalled);
                    Assert.True(result);
                }
            }
            else
            {
                await test();
                Assert.True(tryFuncCalled);
                Assert.True(catchFuncCalled);
                Assert.True(result);
            }
        }
        [Theory]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(true, true, false)]
        [InlineData(true, false, true)]
        [InlineData(true, true, true)]
        public async Task UsingAsync_TryFunc_Fail_CatchFunc_Fail(
            bool cancel,
            bool delayTryFunc,
            bool delayCatchFunc)
        {
            var dbConnection = CreateDbConnection();
            using var cts = new CancellationTokenSource();
            var tryFuncCalled = false;
            var catchFuncCalled = false;
            var result = (bool?)null;
            async Task test()
            {
                if (cancel)
                    cts.CancelAfter(CancellationDelay);
                result = await dbConnection.Object.UsingAsync<bool>(
                    tryFunc: async (_, ct) =>
                    {
                        tryFuncCalled = true;
                        if (delayTryFunc)
                            await Task.Delay(Delay, ct);
                        throw new TryFuncException();
                    },
                    catchFunc: async (_, _, ct) =>
                    {
                        catchFuncCalled = true;
                        if (delayCatchFunc)
                            await Task.Delay(Delay, ct);
                        throw new CatchFuncException();
                    },
                    cancellationToken: cts.Token);
            }
            async Task testThrows()
            {
                await Assert.ThrowsAsync<AggregateException>(test);
            }
            await testThrows();
            Assert.True(tryFuncCalled);
            Assert.True(catchFuncCalled);
            Assert.Null(result);
        }
    }
}
