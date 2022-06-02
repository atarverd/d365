using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_D365_Connector.Model
{
    public class InventoryProduct
    {
        public Guid productId { get; set; }
        public string productName { get; set; }
        public Guid inventoryId { get; set; }
        public int quantity { get; set; }
    }
}
