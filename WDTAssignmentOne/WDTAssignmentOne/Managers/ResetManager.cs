using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using WDTAssignmentOne.Utilities;

namespace WDTAssignmentOne
{
    public class ResetManager
    {
        public List<OwnerInventory> Items { get; }

        public ResetManager()
        {
            using (var connection = Program.ConnectionString.CreateConnection())
            {
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Product.*, OwnerInventory.StockLevel " +
                                      "FROM Product INNER JOIN OwnerInventory " +
                                      "ON Product.ProductID = OwnerInventory.ProductID " +
                                      "WHERE Product.ProductID <> 0 " +
                                      "ORDER BY Product.ProductID";

                Items = command.GetDataTable().Select().Select(x =>
                    new OwnerInventory((int)x["ProductID"], (string)x["Name"],(int)x["StockLevel"])).ToList();
            }
        }

        public OwnerInventory GetItem(int productID) => Items.FirstOrDefault(x => x.ProductID == productID);

        public void UpdateStockLevel(OwnerInventory item)
        {
            using (var connection = Program.ConnectionString.CreateConnection())
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "update OwnerInventory set StockLevel = @stockLevel where ProductID = @productID";
                command.Parameters.AddWithValue("stockLevel", item.StockLevel);
                command.Parameters.AddWithValue("productID", item.ProductID);

                command.ExecuteNonQuery();
            }
        }

    }
}
