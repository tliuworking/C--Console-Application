using System.Linq;
using System.Collections.Generic;
using WDTAssignmentOne.Utilities;

namespace WDTAssignmentOne
{
    public class StockRequestManager
    {
        public List<StoreInventory> Items { get; private set; }

        public void Initialise(int storeID)
        {
            using (var connection = Program.ConnectionString.CreateConnection())
            {
                var command = connection.CreateCommand();
                command.CommandText =
                    @"SELECT StoreInventory.*, Product.Name
                    FROM StoreInventory INNER JOIN Product
                    ON StoreInventory.ProductID = Product.ProductID
                    WHERE StoreInventory.ProductID != 0 AND StoreID = @storeID
                    ORDER BY StoreInventory.StoreID";
                command.Parameters.AddWithValue("storeID", storeID);

                Items = command.GetDataTable().Select().Select(x =>
                    new StoreInventory((int)x["ProductID"], (string)x["Name"], (int)x["StockLevel"])).ToList();
            }
        }

        public StoreInventory GetItem(int productID) => Items.FirstOrDefault(x => x.ProductID == productID);

    }
}
