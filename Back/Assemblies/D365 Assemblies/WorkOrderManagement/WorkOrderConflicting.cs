using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkOrderManagement
{
    public class WorkOrderConflicting : IPlugin
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
                    Entity booking = (Entity)context.InputParameters["Target"];
                    EntityReference worker;
                    if (context.MessageName == "Update")
                        booking = service.Retrieve("cread_booking", booking.Id, new ColumnSet("cread_fk_resource", "cread_dt_start_date", "cread_dt_end_date"));

                    worker = booking.GetAttributeValue<EntityReference>("cread_fk_resource");
                    if (worker != null)
                    {
                        DateTime endDate = booking.GetAttributeValue<DateTime>("cread_dt_end_date");
                        DateTime startDate = booking.GetAttributeValue<DateTime>("cread_dt_start_date");
                        Boolean res = IsBusy(service, worker.Id, endDate, startDate, booking.Id);
                        if (res)
                            throw new InvalidPluginExecutionException("Busy!");
                        tracingService.Trace("busy" + res);
                    }


                }

                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException("An error occured in plugin: " + ex.Message);
                }

            }
        }
        public Boolean IsBusy(IOrganizationService service, Guid workerId, DateTime endDate, DateTime startDate, Guid bookingId)
        {
            QueryExpression workOrderQuery = new QueryExpression
            {
                EntityName = "cread_booking",
                ColumnSet = new ColumnSet("cread_fk_resource", "cread_dt_end_date"),
                Criteria = new FilterExpression
                {

                    FilterOperator = LogicalOperator.And,
                    Filters =
                    {
                        new FilterExpression
                        {
                            FilterOperator = LogicalOperator.Or,
                            Filters =
                            {
                                new FilterExpression
                                {
                                    FilterOperator = LogicalOperator.And,
                                    Conditions =
                                    {
                                        new ConditionExpression("cread_dt_start_date", ConditionOperator.GreaterEqual, startDate),
                                        new ConditionExpression("cread_dt_start_date", ConditionOperator.LessEqual, endDate),
                                    }
                                },
                                new FilterExpression
                                {
                                    FilterOperator = LogicalOperator.And,
                                    Conditions =
                                    {
                                        new ConditionExpression("cread_dt_start_date", ConditionOperator.LessEqual, startDate),
                                        new ConditionExpression("cread_dt_end_date", ConditionOperator.GreaterEqual, startDate),
                                    }
                                },
                            }
                        },
                        new FilterExpression
                        {
                            FilterOperator = LogicalOperator.And,
                            Conditions =
                            {
                                new ConditionExpression("cread_fk_resource", ConditionOperator.Equal, workerId),
                                new ConditionExpression("cread_bookingid", ConditionOperator.NotEqual, bookingId)
                            }
                        },
                    },
                }
            };
            EntityCollection bookings = service.RetrieveMultiple(workOrderQuery);
            if (bookings.Entities.Count > 0)
                return true;
            return false;
        }
    }
}
