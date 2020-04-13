using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Xrm.Sdk;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;

namespace ApiPlugin
{
    public class DemoPlugin : PluginBase
    {
        protected override void ExecuteOnEntity(Entity entity)
        {
            if (Depth > 1)
                return;

            Loan apiResponse = CallApiMethod();

            string currentTime = $"{DateTime.Now.ToLongTimeString()} - {apiResponse}";

            string calc = $"({DateTime.Now.ToLongTimeString()}) - TotalPayments: {apiResponse.TotalPayments}; TotalInterest: {apiResponse.TotalInterest}; MonthlyPayment: {apiResponse.MonthlyPayment}";

            if (entity.Attributes.ContainsKey("new_comments"))
                entity["new_comments"] = calc;
            else
                entity.Attributes.Add("new_comments", calc);

            Service.Update(entity);
        }

        private Loan CallApiMethod()
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://spfapi.azurewebsites.net/api/cashflow/");

                var response = client.GetAsync("calculate?loanAmount=487500&interestRate=0.015&term=25").Result;

                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    
                    Loan loan = ReadToObject(result);
                    return loan;
                }

                TracingService.Trace(response.RequestMessage.RequestUri.ToString());
                TracingService.Trace(response.StatusCode.ToString());
                TracingService.Trace(response.Content.ToString());

                throw new InvalidPluginExecutionException("Error accessing API");
            }
        }

        private static Loan ReadToObject(string json)
        {
            var deserializedLoan = new Loan();
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var ser = new DataContractJsonSerializer(deserializedLoan.GetType());
            deserializedLoan = ser.ReadObject(ms) as Loan;
            ms.Close();
            return deserializedLoan;
        }
    }

    [DataContract]
    class Loan
    {
        [DataMember]
        public decimal TotalPayments { get; set; }
        [DataMember]
        public decimal TotalInterest { get; set; }
        [DataMember]
        public decimal MonthlyPayment { get; set; }
    }
}
