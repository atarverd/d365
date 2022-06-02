using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;

namespace ResourceManagement
{
    public class AutoCalculateAge : CodeActivity
    {

        [Input("dateOfBirth")]
        public InArgument<DateTime> DateOfBirth { get; set; }

        [Output("age")]
        public OutArgument<int> Age { get; set; }

        protected override void Execute(CodeActivityContext executionContext)
        {
            ITracingService tracingService = executionContext.GetExtension<ITracingService>();
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            try
            {
                DateTime dateOfBirth = DateOfBirth.Get(executionContext);

                if (dateOfBirth != DateTime.MinValue)
                {
                    int age = Convert.ToInt32(DateTime.Now.Subtract(dateOfBirth).TotalDays / 365);
                    Age.Set(executionContext, age);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException(ex.Message);
            }

        }
    }
}
