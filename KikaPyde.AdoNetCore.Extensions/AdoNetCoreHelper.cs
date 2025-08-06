namespace KikaPyde.AdoNetCore.Extensions
{
    public static partial class AdoNetCoreHelper
    {
        public static readonly IUsingOptions DefaultUsingOptions = new UsingOptions();
        private static T TryCatchFinally<T>(
            Func<T> tryFunc,
            Func<Exception, T>? catchFunc,
            Action finallyAction)
        {
            try
            {
                return tryFunc();
            }
            catch (Exception tryException)
            {
                if (catchFunc is not null)
                {
                    try
                    {
                        return catchFunc(tryException);
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
                finallyAction();
            }
        }
        private static async Task<T> TryCatchFinallyAsync<T>(
            Func<CancellationToken, Task<T>> tryFunc,
            Func<Exception, CancellationToken, Task<T>>? catchFunc,
            Func<Task> finallyFunc,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await tryFunc(cancellationToken);
            }
            catch (Exception tryException)
            {
                if (catchFunc is not null)
                {
                    try
                    {
                        return await catchFunc(tryException, cancellationToken);
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
                await finallyFunc();
            }
        }
    }
}