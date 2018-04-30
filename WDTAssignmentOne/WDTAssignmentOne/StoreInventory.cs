using System;
using System.Collections.Generic;
using System.Text;

namespace WDTAssignmentOne
{
    public class StoreInventory
    {
        public int ProductID { get; }
        public string Name { get; }
        public int StockLevel { get; }

        public StoreInventory(int productId, string name, int stockLevel)
        {
            this.ProductID = productId;
            this.Name = name;
            this.StockLevel = stockLevel;
        }
        
    }
}
