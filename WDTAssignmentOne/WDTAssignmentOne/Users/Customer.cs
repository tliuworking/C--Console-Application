using System;
using System.Collections.Generic;
using System.Linq;
using WDTAssignmentOne.Utilities;

namespace WDTAssignmentOne
{
    public class Customer
    {
        private string _storeName;

        public PrintAllStores printAllStores { get; set; } = new PrintAllStores();

        public StockRequestManager stockRequestManager { get; } = new StockRequestManager();

        public void CustomerMenu()
        {
            stockRequestManager.Initialise(int.Parse(printAllStores.StoreOption));
            while (true)
            {
                PrintCustomerMenu(printAllStores.StoreOption);

                if (!int.TryParse(Console.ReadLine(), out var inputOption))
                {
                    Console.WriteLine("Incorrect Format and Please input option number 1 or 2\n");
                    continue;
                }
                /* 1 = Display product; 2 = Return to Main Menu */
                switch (inputOption)
                {
                    case 1:
                        // show menu with id and product 
                        DisplayProducts();
                        break;
                    case 2:
                        Console.WriteLine();
                        return;
                    default:
                        Console.WriteLine("Invalid Input, Please select number 1,2\n");
                        break;
                }
            }
        }

        public void DisplayProducts()
        {
            //stockRequestManager.Initialise(int.Parse(printAllStores.StoreOption));
            int startIndex = 0; 
            string inputOption;
            while (true)
            {
                // method: display inventory but only three items no matter there are more than three items or not
                DisplayInventories(startIndex);

                // function: 'N' next page and 'R' return to menu
                // OR pick an product ID to purchase
                Console.WriteLine("[Legend: 'N' Next Page | 'R' Return To Menu]\n");
                Console.Write("Enter Product ID to purchase or function: ");
                inputOption = Console.ReadLine();
                Console.WriteLine();

                if (string.IsNullOrEmpty(inputOption))
                {
                    continue;
                }
                if (inputOption == "N")
                {
                    startIndex = startIndex + 3;
                    continue;
                }
                if (inputOption == "R") {
                    Console.WriteLine("You have returned to menu.");
                    Console.WriteLine();
                    return;
                }
                if(!int.TryParse(inputOption, out var id))
                {
                    Console.WriteLine("Invliad Input, Please input product ID.");
                    Console.WriteLine();
                    continue;
                }

                /* ask user to input preferred quantity
                 * if quantity input > stock level, return error message
                 * else quantity is sufficient, then the quan number of stock level in that store will be delated
                 * 
                 */
                QuantityRequest(id);
                break;
            }
        }
        
        public void QuantityRequest(int inputOption)
        {
            while (true)
            {
                Console.Write("Enter quantity to purchase: ");
                string quantity = Console.ReadLine();
                Console.WriteLine();
                if (!int.TryParse(quantity, out var quanNumber))
                {
                    Console.WriteLine("Invalid Input, Please input number.");
                    Console.WriteLine();
                    continue;
                }
                
                var items = stockRequestManager.Items.Where(x => x.StockLevel < quanNumber && x.ProductID == inputOption);
                if (items.Any())
                {
                    foreach(var i in stockRequestManager.Items.Where(x => x.ProductID == inputOption))
                    {
                        Console.WriteLine("{0} does not have enough stock to fulfill purchase.", i.Name);
                        Console.WriteLine();
                    }
                    return;
                }

                //connect to database and update store inventory
                using (var connection = Program.ConnectionString.CreateConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText =
                        @"UPDATE StoreInventory
                        SET StockLevel = StockLevel - @quantity
                        WHERE StoreID = @storeId AND ProductID = @productId;";

                    command.Parameters.AddWithValue("storeId", int.Parse(printAllStores.StoreOption));
                    command.Parameters.AddWithValue("productId", inputOption);
                    command.Parameters.AddWithValue("quantity", quanNumber);

                    command.ExecuteNonQuery();
                }

                foreach (var i in stockRequestManager.Items.Where(x => x.ProductID == inputOption))
                {
                    Console.WriteLine("Purchased {0} of {1}", quanNumber, i.Name);
                    Console.WriteLine();
                }
                break;
            }
        }
         


        public void DisplayInventories(int startIndex)
        {
            Console.WriteLine("\nInventory\n");
            Console.WriteLine(String.Format("{0,-4}{1,-28}{2,-8}", "ID", "Product", "Current Stock"));
            if(startIndex < stockRequestManager.Items.Count)
            {
                foreach (var item in stockRequestManager.Items.GetRange(startIndex, getEndIndex(stockRequestManager.Items, startIndex)))
                {
                    Console.WriteLine(String.Format("{0,-4}{1,-28}{2,-8}", item.ProductID, item.Name, item.StockLevel));
                }
            }
            Console.WriteLine();
            return;
        }

        private int getEndIndex(List<StoreInventory> itemList, int startIndex)
        {
            // check the range meets 3 Array: [1,2,3,4] Start:1
            if(itemList.Count - startIndex > 3 )
            {
                return 3;
            }
            //Array[1,2,3,4] Start:3
            else
            {
                return itemList.Count - startIndex;
            }
        }

        public void PrintCustomerMenu(string storeOption)
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

            Console.WriteLine("Welcome to Marvelous Magic (Retail - {0})", _storeName);
            Console.WriteLine("===========================");
            Console.WriteLine("1. Display Product");
            Console.WriteLine("2. Return to Main Menu");
            Console.WriteLine();
            Console.Write("Enter an option: ");
        }
    }
}
