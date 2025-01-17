# LiteDb.Tables
- Display litedb as tables or json in **web-based interfaces**.
- Export tables to **Excel**.
- Sort, Search data.

## Usage

```bash
// LiteDb.Tables Services
builder.Services.AddLiteDbTables(options =>
{
    options.ConnectionString = "Filename=MyDatabase.db;Connection=shared;";
});

```

```bash
app.MapLiteDbTables();
```

## Endpoints

- /litedb/tables: Get a list of tables with row count and connection string.
- /litedb/tables/json: Get a list of tables in JSON format.
- /litedb/{tableName}/html: Get the table rendered as an HTML page with interactive features.
- /litedb/{tableName}/json: Get the table data in JSON format.
