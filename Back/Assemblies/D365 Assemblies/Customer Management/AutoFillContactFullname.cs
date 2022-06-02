using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Customer_Management
{
    public class AutoFillContactFullname : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity contact = (Entity)context.InputParameters["Target"];

                if (contact.Contains("cread_slot_first_name") && contact.Contains("cread_slot_last_name"))
                {
                    string firstName = contact.GetAttributeValue<string>("cread_slot_first_name");
                    string lastName = contact.GetAttributeValue<string>("cread_slot_last_name");
                    string fullName = firstName + " " + lastName;
                    contact["cread_name"] = fullName;
                }
            }
        }

    }
}
