using System;
using System.Data.SqlClient;
using System.Linq;
using WDTAssignmentOne.Utilities;

namespace WDTAssignmentOne
{
    public class FranchiseHolder
    {
        private string _storeName;
        public string restockInput { get; set; }

        public StockRequestManager stockRequestManager { get; } = new StockRequestManager();

        public PrintAllStores printAllStores { get; set; } = new PrintAllStores();

        public void FranchiseHolderMenu()
        {
            stockRequestManager.Initialise(int.Parse(printAllStores.StoreOption));
            while (true)
            {
                PrintFranchiseHolderMenu(printAllStores.StoreOption);
                
                if (!int.TryParse(Console.ReadLine(), out var inputOption))
                {
                    Console.WriteLine("Incorrect Format and Please input option from number 1 to 4\n");
                    continue;
                }

                /* 1 = Display Inventory; 
                 * 2 = Stock Request (Threshold); 
                 * 3 = Add New Inventory Item; 
                 * 4 = Return to Main Menu */
                switch (inputOption)
                {
                    case 1:
                        // display inventory menu with id, product and current stock
                        DisplayStoreInventory();
                        break;
                    case 2:
                        StockRequest();
                        break;
                    case 3:
                        AddNewInventoryItem(printAllStores.StoreOption);
                        break;
                    case 4:
                        Console.WriteLine();
                        return;
                    default:
                        Console.WriteLine("Invalid Input, Please select number 1,2,3,4\n");
                        break;
                }
            }
        }
        
        public void DisplayStoreInventory()
        {
            Console.WriteLine("\nInventory\n");
            Console.WriteLine(String.Format("{0,-4}{1,-28}{2,-8}", "ID", "Product", "Current Stock"));
            
            foreach(var x in stockRequestManager.Items)
            {
                Console.WriteLine(String.Format("{0,-4}{1,-28}{2,-8}", x.ProductID, x.Name, x.StockLevel));
            }
            Console.WriteLine();
            return;
        }

        public void StockRequest()
        {
            // After input restock numeber, 
            // 1. checking whether all stock level are equal to or above that number, 
            // if so, return "all stock level are equal to or above xxx"
            // if not, return inventory
            // 2. franchise enter request through ID input, then request created
            // insert that request in the table StockRequest, put in the list and then call method to display AllOwnerRequest
            // 3. the owner action this request later
            while (true)
            {
                Console.Write("Enter shershold for re-stocking: ");
                Console.WriteLine();
                restockInput = Console.ReadLine();

                if (string.IsNullOrEmpty(restockInput))
                {
                    return;
                }

                if (!int.TryParse(restockInput, out var number))
                {
                    Console.WriteLine("Invalid Input, Please input integer.");
                    Console.WriteLine();
                    continue;
                }

                var items = stockRequestManager.Items.Where(x => x.StockLevel < number);
                if(!items.Any())
                {
                    Console.WriteLine("All inventory stock levels equals to or above {0}", restockInput);
                    Console.WriteLine();
                    return;
                }

                // Printout Inventory less than thershold
                DisplayStockLessThanThreshold(number);
                CreateRequest(number);
                break;
            }
        }

        public void DisplayStockLessThanThreshold(int restockInput)
        {
            using (var connection = Program.ConnectionString.CreateConnection())
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"SELECT StoreInventory.*, Product.Name
                                        FROM StoreInventory INNER JOIN Product
                                        ON StoreInventory.ProductID = Product.ProductID
                                        WHERE StoreInventory.ProductID <> 0 AND StoreID = @storeID
                                            AND StockLevel < @restockInput
                                        ORDER BY StoreInventory.StoreID ";
                command.Parameters.AddWithValue("storeID", printAllStores.StoreOption);
                command.Parameters.AddWithValue("restockInput", restockInput);

                SqlDataReader read = command.ExecuteReader();
                Console.WriteLine("\nInventory\n");
                Console.WriteLine(String.Format("{0,-4}{1,-28}{2,-8}", "ID", "Product", "Current Stock"));
                
                while (read.Read())
                {
                    Console.WriteLine(String.Format("{0,-4}{1,-28}{2,-8}", read["ProductID"], read["Name"], read["StockLevel"]));
                }
                Console.WriteLine();
            }
        }

        // when productID is input, stock request is created
        // then the StockRequest table will be updated. it should work like reset inventory
        public void CreateRequest(int restockInput)
        {
            while (true)
            {
                Console.Write("Enter request to process: ");
                var restockId = Console.ReadLine();
                if (string.IsNullOrEmpty(restockId))
                {
                    return;
                }
                if (!int.TryParse(restockId, out var id))
                {
                    Console.WriteLine("Invliad Input");
                    Console.WriteLine();
                    continue;
                }
                
                using (var connection = Program.ConnectionString.CreateConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText =
                        @"INSERT INTO StockRequest(StoreID, ProductID, Quantity)
                        VALUES(@storeId, @productId, @quantity)";
                 
                    command.Parameters.AddWithValue("storeId", int.Parse(printAllStores.StoreOption));
                    command.Parameters.AddWithValue("productId", id);
                    command.Parameters.AddWithValue("quantity", restockInput);

                    command.ExecuteNonQuery();
                }
                Console.WriteLine("\nSuccessfully stock request.\n");
                break;
            }
        }


        public void AddNewInventoryItem(string store)
        {
            // 1. display the owner stock items that the store does not have,
            // 2. select product ID to add new item
            // 3. send stock request to StockRequest table and StockRequest.quantity = 1
            while (true)
            {
                using (var connection = Program.ConnectionString.CreateConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText =
                        @"SELECT OwnerInventory.*, Product.Name FROM OwnerInventory
                        INNER JOIN Product ON OwnerInventory.ProductID = Product.ProductID
                        WHERE not exists 
                        (SELECT StoreInventory.ProductID FROM StoreInventory
                         WHERE StoreID = @storeID AND StoreInventory.ProductID = OwnerInventory.ProductID)";
                    command.Parameters.AddWithValue("storeID", store);
                    
                    SqlDataReader read;
                    read = command.ExecuteReader();
                    Console.WriteLine("\nInventory\n");
                    Console.WriteLine(String.Format("{0,-4}{1,-28}{2,-8}", "ID", "Product", "Current Stock"));
                    while (read.Read())
                    {
                        Console.WriteLine(String.Format("{0,-4}{1,-28}{2,-8}", read["ProductID"], read["Name"], read["StockLevel"]));
                    }
                    Console.WriteLine();
                }

                EnterProductToAddStoreReq();
                Console.WriteLine("A new stock request is created with quantity set to 1.\n");
                break;
            }


        }

        public void EnterProductToAddStoreReq()
        {
            while (true)
            {
                Console.Write("Enter Product to add: ");
                var proOption = Console.ReadLine();
                Console.WriteLine();
                
                if (string.IsNullOrEmpty(proOption))
                {
                    return;
                }

                if (!int.TryParse(proOption, out var id))
                {
                    Console.WriteLine("Invalid Input, Please input ID Number\n");
                    Console.WriteLine();
                    continue;
                }

                SendToStockRequest(id);
                break;
            }
         
            
        }
        // this method will be called in add new item method
        public void SendToStockRequest(int productId)
        {
            using (var connection = Program.ConnectionString.CreateConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"INSERT INTO StockRequest(StoreID, ProductID, Quantity)
                    VALUES(@storeId, @productId, @quantity)";

                command.Parameters.AddWithValue("storeId", int.Parse(printAllStores.StoreOption));
                command.Parameters.AddWithValue("productId", productId);
                command.Parameters.AddWithValue("quantity", 1);

                command.ExecuteNonQuery();
            }
        }

        public void PrintFranchiseHolderMenu(string storeOption)
        {
            // print store name on the menu title
            if (storeOption == "1")
                _storeName = "Melbourne CBD";
            else if (storeOption == "2")
                _storeName = "North Melbourne";
            else if (storeOption == "3")
                _storeName = "East Melbourne";
            else if (storeOption == "4")
                _storeName = "South Melbourne";
            else if (storeOption == "5")
                _storeName = "West Melbourne";
            else
                _storeName = "";
            
            Console.WriteLine("Welcome to Marvelous Magic (Franchise Holder - {0})", _storeName);
            Console.WriteLine("===========================");
            Console.WriteLine("1. Display Inventory");
            Console.WriteLine("2. Stock Request (Threshold)");
            Console.WriteLine("3. Add New Inventory Item");
            Console.WriteLine("4. Return to Main Menu");
            Console.WriteLine();
            Console.Write("Enter an option: ");
        }
        

    }
}
