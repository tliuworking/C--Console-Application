using System;
using System.Linq;
using System.Data.SqlClient;
using WDTAssignmentOne.Utilities;

namespace WDTAssignmentOne
{
    public class Owner
    {
        public ResetManager resetManager { get; } = new ResetManager();

        public StockReqTableManager stockReqTableManager { get; set; } = new StockReqTableManager();
        //public string _availability { get => _availability; set => _availability = value; }


        // the sub menu of Owner 
        public void OwnerMenu()
        {
            // owner behavours with 4 different functions
            while (true)
            {
                PrintOwnerMenu();
                if (!int.TryParse(Console.ReadLine(), out var inputOption))
                {
                    Console.WriteLine("Incorrect Format and Please input option from number 1 to 4\n");
                    continue;
                }
                /* 1 = Owner; 2 = Franchise holder; 3 = Customer; 4 = Quit */
                switch (inputOption)
                {
                    case 1:
                        DisplayAllStockRequests();
                        break;
                    case 2:
                        DisplayOwnerInventory();
                        break;
                    case 3:
                        //Reset Inventory Item Stock
                        ResetInventoryItemStock();
                        break;
                    case 4:
                        //Return to the main menu
                        Console.WriteLine();
                        return;
                    default:
                        Console.WriteLine("Invalid Input, Please select number 1,2,3,4\n");
                        break;
                }

            }
        }

        
        public void DisplayAllStockRequests()
        {
            stockReqTableManager.InitialiseTable();

            // display stock request table
            DisplayStockRequestTable();

            // owner processes requests
            ProcessRequests();
        }
        
        public void DisplayStockRequestTable()
        {
            Console.WriteLine("\nStock Request\n");
            Console.WriteLine(String.Format("{0,-4}{1,-18}{2,-22}{3,-10}{4,-16}{5,-18}",
                    "ID", "Store", "Product", "Quantity", "Current Stock", "Stock _availability"));

            foreach (var i in stockReqTableManager.Items)
            {
                    
                Console.WriteLine(String.Format("{0,-4}{1,-18}{2,-22}{3,-10}{4,-16}{5,-18}",
                    i.StockRequestId, i.StoreName, i.ProductName, i.Quantity, i.CurrentStock, i.Quantity <= i.CurrentStock));
            }
            Console.WriteLine();
        }

        public void ProcessRequests()
        {
            while (true)
            {
                // Enter Request to process
                Console.Write("Enter Request to process: ");
                var requestInput = Console.ReadLine();
                Console.WriteLine();
                if (string.IsNullOrEmpty(requestInput))
                {
                    return;
                }
                if (!int.TryParse(requestInput, out var id))
                {
                    Console.WriteLine("Invalid Input, Please input ID Number");
                    Console.WriteLine();
                    continue;
                }

                // check if id exists and make sure its available.
                var stockRequest = stockReqTableManager.Items.FirstOrDefault(x => x.StockRequestId == id);

                if (stockRequest == null)
                {
                    Console.WriteLine("Stock Request ID does not exist.");
                    return;
                }
                if (stockRequest.Quantity > stockRequest.CurrentStock)
                {
                    DeleteStockRequest(id);
                    Console.WriteLine("You do not have enough stock to fulfil the request.\n");
                    return;
                }

                // OwnerInventory update: Current Stock = Current Stock - StockReq.Quantity 
                UpdateOwnerInventory(id);

                // UpdateStoreInventory: 
                //  - create new item(new row) in the StoreInventory
                //  - StoreInventory.StockLevel = StoreInventory.StockLevel - StockReq.Quantity
                UpdateStoreInventory(stockRequest);

                // the row in the StockRequest is deleted after the request processed successfully
                DeleteStockRequest(id);
                Console.WriteLine("Done Process.\n");
            }
        }

        public void UpdateOwnerInventory(int stockRequestId)
        {
            // Owner Inventory update: OwnerInventory.StockLevel = OwnerInventory.StockLevel - StockRequest.Quantity
            using (var connection = Program.ConnectionString.CreateConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                    @"UPDATE OwnerInventory
                    SET OwnerInventory.StockLevel = OwnerInventory.StockLevel - StockRequest.Quantity
                    FROM StockRequest
                    WHERE StockRequest.StockRequestId = @stockRequestId AND
                    StockRequest.ProductID = OwnerInventory.ProductID";
                
                command.Parameters.AddWithValue("stockRequestId", stockRequestId);
                command.ExecuteNonQuery();
            }

        }

        public void UpdateStoreInventory(StockRequestTable stockRequest)
        {
            /*
             * to calculate if that product existed or not
             * if so, which means only to update stock level 
             * if not, which means that is new item ,insert it in storeInventory.
             */
            using (var connection = Program.ConnectionString.CreateConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                    @"select count(*) from StoreInventory
                    where StoreID = @storeID and ProductID = @productID;";
                command.Parameters.AddWithValue("storeID", stockRequest.StoreID);
                command.Parameters.AddWithValue("productID", stockRequest.ProductID);

                var count = (int) command.ExecuteScalar();
                // count = 0: does not exist that product, so insert new product into the store inventory
                if (count == 0)
                {
                    command = connection.CreateCommand();
                    command.CommandText =
                        @"INSERT INTO StoreInventory(StoreID, ProductID, StockLevel)
                        VALUES(@storeId, @productId, @stockLevel)";

                    command.Parameters.AddWithValue("storeId", stockRequest.StoreID);
                    command.Parameters.AddWithValue("productId", stockRequest.ProductID);
                    command.Parameters.AddWithValue("stockLevel", 1);
                    command.ExecuteNonQuery();
                }
                else
                {
                    command = connection.CreateCommand();
                    command.CommandText =
                        @"UPDATE StoreInventory
                        SET StoreInventory.StockLevel = StoreInventory.StockLevel + StockRequest.Quantity
                        FROM StockRequest
                        WHERE StockRequest.StockRequestId = @stockRequestId AND 
                        StoreInventory.ProductID = StockRequest.ProductID AND
                        StoreInventory.StoreID = StockRequest.StoreID";

                    command.Parameters.AddWithValue("stockRequestId", stockRequest.StockRequestId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteStockRequest(int stockRequestId)
        {
            using (var connection = Program.ConnectionString.CreateConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"DELETE FROM StockRequest WHERE StockRequest.StockRequestId = @stockRequestId";
                command.Parameters.AddWithValue("stockRequestId", stockRequestId);
                command.ExecuteNonQuery();
            }
        }

        public void DisplayOwnerInventory()
        {
            using (var connection = Program.ConnectionString.CreateConnection())
            {
                string commandString = 
                    @"SELECT Product.*, OwnerInventory.StockLevel
                    FROM Product INNER JOIN OwnerInventory 
                    ON Product.ProductID = OwnerInventory.ProductID 
                    WHERE Product.ProductID <> 0
                    ORDER BY Product.ProductID";
                SqlCommand cmd = new SqlCommand(commandString, connection);
                SqlDataReader read;
                connection.Open();
                read = cmd.ExecuteReader();
                // print out owner inventory
                Console.WriteLine("\nOwner Inventory\n");
                string header = String.Format("{0,-4}{1,-28}{2,-8}", "ID", "Product", "Current Stock");
                Console.WriteLine(header);
                string output;
                while (read.Read())
                {
                    output = String.Format("{0,-4}{1,-28}{2,-8}", read["ProductID"], read["Name"], read["StockLevel"]);
                    Console.WriteLine(output);
                }
                Console.WriteLine();
            }

        }

        public void ResetInventoryItemStock()
        {
            /*
             * 1. print out previous owner inventory items first
             * 2. use product id to choose reset option
             * 3. connect to database and check if the stocklevel of that product < 20, 
             *    if so, reset it to 20 stock level,and if not, refure to reset
             * 4. if the user inputs the product with stock level over 20, output: "the product already has enough stock" 
             * and then return owner inventory and choose again
             * 5. type enter key to return owner menu
             */
                while (true)
            {
                Console.WriteLine("Reset Stock");
                Console.WriteLine("Product stock will be reset to 20.");
                
                DisplayOwnerInventory();
                Console.Write("Enter ProductID to reset: ");
         
                var resetOption = Console.ReadLine();
                if (string.IsNullOrEmpty(resetOption))
                {
                    return;
                }

                if (!int.TryParse(resetOption, out var id))
                {
                    Console.WriteLine("Invalid Input, Please input ID Number\n");
                    Console.WriteLine();
                    continue;
                }

                var item = resetManager.GetItem(id);
                if (item == null)
                {
                    Console.WriteLine("No such item.");
                    Console.WriteLine();
                    continue;
                }

                UpdateStockLevel(item);
                break;
            }

        }
      
        public void UpdateStockLevel(OwnerInventory item)
        {
            while (true)
            {
                if (item.StockLevel >= 20)
                {
                    Console.WriteLine("{0} already has enough stock.", item.Name);
                    Console.WriteLine();
                    return;
                }

                item.StockLevel = 20;
                resetManager.UpdateStockLevel(item);

                Console.WriteLine("{0} stock level has been reset to 20.", item.Name);
                Console.WriteLine();
                break;
            }
        } 
        // print owner menu
        public void PrintOwnerMenu()
        {
            Console.WriteLine("Welcome to Marvelous Magic (Owner)");
            Console.WriteLine("==================================");
            Console.WriteLine("1. Display All Stock Requests");
            Console.WriteLine("2. Display Owner Inventory");
            Console.WriteLine("3. Reset Inventory Item Stock");
            Console.WriteLine("4. Return to Main Menu");
            Console.WriteLine();
            Console.Write("Enter an option: ");
            
        }

    }
}
