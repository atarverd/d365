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
    public class DeleteInvoices : CodeActivity
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
                EntityReference workOrderRef = WorkOrderRef.Get(executionContext);
                QueryExpression invoiceQuery = new QueryExpression("cread_invoice")
                {
                    ColumnSet = new ColumnSet("cread_fk_work_order"),
                    Criteria = new FilterExpression(LogicalOperator.And)
                    {
                        Conditions =
                        {
                            new ConditionExpression("cread_fk_work_order", ConditionOperator.Equal, workOrderRef.Id)
                        }
                    }
                };

                EntityCollection invoices = service.RetrieveMultiple(invoiceQuery);
                tracingService.Trace(" count  " + invoices.Entities.Count);
                foreach (Entity invoice in invoices.Entities)
                {
                    service.Delete("cread_invoice", invoice.Id);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }
    }
}
