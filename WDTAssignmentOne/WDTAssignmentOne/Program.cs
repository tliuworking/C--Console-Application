using System;
using Microsoft.Extensions.Configuration;

namespace WDTAssignmentOne
{
    public class Program
    {
        private static IConfigurationRoot Configuration { get; } = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        public static string ConnectionString { get; } = Configuration["ConnectionString"];

        static void Main(string[] args)
        {
            PrintMainMenu printMainMenu = new PrintMainMenu(); 
            Owner owner = new Owner();
            FranchiseHolder franchiseHolder = new FranchiseHolder();
            Customer customer = new Customer();

            while(true)
            {
                printMainMenu.MainMenuPrintOut();
                if(!int.TryParse(Console.ReadLine(), out var inputOption))
                {
                    Console.WriteLine("Incorrect Format and Please input option from number 1 to 4\n");
                    continue;
                }
                /* 1 = Owner; 2 = Franchise holder; 3 = Customer; 4 = Quit */
                switch (inputOption)
                {
                    case 1:
                        owner.OwnerMenu();
                        break;
                    case 2:
                        franchiseHolder.printAllStores.TheStoreToUse();
                        franchiseHolder.FranchiseHolderMenu();
                        break;
                    case 3:
                        customer.printAllStores.TheStoreToUse();
                        customer.CustomerMenu();
                        break;
                    case 4:
                        Console.WriteLine("Goodbye.");
                        return;                    
                    default:
                        Console.WriteLine("Invalid Input, Please select number 1,2,3,4\n");
                        break;
                }
            }
        }

        
    }
}
