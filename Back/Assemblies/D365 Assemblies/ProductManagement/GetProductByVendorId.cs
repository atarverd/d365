using Microsoft.Xrm.Sdk;
using Newtonsoft.Json.Linq;
using ProductManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace ProductManagement
{
    public class GetProductByVendorId: IPlugin
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
                    tracingService.Trace("Start");
                    Entity product = (Entity)context.InputParameters["Target"];
                    SetProductPriceAndName(service, product);
                }
                catch (Exception ex)
                {
                    throw new InvalidPluginExecutionException("An error occured in plugin: " + ex.Message);
                }
            }
        }

        private void SetProductPriceAndName(IOrganizationService service, Entity product)
        {
            string vendorId = product.GetAttributeValue<string>("cread_slot_vendorid");
            string productDataText;

            using (WebClient wc = new WebClient())
            {
                productDataText = wc.DownloadString("https://invoicesapi20210913135422.azurewebsites.net/parts");
            }

            JObject productData = JObject.Parse(productDataText);
            IList<JToken> results = productData["value"].Children().ToList();
            foreach (JToken result in results)
            {
                Product vendorProduct = result.ToObject<Product>();

                if(vendorProduct.ProductId == vendorId)
                {
                    product["cread_name"] = vendorProduct.Name;
                    product["cread_mon_cost"] = new Money(vendorProduct.Price);
                    service.Update(product);
                }
            }
        }
    }
}
