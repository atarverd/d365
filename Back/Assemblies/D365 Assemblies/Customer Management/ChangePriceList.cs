using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customer_Management
{
    public class ChangePriceList : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity inventory= (Entity)context.InputParameters["Target"];
                if (inventory.Contains("cread_fk_price_list")) {
                    EntityReference currencyRef = inventory.GetAttributeValue<EntityReference>("cread_fk_price_list");
                    Entity currencyRet = service.Retrieve("cread_price_list", currencyRef.Id, new ColumnSet("transactioncurrencyid"));
                    tracingService.Trace("id" + currencyRet.GetAttributeValue<EntityReference>("transactioncurrencyid").Id);
                    Guid currency = currencyRet.GetAttributeValue<EntityReference>("transactioncurrencyid").Id;
                    EntityCollection productItems = GetInventoryProducts(service, inventory.Id);
                    foreach (Entity productItem in productItems.Entities)
                    {
                         productItem["transactioncurrencyid"] = new EntityReference("transactioncurrency",currency);
                         service.Update(productItem);
                    }
                }
            }

        }
        public EntityCollection GetInventoryProducts(IOrganizationService service, Guid inventoryId)
        {
            QueryExpression productsQuery = new QueryExpression
            {
                EntityName = "cread_inventory_product",
                ColumnSet = new ColumnSet("cread_fk_inventory", "transactioncurrencyid"),
                Criteria = {
                    Conditions={
                        new ConditionExpression("cread_fk_inventory", ConditionOperator.Equal, inventoryId)
                    }
                }
            };

            return service.RetrieveMultiple(productsQuery);
        }
    }
}

