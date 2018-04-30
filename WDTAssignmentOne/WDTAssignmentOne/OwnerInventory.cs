using System;
using System.Collections.Generic;
using System.Text;

namespace WDTAssignmentOne
{
    public class OwnerInventory
    {
        public int ProductID { get; }
        public string Name { get; }
        public int StockLevel { get; set; }

        public OwnerInventory(int produectID, string name, int stockLevel)
        {
            this.ProductID = produectID;
            this.Name = name;
            this.StockLevel = stockLevel;
        }


    }
}
