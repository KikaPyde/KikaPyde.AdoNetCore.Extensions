namespace KikaPyde.AdoNetCore.Extensions.Examples
{
    public enum ExampleDbProviderCode
    {
        None,
        /// <summary>
        /// Microsoft SQLite
        /// </summary>
        Sqlite,
        /// <summary>
        /// PostgreSQL
        /// </summary>
        Npgsql,
        Unsupported = -1,
    }
}
