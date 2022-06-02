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
    public class GenerateInvoices : CodeActivity
    {
        [Input("workOrderRef")]
        [ReferenceTarget("cread_work_order")]
        public InArgument<EntityReference> WorkOrderRef { get; set; }

        protected override void Execute(CodeActivityContext executionContext)
        {
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            try
            {
                tracingService.Trace("Start");

                EntityReference workOrderRef = WorkOrderRef.Get(executionContext);
                if (workOrderRef == null)
                    return;

                Entity workOrder = GetWorkOrderById(service, workOrderRef.Id);
                EntityCollection workOrderProducts = GetWorkOrderProducts(service, workOrderRef.Id);
                EntityCollection workOrderServices = GetWorkOrderServices(service, workOrderRef.Id);

                if (workOrderProducts == null && workOrderServices == null)
                    return;

                Entity invoice = new Entity("cread_invoice");
                invoice["cread_name"] = workOrder.GetAttributeValue<string>("cread_name").Replace("WO", "INV");
                invoice["cread_customer"] = workOrder.GetAttributeValue<EntityReference>("cread_fk_customer");
                invoice["cread_fk_price_list"] = workOrder.GetAttributeValue<EntityReference>("cread_fk_price_list");
                invoice["cread_fk_work_order"] = workOrderRef;

                Guid invoiceId = service.Create(invoice);

                Entity invoiceLine = new Entity("cread_invoice_line");
                foreach (Entity workOrderProduct in workOrderProducts.Entities)
                {
                    decimal quantity = workOrderProduct.GetAttributeValue<int>("cread_quantity");
                    invoiceLine["cread_fk_invoice"] = new EntityReference("cread_invoice", invoiceId);
                    invoiceLine["cread_fk_product"] = workOrderProduct.GetAttributeValue<EntityReference>("cread_fk_my" +
                        "_product");
                    invoiceLine["cread_dec_quantity"] = quantity;
                    invoiceLine["cread_mon_price_per_unit"] = workOrderProduct.GetAttributeValue<Money>("cread_fk_price_per_unit");
                    invoiceLine["cread_mon_total_amount"] = workOrderProduct.GetAttributeValue<Money>("cread_mon_total_amount");

                    service.Create(invoiceLine);
                }
                foreach (Entity workOrderService in workOrderServices.Entities)
                {
                    decimal duration = workOrderService.GetAttributeValue<int>("cread_whole_duration") / 60m;
                    invoiceLine["cread_fk_invoice"] = new EntityReference("cread_invoice", invoiceId);
                    invoiceLine["cread_fk_product"] = workOrderService.GetAttributeValue<EntityReference>("cread_fk_service");
                    invoiceLine["cread_dec_quantity"] = duration;
                    invoiceLine["cread_mon_price_per_unit"] = workOrderService.GetAttributeValue<Money>("cread_mon_price_pet_unit");
                    invoiceLine["cread_mon_total_amount"] = workOrderService.GetAttributeValue<Money>("cread_mon_total_amount");
                    invoiceLine["cread_mon_total_amount"] = workOrderService.GetAttributeValue<Money>("cread_mon_total_amount");

                    service.Create(invoiceLine);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }

        private Entity GetWorkOrderById(IOrganizationService service, Guid workOrderId)
        {
            QueryExpression workOrderQuery = new QueryExpression("cread_work_order")
            {
                ColumnSet = new ColumnSet("cread_work_orderid", "cread_name", "cread_fk_customer", "cread_fk_price_list"),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression("cread_work_orderid", ConditionOperator.Equal, workOrderId)
                    }
                }
            };
            EntityCollection workOrders = service.RetrieveMultiple(workOrderQuery);
            Entity workOrder = workOrders.Entities[0];
            return workOrder;
        }

        private EntityCollection GetWorkOrderProducts(IOrganizationService service, Guid workOrderId)
        {
            QueryExpression workOrderProductQuery = new QueryExpression("cread_workorderproduct")
            {
                ColumnSet = new ColumnSet("cread_fk_work_order", "cread_fk_my_product", "cread_fk_price_per_unit", "cread_quantity", "cread_mon_total_amount"),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression("cread_fk_work_order", ConditionOperator.Equal, workOrderId)
                    }
                }
            };
            return service.RetrieveMultiple(workOrderProductQuery);

        }

        private EntityCollection GetWorkOrderServices(IOrganizationService service, Guid workOrderId)
        {
            QueryExpression workOrderServiceQuery = new QueryExpression("cread_workorderservice")
            {
                ColumnSet = new ColumnSet("cread_fk_work_order", "cread_fk_service", "cread_mon_price_pet_unit", "cread_whole_duration", "cread_mon_total_amount"),
                Criteria = new FilterExpression(LogicalOperator.And)
                {
                    Conditions =
                    {
                        new ConditionExpression("cread_fk_work_order", ConditionOperator.Equal, workOrderId)
                    }
                }
            };
            return service.RetrieveMultiple(workOrderServiceQuery);
        }
    }
}

