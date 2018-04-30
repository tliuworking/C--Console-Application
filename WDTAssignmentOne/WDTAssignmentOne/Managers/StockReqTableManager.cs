using System.Linq;
using System.Collections.Generic;
using WDTAssignmentOne.Utilities;

namespace WDTAssignmentOne
{
    public class StockReqTableManager
    {
        public List<StockRequestTable> Items { get; private set; }

        public void InitialiseTable()
        {
            using (var connection = Program.ConnectionString.CreateConnection())
            {
                var command = connection.CreateCommand();
                command.CommandText =
                    @"Select StockRequest.StockRequestID, StockRequest.ProductID, StockRequest.StoreID, Store.Name, Product.Name, 
                    StockRequest.Quantity, OwnerInventory.StockLevel
                    FROM StockRequest 
                        INNER JOIN Product 
                        ON StockRequest.ProductID = Product.ProductID
                        INNER JOIN OwnerInventory
                        ON StockRequest.ProductID = OwnerInventory.ProductID
                        INNER JOIN Store
                        ON StockRequest.StoreID = Store.StoreID
                    WHERE StockRequest.ProductID != 0
                    ORDER BY StockRequest.ProductID";

                Items = command.GetDataTable().Select().Select(x =>
                    new StockRequestTable((int)x[0], (int)x[1], (int) x[2],(string)x[3], (string)x[4], (int)x[5], (int)x[6])).ToList();
            }
        }

        public StockRequestTable GetItem(int productID) => Items.FirstOrDefault(x => x.ProductID == productID);
    }
}
