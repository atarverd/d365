using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_D365_Connector.Model
{
    public class PriceListItem
    {
        public Guid priceListId { get; set; }
        public Guid productId { get; set; }
        public Guid transactionCurrencyId { get; set; }
        public decimal pricePerUnit { get; set; }
        public string productName { get; set; }
    }
}
