
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test_D365_Connector.Model;
using Test_D365_Connector.Utilities;

namespace Test_D365_Connector
{
    class Program
    {


        static void Main(string[] args)
        {
            Logger logger = new Logger();

           try
            {
                D365Connector d365Connector = new D365Connector("ArturTarverdyan@Ligma202.onmicrosoft.com", "Menua2024!", "https://org6503dd14.api.crm4.dynamics.com/api/data/v9.2/");
                logger.Log("Succesfully connected to D365.");
                Console.WriteLine("Ennter Inventory Name");
                string inventoryName=Console.ReadLine();
               Console.WriteLine("Ennter Product Name");
                string productName=Console.ReadLine();
                 Console.WriteLine("Ennter Quantity");
                int userQuantity=Int32.Parse(Console.ReadLine());
                Console.WriteLine("1.Sub \n 2.Add");
                string typeOfOperation=Console.ReadLine();
                Product products=d365Connector.getProductByName(productName);
               
                Inventory inventory=d365Connector.getInventoryByName(inventoryName);
                InventoryProduct inventoryProduct=d365Connector.getInventoryProduct(products,inventory,typeOfOperation,userQuantity);

                //Guid invnetoryProductId=d365Connector.getInventoryProduct(products.productId,inventory.inventoryId).inventoryProductId;
                    
                Console.ReadLine();

            }
            catch(Exception ex)
            {
                logger.Log("Strange error occured: " + ex.Message);
            }
            finally
            {
                logger.Terminate();
            }
        }



    }


}
