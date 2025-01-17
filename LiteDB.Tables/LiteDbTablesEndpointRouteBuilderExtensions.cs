using LiteDB.Tables.Models;
using LiteDB.Tables.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LiteDB.Tables;

public static class LiteDbTablesEndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapLiteDbTables(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/litedb/tables");

        group.MapGet("/", async (HttpContext context, LiteDbService _liteDbService) =>
        {
            var dbInfo = _liteDbService.GetDbInfo();
            var tables = _liteDbService.GetTables();
            if(tables == null || !tables.Any())
            {
                await context.Response.WriteAsync("<h1>No tables found</h1>");
                return;
            }
            var html = @"
        <!DOCTYPE html>
        <html>
        <head>
            <title>LiteDb.Tables</title>
            <style>
                body {
                    font-family: Arial, sans-serif;
                    margin: 20px;
                }
                .table-info {
                    margin-bottom: 20px;
                }
                .table-info p {
                    margin: 5px 0;
                }
                table {
                    border-collapse: collapse;
                    width: 100%;
                    margin-top: 20px;
                    table-layout: fixed;
                    resize: both;
                    overflow: auto;
                }
                th, td {
                    border: 1px solid #ddd;
                    padding: 8px;
                    text-align: left;
                    overflow: hidden;
                }
                th {
                    background-color: #f4f4f4;
                    cursor: pointer;
                }
                th:hover {
                    background-color: #ddd;
                }
                tr:nth-child(even) {
                    background-color: #f9f9f9;
                }
                tr:hover {
                    background-color: #f1f1f1;
                }
                a {
                    text-decoration: none;
                    color: #007bff;
                }
                a:hover {
                    text-decoration: underline;
                }
            </style>
        </head>
        <body>
            <h1>Database Tables</h1> 
            <table>
                <thead>
                    <tr>
                        <th>Table Name</th>
                        <th>Row Count</th>
                        <th>Connection String</th>
                    </tr>
                </thead>
                <tbody>
        ";

            foreach (var table in tables)
            {
                var rowCount = _liteDbService.GetTableData(table.Name!).Rows.Count;
                html += $"<tr><td>{table.Name}<a style='margin:4px' href='/litedb/{table.Name}/html'>html</a> <a href='/litedb/{table.Name}/json'>json</a> </td>  <td>{rowCount} rows</td><td>{table.ConnectionString}</td></tr>";
            }

            html += @"
                </tbody>
            </table>
        </body>
        </html>
        ";

            await context.Response.WriteAsync(html);
        });

        group.MapGet("/json", async (HttpContext context, LiteDbService _liteDbService) =>
        {
            var tables = _liteDbService.GetTables();

            if(tables == null || !tables.Any())
            {
                await context.Response.WriteAsJsonAsync("No tables found");
                return;
            }   

            var tableInfo = tables.Select(t => new
            {
                t.Name,
                RowCount = _liteDbService.GetTableData(t.Name!).Rows.Count
            });

            await context.Response.WriteAsJsonAsync(tableInfo);
        });

        app.MapGet("/litedb/{tableName}/html", async (string tableName, HttpContext Context, LiteDbService _liteDbService) =>
        {
            var tableData = _liteDbService.GetTableData(tableName);

            if(tableData.Rows.Count == 0)
            {
                await Context.Response.WriteAsync("<h1>No data found</h1>");
                return;
            }

            var html = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='UTF-8'>
            <title>{tableName}</title>
<link href=""https://fonts.googleapis.com/css2?family=Inter:wght@400;600&display=swap"" rel=""stylesheet"">
            <style>
root {{
--primary-color: #007bff;
            --primary-hover: #0056b3;
            --background-color: #f8f9fa;
            --text-color: #333;
            --border-color: #dee2e6;
}}

         

                body {{
                    font-family: 'Inter', sans-serif;
                     line-height: 1.6;
            color: var(--text-color);
            background-color: var(--background-color);
            margin: 0;
            padding: 20px;
                }}
                .table-info {{
                    margin-bottom: 20px;
                }}
                .table-info p {{
                    margin: 5px 0;
                }}
                table {{
                    border-collapse: collapse;
                    width: 100%;
                    margin-top: 20px;

                    resize: both;
                    overflow: auto;
                }}
                th, td {{
                    border: 1px solid #ddd;
                    padding: 8px;
                    text-align: left;

                    overflow: hidden;
                }}
td{{resize: both; }}
                th {{
                    background-color: #f4f4f4;
                    cursor: pointer;
                }}
                th:hover {{
                    background-color: #ddd;
                }} 
                tr:nth-child(even) {{
                    background-color: #f9f9f9;
                }}
                tr:hover {{
                    background-color: #f1f1f1;
                }}

                .buttons {{
                    display: flex;
                    justify-content: end;
                    items-align: center;
gap: 10px;
                    margin-top: 20px;
                }}
                .buttons button {{
                    margin-right: 10px;
                    padding: 10px 12px;
                    border: none;
                    background-color: #007bff;
                    color: white;
                    cursor: pointer;
                    border-radius: 5px;
float: right;
                }}
                .buttons button:hover {{
                    background-color: #0056b3;
                }}
                
                #searchInput {{
                    margin-top: 10px;
                    margin-bottom: 10px;
                    padding: 8px;
                    width: 100%;
                    font-size: 16px;
                    border: 1px solid #ddd;
                    border-radius: 5px;
                }}
            </style>
            <script src='https://cdnjs.cloudflare.com/ajax/libs/xlsx/0.18.5/xlsx.full.min.js'></script>
             <script>
                function sortTable(n) {{
                    const table = document.getElementById('dataTable');
                    let rows = Array.from(table.rows).slice(1);
                    let isAscending = table.getAttribute('data-sort-direction') === 'asc';

                    rows.sort((rowA, rowB) => {{
                        let cellA = rowA.cells[n].innerText.trim();
                        let cellB = rowB.cells[n].innerText.trim();
                        let a = isNaN(cellA) ? cellA : parseFloat(cellA);
                        let b = isNaN(cellB) ? cellB : parseFloat(cellB);
                        return isAscending ? (a > b ? 1 : -1) : (a < b ? 1 : -1);
                    }});

                    rows.forEach(row => table.tBodies[0].appendChild(row));
                    table.setAttribute('data-sort-direction', isAscending ? 'desc' : 'asc');
                }}

                function exportToExcel() {{
                    const table = document.getElementById('dataTable');
                    const wb = XLSX.utils.table_to_book(table, {{ sheet: '{tableName}' }});
                    XLSX.writeFile(wb, '{tableName}.xlsx');
                }}

                function searchTable() {{
                    const input = document.getElementById('searchInput');
                    const filter = input.value.toLowerCase();
                    const table = document.getElementById('dataTable');
                    const rows = table.getElementsByTagName('tr');

                    for (let i = 1; i < rows.length; i++) {{
                        let row = rows[i];
                        let cells = row.getElementsByTagName('td');
                        let match = false;

                        for (let j = 0; j < cells.length; j++) {{
                            if (cells[j].innerText.toLowerCase().includes(filter)) {{
                                match = true;
                                break;
                            }}
                        }}

                        row.style.display = match ? '' : 'none';
                    }}
                }}
            </script>
        </head>
        <body>
            <div class='table-info'>
<p><a href='/litedb/tables'>Back to Tables</a></p>
                <h1 style='font-style:italic'> {tableName}</h1>
                <p><strong>Row Count:</strong> {tableData.Rows.Count}</p>
                <p><strong>Columns:</strong> {string.Join(", ", tableData.Columns)}</p> 
                <p><strong>Last Modified:</strong> {tableData.LastModified}</p>
            </div>
            <div class='buttons'>
                <a href='/litedb/{tableName}/json' >JSON</a>
                <button onclick='exportToExcel()'>Export to Excel</button>
            </div>
            <input type='text' id='searchInput' onkeyup='searchTable()' placeholder='Search table...'>
            <table id='dataTable' data-sort-direction='asc'>
                <thead>
                    <tr>
        ";

            foreach (var column in tableData.Columns)
            {
                html += $"<th onclick='sortTable({tableData.Columns.IndexOf(column)})'>{column} <div class='resize-handle'></div>  ↕ </th>";
            }

            html += @"
                    </tr>
                </thead>
                <tbody>
        ";

            foreach (var row in tableData.Rows)
            {
                var rowId = row["_id"].ToString();

                html += "<tr>";

                foreach (var column in tableData.Columns)
                {
                    var cellValue = row.ContainsKey(column) ? row[column].RawValue?.ToString() ?? "" : "";
                    html += $"<td>{cellValue}</td>";
                }

                html += "</tr>";
            }

            html += @"
                </tbody>
            </table>
        </body>
        </html>
        ";

            await Context.Response.WriteAsync(html);
        });

        app.MapGet("/litedb/{tableName}/json", async (string tableName, HttpContext Context, LiteDbService _liteDbService) =>
        {
            var tableData = _liteDbService.GetTableData(tableName);

            if(tableData.Rows.Count == 0)
            {
                await Context.Response.WriteAsJsonAsync("No data found");
                return;
            }

            var tableDetails = new
            {
                TableName = tableName,
                RowCount = tableData.Rows.Count,
                ColumnCount = tableData.Columns.Count,
                tableData.Columns,
                Rows = tableData.Rows.Select(row => row.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.RawValue.ToString())).ToList(),
                tableData.LastModified
            };

            await Context.Response.WriteAsJsonAsync(tableDetails);
        });

        return app;
    }
}
