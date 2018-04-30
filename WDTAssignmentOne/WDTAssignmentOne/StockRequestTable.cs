using System;
using System.Collections.Generic;
using System.Text;

namespace WDTAssignmentOne
{
    public class StockRequestTable
    {
        public int StockRequestId { get; }
        public int ProductID { get; }
        public int StoreID { get; }
        public string StoreName { get; }
        public string ProductName { get; }
        public int Quantity { get; set; }
        public int CurrentStock { get; set; }

        public StockRequestTable(int stockRequestId, int ProduectID, int storeID, string storeName, string productName, int quantity, int currentStock)
        {
            StockRequestId = stockRequestId;
            ProductID = ProduectID;
            StoreID = storeID;
            StoreName = storeName;
            ProductName = productName;
            Quantity = quantity;
            CurrentStock = currentStock;
        }
    }
}
