using System;
using System.Collections.Generic;
using System.Text;

namespace WDTAssignmentOne
{
    class StockRequest
    {
        public int StockRequestID { get; }
        public int StoreID { get; }
        public int ProductID { get; }
        public int Quantity { get; set; }

        public StockRequest(int stockRequestId, int storeId, int proID, int quan)
        {
            StockRequestID = stockRequestId;
            StoreID = storeId;
            ProductID = proID;
            Quantity = quan;
        }
    }
}
