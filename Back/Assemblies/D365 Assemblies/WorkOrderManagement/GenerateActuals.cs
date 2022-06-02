using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkOrderManagement
{
    public class GenerateActuals : CodeActivity
    {
        [Output("status")]
        public OutArgument<string> Status { get; set; }

        protected override void Execute(CodeActivityContext executionContext)
        {
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            IExecutionContext crmContext = executionContext.GetExtension<IExecutionContext>();

            try
            {
                tracingService.Trace("Start");
                EntityReference workOrderRef = (EntityReference)crmContext.InputParameters["Target"];

                if (workOrderRef == null)
                    return;

                EntityCollection workOrderProducts = GetWorkOrderProducts(service, workOrderRef.Id);
                tracingService.Trace("Start1");

                EntityCollection workOrderServices = GetWorkOrderServices(service, workOrderRef.Id);
                EntityCollection actuals = GetWorkOrderActuals(service, workOrderRef.Id);
                if (workOrderProducts == null && workOrderServices == null)
                    return;
                Entity actual = new Entity("cread_actual");
                foreach (Entity woActual in actuals.Entities)
                {
                    service.Delete("cread_actual", woActual.Id);
                }
                tracingService.Trace("Sta3rt");

                foreach (Entity workOrderProduct in workOrderProducts.Entities)
                {
                    tracingService.Trace("here");

                    Guid productId = workOrderProduct.GetAttributeValue<EntityReference>("cread_fk_my_product").Id;
                    decimal costPerUnit = workOrderProduct.GetAttributeValue<Money>("cread_mon_cost").Value;
                    decimal quantity = workOrderProduct.GetAttributeValue<int>("cread_quantity");
                    actual["cread_name"] = GetProductNameById(service, productId);
                    actual["cread_fk_work_order"] = new EntityReference("cread_work_order", workOrderRef.Id);
                    actual["cread_mon_cost_per_unit"] = new Money(costPerUnit);
                    actual["cread_dec_quantity"] = quantity;
                    actual["cread_mon_total_cost"] = new Money(costPerUnit * quantity);
                    tracingService.Trace(""+ GetProductNameById(service, productId));
                    service.Create(actual);
                }

                foreach (Entity workOrderService in workOrderServices.Entities)
                {
                    Guid productId = workOrderService.GetAttributeValue<EntityReference>("cread_fk_service").Id;
                    Guid resourceId = workOrderService.GetAttributeValue<EntityReference>("cread_fk_resource").Id;
                    tracingService.Trace("" + resourceId);
                    decimal hourlyRate = GetResourceRateById(service, resourceId);
                    decimal duration = workOrderService.GetAttributeValue<int>("cread_whole_duration") / 60m;
                    actual["cread_name"] = GetProductNameById(service, productId);
                    actual["cread_fk_work_order"] = new EntityReference("cread_work_order", workOrderRef.Id);
                    actual["cread_mon_cost_per_unit"] = new Money(hourlyRate);
                    actual["cread_dec_quantity"] = duration;
                    actual["cread_mon_total_cost"] = new Money(hourlyRate * duration);
                    service.Create(actual);
                }
                tracingService.Trace("Sta6rt");

                Status.Set(executionContext, "Successed");
            }
            catch (Exception ex)
            {
                Status.Set(executionContext, "Failed");
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }



        private decimal GetResourceRateById(IOrganizationService service, Guid resourceId)
        {
            QueryExpression resourceQuery = new QueryExpression("cread_recourse")
            {
                ColumnSet = new ColumnSet("cread_recourseid", "cread_mon_hourly_rate"),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression("cread_recourseid", ConditionOperator.Equal, resourceId)
                    }
                }
            };

            EntityCollection resources = service.RetrieveMultiple(resourceQuery);
            Entity resource = resources.Entities[0];
            decimal hourlyRate = resource.GetAttributeValue<Money>("cread_mon_hourly_rate").Value;
            return hourlyRate;
        }

        private EntityCollection GetWorkOrderActuals(IOrganizationService service, Guid workOrderId)
        {
            EntityCollection workOrderActuals;
            QueryExpression workOrderActualQuery = new QueryExpression("cread_actual")
            {
                ColumnSet = new ColumnSet("cread_fk_work_order"),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression("cread_fk_work_order", ConditionOperator.Equal, workOrderId)
                    }
                }
            };

            workOrderActuals = service.RetrieveMultiple(workOrderActualQuery);

            return workOrderActuals;
        }
        private string GetProductNameById(IOrganizationService service, Guid productId)
        {

            QueryExpression productQuery = new QueryExpression("cread_my_products")
            {
                ColumnSet = new ColumnSet("cread_my_productsid", "cread_name"),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression("cread_my_productsid", ConditionOperator.Equal, productId)
                    }
                }
            };

            EntityCollection products = service.RetrieveMultiple(productQuery);
            Entity product = products.Entities[0];
            string productName = product.GetAttributeValue<string>("cread_name");
            return productName;
        }
        private EntityCollection GetWorkOrderProducts(IOrganizationService service, Guid workOrderId)
        {
            EntityCollection workOrderProducts;
            QueryExpression workOrderProductQuery = new QueryExpression("cread_workorderproduct")
            {
                ColumnSet = new ColumnSet("cread_fk_work_order", "cread_fk_my_product", "cread_mon_cost", "cread_quantity"),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression("cread_fk_work_order", ConditionOperator.Equal, workOrderId)
                    }
                }
            };

            workOrderProducts = service.RetrieveMultiple(workOrderProductQuery);

            return workOrderProducts;

        }

        private EntityCollection GetWorkOrderServices(IOrganizationService service, Guid workOrderId)
        {
            EntityCollection workOrderServices;
            QueryExpression workOrderServiceQuery = new QueryExpression("cread_workorderservice")
            {
                ColumnSet = new ColumnSet("cread_fk_work_order", "cread_fk_resource", "cread_fk_service", "cread_whole_duration"),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression("cread_fk_work_order", ConditionOperator.Equal, workOrderId)
                    }
                }
            };

            workOrderServices = service.RetrieveMultiple(workOrderServiceQuery);

            return workOrderServices;
        }
    }
}