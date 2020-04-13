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

            decimal loanAmount = GetAttribute<decimal>(entity, "new_loan_amount");
            decimal interestRate = GetAttribute<decimal>(entity, "new_interest_rate");
            int term = GetAttribute<int>(entity, "new_term");
            
            Loan apiResponse = CallApiMethod(loanAmount, interestRate, term);

            string comments = $"({DateTime.Now.ToLongTimeString()}) - TotalPayments: {apiResponse.TotalPayments}; TotalInterest: {apiResponse.TotalInterest}; MonthlyPayment: {apiResponse.MonthlyPayment}";

            SetAttribute(entity, "new_total_payment", apiResponse.TotalPayments);
            SetAttribute(entity, "new_total_interest", apiResponse.TotalInterest);
            SetAttribute(entity, "new_monthly_payment", apiResponse.MonthlyPayment);
            SetAttribute(entity, "new_comments", comments);

            Service.Update(entity);
        }

        private static void SetAttribute(Entity entity, string attributeName, decimal attributeValue)
        {
            if (entity.Attributes.ContainsKey(attributeName))
                entity[attributeName] = attributeValue;
            else
                entity.Attributes.Add(attributeName, attributeValue);
        }

        private static void SetAttribute(Entity entity, string attributeName, string attributeValue)
        {
            if (entity.Attributes.ContainsKey(attributeName))
                entity[attributeName] = attributeValue;
            else
                entity.Attributes.Add(attributeName, attributeValue);
        }

        private static T GetAttribute<T>(Entity entity, string attributeName) where T : struct
        {
            if (entity.Attributes.ContainsKey(attributeName))
                return (T)entity[attributeName];
            else 
                return default(T);
        }

        private Loan CallApiMethod(decimal loanAmount, decimal interestRate, int term)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://spfapi.azurewebsites.net/api/cashflow/");

                string fragment = $"calculate?loanAmount={loanAmount}&interestRate={interestRate}&term={term}";

                TracingService.Trace(fragment);

                var response = client.GetAsync(fragment).Result;

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
