using System;
using System.Data.SqlClient;
using WDTAssignmentOne.Utilities;

namespace WDTAssignmentOne
{
    public class PrintAllStores
    {
        public string StoreOption { get; set; }
        
        public virtual void TheStoreToUse()
        {
            using (var connection = Program.ConnectionString.CreateConnection())
            {
                string commandString = "select * from Store";
                SqlCommand cmd = new SqlCommand(commandString, connection);
                SqlDataReader read;
                while (true)
                {
                    connection.Open();
                    read = cmd.ExecuteReader();
                    // print out store with id and name
                    Console.WriteLine("\nStores: \n");
                    string header = String.Format("{0, -8}{1,-18}", "ID", "Name");
                    Console.WriteLine(header);
                    string output;
                    while (read.Read())
                    {
                        output = String.Format("{0,-8}{1,-18}", read["StoreID"], read["Name"]);
                        Console.WriteLine(output);
                    }
                    Console.WriteLine();
                    while (true)
                    {
                        // choose your option
                        Console.Write("Enter the store to use: ");
                        StoreOption = Console.ReadLine();
                        Console.WriteLine();

                        if (StoreOption != "1" && StoreOption != "2" && StoreOption != "3" && StoreOption != "4" && StoreOption != "5")
                        {
                            Console.WriteLine("Incorrect Format and Please input option from number 1 to 5");
                            continue;
                        }
                        break;
                    }
                    break;
                    
                }
                
            }
                
            
        }

    }
}
