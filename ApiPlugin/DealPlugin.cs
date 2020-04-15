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
using System.Web;

namespace ApiPlugin
{
    public class DealPlugin : PluginBase
    {
        protected override void ExecuteOnEntity(Entity entity)
        {
            if (entity.LogicalName != "new_deal" || Depth > 1)
                return;

            Entity preImage = GetPreImage("PreImage_Deal");

            //decimal acquisitionCosts = GetAttribute<decimal>(entity, "new_acquisition_costs");
            decimal acquisitionCosts = entity.GetValue<decimal>(preImage, "new_acquisition_costs");
            string loanType = entity.GetFieldValue<string>(preImage, "new_loan_type");
            
            Deal apiResponse = CallApiMethod(acquisitionCosts, loanType);

            string comments = $"({DateTime.Now.ToLongTimeString()}) - iru: {apiResponse.iru}; non-util: {apiResponse.non_utilisation}; arr.fee: {apiResponse.arrangementfee}; totalCosts: {apiResponse.totalCosts}";

            SetAttribute(entity, "new_iru", apiResponse.iru);
            SetAttribute(entity, "new_non_utilisation", apiResponse.non_utilisation);
            SetAttribute(entity, "new_arrangement_fee", apiResponse.arrangementfee);
            SetAttribute(entity, "new_total_costs_str", apiResponse.totalCosts);
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

        private Deal CallApiMethod(decimal acquisitionCosts, string loanType)
        {
            using (HttpClient client = new HttpClient())
            {
                var builder = new UriBuilder("https://spfapi.azurewebsites.net/api/cashflow/calculatespf");

                var query = HttpUtility.ParseQueryString(string.Empty);
                query["acquisitionCosts"] = acquisitionCosts.ToString();
                query["loanType"] = loanType;

                builder.Query = query.ToString();

                builder.Query = query.ToString();

                string url = builder.ToString();

                TracingService.Trace(url);

                var response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    
                    Deal deal = ReadToObject(result);
                    return deal;
                }

                TracingService.Trace(response.RequestMessage.RequestUri.ToString());
                TracingService.Trace(response.StatusCode.ToString());
                TracingService.Trace(response.Content.ToString());

                throw new InvalidPluginExecutionException("Error accessing API");
            }
        }

        private static Deal ReadToObject(string json)
        {
            var deserializedLoan = new Deal();
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var ser = new DataContractJsonSerializer(deserializedLoan.GetType());
            deserializedLoan = ser.ReadObject(ms) as Deal;
            ms.Close();
            return deserializedLoan;
        }
    }

    [DataContract]
    class Deal
    {
        [DataMember]
        public string acquisitionCosts { get; set; }
        [DataMember]
        public string iru { get; set; }
        [DataMember]
        public string non_utilisation { get; set; }
        [DataMember]
        public string arrangementfee { get; set; }
        [DataMember]
        public string totalCosts { get; set; }
    }
}
