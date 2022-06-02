using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customer_Management
{
    public class InventoryTotal : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity || context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference)
            {
              
                Entity inventoryProduct;
                decimal totalAmount;
                EntityReference inventory; 
                if (context.MessageName == "Create")
                {
                    inventoryProduct = (Entity)context.InputParameters["Target"];
                    inventory = inventoryProduct.GetAttributeValue<EntityReference>("cread_fk_inventory");
                    tracingService.Trace("id" + inventory.Id);
                    totalAmount = (decimal)inventoryProduct.GetAttributeValue<Money>("cread_total_amount").Value;

                }
                else if (context.MessageName == "Delete") {
                    Entity inventoryProductPreimage = (Entity)context.PreEntityImages["preimage"];
                    inventory = inventoryProductPreimage.GetAttributeValue<EntityReference>("cread_fk_inventory");
                    totalAmount = -(decimal)inventoryProductPreimage.GetAttributeValue<Money>("cread_total_amount").Value;
                    tracingService.Trace("totalDel" + totalAmount);
                }
                else
                {
                    inventoryProduct = (Entity)context.InputParameters["Target"];
                    Entity inventoryProductPreimage = (Entity)context.PreEntityImages["preimage"];

                    inventory = inventoryProductPreimage.GetAttributeValue<EntityReference>("cread_fk_inventory");
                    totalAmount = (decimal)inventoryProduct.GetAttributeValue<Money>("cread_total_amount").Value - (decimal)inventoryProductPreimage.GetAttributeValue<Money>("cread_total_amount").Value;
                    tracingService.Trace("totalupd" + totalAmount);
                }
                tracingService.Trace("tota2l" + inventory.Id);
                EntityCollection productItems = GetInventoryProducts(service, inventory.Id);
               
                foreach (Entity productItem in productItems.Entities)
                {
                    tracingService.Trace("tota2l" + productItem.Id);
                    totalAmount += (decimal)productItem.GetAttributeValue<Money>("cread_total_amount").Value;
                }
                
                tracingService.Trace("total" + totalAmount);
                tracingService.Trace("id" + totalAmount);
                Entity inventoryUpd = new Entity("cread_inventory");
                inventoryUpd.Id = inventory.Id;
                inventoryUpd["cread_total_amount"] = new Money(totalAmount);
                service.Update(inventoryUpd);
            }
        }
        public EntityCollection GetInventoryProducts(IOrganizationService service,Guid inventoryId)
        {
            QueryExpression productsQuery = new QueryExpression
            {
                EntityName = "cread_inventory_product",
                ColumnSet = new ColumnSet("cread_fk_inventory", "cread_total_amount"),
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
