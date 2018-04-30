using System.Data;
using System.Data.SqlClient;

namespace WDTAssignmentOne.Utilities
{
    public static class MiscellaneousExtensionUtilities
    {
        public static SqlConnection CreateConnection(this string connectionString) =>
            new SqlConnection(connectionString);

        public static DataTable GetDataTable(this SqlCommand command)
        {
            var table = new DataTable();
            new SqlDataAdapter(command).Fill(table);

            return table;
        }
    }
}
