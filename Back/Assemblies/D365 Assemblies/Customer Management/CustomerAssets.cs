using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customer_Management
{
    public class CustomerAssets : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity customerAsset = (Entity)context.InputParameters["Target"];
                EntityReference account = customerAsset.GetAttributeValue<EntityReference>("cread_fk_my_account");
                EntityCollection assets= FilterAssets(service, account.Id);
                Entity accountRet = service.Retrieve("cread_my_account", account.Id, new ColumnSet("cread_name"));
                customerAsset["cread_name"] = accountRet.GetAttributeValue<string>("cread_name") + "-" + assets.Entities.Count;
                service.Update(customerAsset);
                tracingService.Trace("count" + assets.TotalRecordCount);
             
            }
        }

         public EntityCollection FilterAssets(IOrganizationService service,Guid accountId) {
            QueryExpression assetsQuery = new QueryExpression
            {
                EntityName = "cread_assets",
                ColumnSet = new ColumnSet("cread_fk_my_account"),
                Criteria = {
                    Conditions={
                        new ConditionExpression("cread_fk_my_account", ConditionOperator.Equal, accountId)
                    }
                }
            };

            return service.RetrieveMultiple(assetsQuery);
        }

    }
}
