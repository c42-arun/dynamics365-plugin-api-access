using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Xrm.Sdk;

namespace ApiPlugin
{
    public class DemoPlugin : PluginBase
    {
        protected override void ExecuteOnEntity(Entity entity)
        {
            if (Depth > 1)
                return;

            string apiResponse = CallApiMethod();

            string currentTime = $"{DateTime.Now.ToLongTimeString()} - {apiResponse}";

            if (entity.Attributes.ContainsKey("new_comments"))
                entity["new_comments"] = currentTime;
            else
                entity.Attributes.Add("new_comments", currentTime);

            Service.Update(entity);
        }

        private string CallApiMethod()
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://spfapi.azurewebsites.net/api/cashflow/");

                var response = client.GetAsync("calculate?loanAmount=487500&interestRate=0.015&term=25").Result;

                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    return result;
                }
                TracingService.Trace(response.RequestMessage.RequestUri.ToString());
                TracingService.Trace(response.StatusCode.ToString());
                TracingService.Trace(response.Content.ToString());

                throw new InvalidPluginExecutionException("Error accessing API");
            }
        }
    }
}
