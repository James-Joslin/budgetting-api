using System;
using System.Data;
using System.Globalization;

namespace financesApi.utilities
{
    public static class DataEditor
    {
        public static Dictionary<string, List<List<string>>> ConvertData(DataTable dataTable)
        {
            var headers = dataTable.Columns.Cast<DataColumn>()
                .Select(column => column.ColumnName)
                .ToList();

            var rows = dataTable.AsEnumerable()
                .Select(row => dataTable.Columns.Cast<DataColumn>()
                    .Select(column => row[column]?.ToString() ?? string.Empty)
                    .ToList())
                .ToList();

            var result = new Dictionary<string, List<List<string>>>
            {
                { "Headers", new List<List<string>> { headers } },
                { "Rows", rows }
            };

            return result;
        }
    }
}