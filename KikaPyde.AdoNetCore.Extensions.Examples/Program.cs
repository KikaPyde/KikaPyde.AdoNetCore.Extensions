using System.Data.Common;

namespace KikaPyde.AdoNetCore.Extensions.Examples
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var code = Enum.Parse<ExampleDbProviderCode>(args[0]);
                var builder = code.CreateDbConnectionStringBuilder(args[1]);
                var result1 = builder.GetList<int>(
                    x => x.ChangeCommandText(
                        sqlite: args[2]));
                Console.WriteLine(string.Join("; ", result1));
                var result2 = builder.Using(
                    x =>
                    {
                        var r1 = x.GetList<int>(
                            x => x.CommandText = args[2]);
                        var r2 = x.GetList<int>(
                            x => x.CommandText = args[3]);
                        return r1.Concat(r2).ToList();
                    });
                Console.WriteLine(string.Join("; ", result2));
                var result3 = builder.ExecuteNonQuery(
                    x => x.CommandText = args[4],
                    usingOptions: new UsingOptions
                    {
                        AllowTransactionCommitOnSuccessTryFunc = true,
                    });
                Console.WriteLine(result3);
                var result4 = builder.Using(
                    (DbConnection _, DbCommand dbCommand) =>
                    {
                        dbCommand.CommandText = args[2];
                        var r1 = dbCommand.GetList<int>();
                        dbCommand.CommandText = args[3];
                        var r2 = dbCommand.GetList<int>();
                        return r1.Concat(r2).ToList();
                    });
                Console.WriteLine(string.Join("; ", result4));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        static async Task MainAsync(string[] args)
        {
            try
            {
                var code = Enum.Parse<ExampleDbProviderCode>(args[0]);
                var builder = code.CreateDbConnectionStringBuilder(args[1]);
                var result1 = await builder.GetListAsync<int>(
                    x => x.ChangeCommandText(
                        sqlite: args[2]));
                Console.WriteLine(string.Join("; ", result1));
                var result2 = await builder.UsingAsync(
                    async (x, _) =>
                    {
                        var r1 = await x.GetListAsync<int>(
                            x => x.CommandText = args[2]);
                        var r2 = await x.GetListAsync<int>(
                            x => x.CommandText = args[3]);
                        return r1.Concat(r2).ToList();
                    });
                Console.WriteLine(string.Join("; ", result2));
                var result3 = await builder.ExecuteNonQueryAsync(
                    x => x.CommandText = args[4],
                    usingOptions: new UsingOptions
                    {
                        AllowTransactionCommitOnSuccessTryFunc = true,
                    });
                Console.WriteLine(result3);
                var result4 = await builder.UsingAsync(
                    async (DbConnection _, DbCommand dbCommand, CancellationToken _) =>
                    {
                        dbCommand.CommandText = args[2];
                        var r1 = await dbCommand.GetListAsync<int>();
                        dbCommand.CommandText = args[3];
                        var r2 = await dbCommand.GetListAsync<int>();
                        return r1.Concat(r2).ToList();
                    });
                Console.WriteLine(string.Join("; ", result4));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
