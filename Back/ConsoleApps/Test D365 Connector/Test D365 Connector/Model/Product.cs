using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_D365_Connector.Model
{
    class Product
    {
        public Guid productId { get; set; }
        public string productName { get; set; }
        public decimal defaultPricePerUnit { get; set; } 
    }

    public class InventoryProduct
    {
        public string inventoryProductName { get; set; }
        public Guid inventoryProductId { get; set; }
        public int inventoryProductQuantity { get; set; }
    }
    
    public class Inventory { 
        public string invName { get; set; }
        public Guid inventoryId { get; set; }
        public Guid priceListId { get; set; } 
        public string priceListName { get; set; }
    }
}
