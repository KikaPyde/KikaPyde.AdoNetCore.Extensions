# Usage

Add a handler for the "DbConnectionCreating" event

    AdoNetCoreHelper.DbConnectionCreating += (builder, e) =>
    {
		e.DbConnection = new SqliteConnection(builder.ConnectionString);
		e.Handled = true;
	}
Then  call the "CreateDbConnection"  method  in  your  code.

    var builder = new SqliteConnectionStringBuilder("DataSource=TestDatabase.db");
    using var dbConnection = builder.CreateDbConnection();
Or  use  any  other  method  to  get the result.

    var result = builder.GetList<int>(x => x.CommandText = "SELECT TestField FROM TestTable")
