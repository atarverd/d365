using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customer_Management
{
    public class AutoAddPriceListItems : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
               
                tracingService.Trace("context contains Target");
                Entity priceList = (Entity)context.InputParameters["Target"];
                EntityCollection products = GetProducts(service);
                if (products != null && products.Entities.Count > 0)
                {
                    tracingService.Trace("Products quantity is " + products.Entities.Count);

                    foreach (Entity product in products.Entities)
                    {

                        Entity priceListItem = new Entity("cread_price_list_items");
                        priceListItem["cread_name"] = product.GetAttributeValue<string>("cread_name");
                        priceListItem["cread_fk_my_product"] = product.ToEntityReference();
                        priceListItem["cread_fk_price_list"] = priceList.ToEntityReference();
                        priceListItem["transactioncurrencyid"] = priceList.GetAttributeValue<EntityReference>("transactioncurrencyid");
                        priceListItem["cread_mon_price"] = new Money(1);
                        tracingService.Trace("id" + priceListItem.Id);
                        service.Create(priceListItem);
                    }
                }

            }
        }

        public EntityCollection GetProducts(IOrganizationService service)
        {
            QueryExpression productsQuery = new QueryExpression
            {
                EntityName = "cread_my_products",
                ColumnSet = new ColumnSet("cread_name"),
            };

            return service.RetrieveMultiple(productsQuery);
        }
    }
}
