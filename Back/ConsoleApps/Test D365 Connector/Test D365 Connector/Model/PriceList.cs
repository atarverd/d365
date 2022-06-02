using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_D365_Connector.Model
{
    class PriceList
    {
        public Guid priceListId { get; set; }
        public string priceListName { get; set; }
        public Guid currencyId { get; set; }
    }
}
