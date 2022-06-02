using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace WorkOrderManagement
{
    public class WorkOrderCloseStatus : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                try
                {
                    Entity workOrder = (Entity)context.InputParameters["Target"];
                    OptionSetValue status = workOrder.GetAttributeValue<OptionSetValue>("cread_os_status");
                    EntityCollection workOrderProducts = GetWorkOrderProducts(service, workOrder.Id);
                    foreach (Entity workOrderProduct in workOrderProducts.Entities)
                    {
                        EntityReference productRef = workOrderProduct.GetAttributeValue<EntityReference>("cread_fk_my_product");
                        EntityReference inventoryRef = workOrderProduct.GetAttributeValue<EntityReference>("cread_fk_inventory");
                        int quantity = workOrderProduct.GetAttributeValue<int>("cread_quantity");
                        EntityCollection inventoryProducts = IsQuantityAvailable(service, inventoryRef.Id, productRef.Id, quantity);
                        if (inventoryProducts.Entities.Count > 0)
                        {
                            foreach (Entity inventoryProduct in inventoryProducts.Entities)
                            {
                                Entity inventoryProductUpd = new Entity("cread_inventory_product");
                                inventoryProductUpd.Id = inventoryProduct.Id;
                                inventoryProductUpd["cread_quantity"] = inventoryProduct.GetAttributeValue<int>("cread_quantity") - quantity;
                                service.Update(inventoryProductUpd);
                            }
                        }
                    }
                    tracingService.Trace("asdf");
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException("An error occured in plugin: " + ex.Message);
                }
            }
        }
        public EntityCollection GetWorkOrderProducts(IOrganizationService service, Guid workOrderId)
        {
            QueryExpression workOrderProductsQuery = new QueryExpression
            {
                EntityName = "cread_workorderproduct",
                ColumnSet = new ColumnSet("cread_fk_inventory", "cread_fk_my_product", "cread_fk_work_order", "cread_quantity"),
                Criteria = {
                    Conditions={
                        new ConditionExpression("cread_fk_work_order", ConditionOperator.Equal, workOrderId)
                    }
                }
            };

            return service.RetrieveMultiple(workOrderProductsQuery);
        }

        public EntityCollection IsQuantityAvailable(IOrganizationService service, Guid inventoryId, Guid productId, int quantity)
        {
            QueryExpression inventoryProductQuery = new QueryExpression
            {
                EntityName = "cread_inventory_product",
                ColumnSet = new ColumnSet("cread_fk_inventory", "cread_fk_my_product", "cread_quantity"),
                Criteria = {
                    Conditions={
                        new ConditionExpression("cread_fk_inventory", ConditionOperator.Equal, inventoryId),
                        new ConditionExpression("cread_fk_my_product", ConditionOperator.Equal, productId),
                        new ConditionExpression("cread_quantity", ConditionOperator.GreaterEqual, quantity)
                    }
                }
            };


            return service.RetrieveMultiple(inventoryProductQuery);
        }
    }
}
