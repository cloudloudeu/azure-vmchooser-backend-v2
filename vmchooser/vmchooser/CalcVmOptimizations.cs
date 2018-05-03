using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Collections.Generic;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System.Text;
using System.Web.Http.Description;
using System.ComponentModel.DataAnnotations;

namespace vmchooser
{
    public class VmSizeOptimizer
    {
        [Display(Description = "The name of the VM Size")]
        [BsonElement("name")]
        public string Name { get; set; }

        [Display(Description = "The name of the Tier")]
        [BsonElement("tier")]
        public string Tier { get; set; }

        [Display(Description = "The region linked to the pricing & availability of this VM Size")]
        [BsonElement("region")]
        public string Region { get; set; }

        [Display(Description = "The Price in PAYG with the windows license included")]
        public decimal Price_Windows_PAGY { get; set; }

        [Display(Description = "The Price in RI1Y with the windows license included")]
        public decimal Price_Windows_RI1Y { get; set; }

        [Display(Description = "The Price in RI3Y with the windows license included")]
        public decimal Price_Windows_RI3Y { get; set; }

        [Display(Description = "The Price in PAYG with no OS license")]
        public decimal Price_Linux_PAGY { get; set; }

        [Display(Description = "The Price in RI1Y with no OS license")]
        public decimal Price_Linux_RI1Y { get; set; }

        [Display(Description = "The Price in RI3Y with no OS license")]
        public decimal Price_Linux_RI3Y { get; set; }

        [Display(Description = "The currency used for the indicated pricing")]
        public String Currency = "EUR";

        [Display(Description = "The price diff between linux & windows for PAGY")]
        public decimal Diff_Os_PAGY { get; set; }
        [Display(Description = "The price diff between linux & windows for RI1Y")]
        public decimal Diff_Os_RI1Y { get; set; }
        [Display(Description = "The price diff between linux & windows for RI3Y")]
        public decimal Diff_Os_RI3Y { get; set; }

        [Display(Description = "The price diff between PAGY & RI1Y for Windows")]
        public decimal Diff_Windows_RI1Y { get; set; }
        [Display(Description = "The price diff between PAGY & RI3Y for Windows")]
        public decimal Diff_Windows_RI3Y { get; set; }

        [Display(Description = "The price diff between PAGY & RI1Y for Linux")]
        public decimal Diff_Linux_RI1Y { get; set; }
        [Display(Description = "The price diff between PAGY & RI3Y for Linux")]
        public decimal Diff_Linux_RI3Y { get; set; }

        public void SetDifferences()
        {
            Diff_Os_PAGY = Price_Windows_PAGY - Price_Linux_PAGY;
            Diff_Os_RI1Y = Price_Windows_RI1Y - Price_Linux_RI1Y;
            Diff_Os_RI3Y = Price_Windows_RI3Y - Price_Linux_RI3Y;

            Diff_Windows_RI1Y = Price_Windows_PAGY - Price_Windows_RI1Y;
            Diff_Windows_RI3Y = Price_Windows_PAGY - Price_Windows_RI3Y;

            Diff_Linux_RI1Y = Price_Linux_PAGY - Price_Linux_RI1Y;
            Diff_Linux_RI3Y = Price_Linux_PAGY - Price_Linux_RI3Y;
        }

        public void SetPrice(decimal InputPrice, string InputContract, string InputOs)
        {
            switch (InputOs.ToLower()) {
                case "linux":
                    SetPriceLinux(InputPrice, InputContract);
                    break;
                case "windows":
                    SetPriceWindows(InputPrice, InputContract);
                    break;
            }
        }
        public void SetPriceLinux(decimal InputPrice, string InputContract)
        {
            switch (InputContract.ToLower())
            {
                case "payg":
                    Price_Linux_PAGY = InputPrice;
                    break;
                case "ri1y":
                    Price_Linux_RI1Y = InputPrice;
                    break;
                case "ri3y":
                    Price_Linux_RI3Y = InputPrice;
                    break;
            }
        }
        public void SetPriceWindows(decimal InputPrice, string InputContract)
        {
            switch (InputContract.ToLower())
            {
                case "payg":
                    Price_Windows_PAGY = InputPrice;
                    break;
                case "ri1y":
                    Price_Windows_RI1Y = InputPrice;
                    break;
                case "ri3y":
                    Price_Windows_RI3Y = InputPrice;
                    break;
            }
        }
    }

    public static class CalcVmOptimizations
    {
        [FunctionName("CalcVmOptimizations")]
        [ResponseType(typeof(VmSize))]
        [Display(Name = "CalcVmOptimizations", Description = "Find the best VM T-Shirt Size for your given specifications")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", "options", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            // CosmosDB Parameters, retrieved via environment variables
            string databaseName = Environment.GetEnvironmentVariable("cosmosdbDatabaseName");
            string collectionName = Environment.GetEnvironmentVariable("cosmosdbCollectionName");
            string mongodbConnectionString = Environment.GetEnvironmentVariable("cosmosdbMongodbConnectionString");

            // Set BSON AutoMap
            //BsonClassMap.RegisterClassMap<VmSize>();

            // This endpoint is valid for all MongoDB
            var client = new MongoClient(mongodbConnectionString);
            var database = client.GetDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);

            // Get Parameters
            dynamic contentdata = await req.Content.ReadAsAsync<object>();
            // Tier #
            string tier = GetParameter("tier", "standard", req).ToLower();
            log.Info("Tier : " + tier.ToString());
            // Region #
            string region = GetParameter("region", "europe-west", req).ToLower();
            log.Info("Region : " + region.ToString());
            // Currency #
            string currency = GetParameter("currency", "EUR", req).ToUpper();
            log.Info("Currency : " + currency.ToString());
            
            // Name
            string vmsize = GetParameter("vmsize", "a0", req).ToLower();
            log.Info("Name : " + vmsize.ToString());

            // Get price for Linux
            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter = filterBuilder.Eq("type", "vm")
                        & filterBuilder.Eq("region", region)
                        & filterBuilder.Eq("tier", tier)
                        & filterBuilder.Eq("name", vmsize)
                        ;

            var cursor = collection.Find<BsonDocument>(filter).ToCursor();

            // Get results and put them into a list of objects
            var results = new VmSizeOptimizer();
            results.Currency = currency;
            results.Name = vmsize;
            results.Region = region;
            results.Tier = tier;

            foreach (var document in cursor.ToEnumerable())
            {
                log.Info(document.ToString());
                VmSize myVmSize = BsonSerializer.Deserialize<VmSize>(document);
                log.Info("Price :" + myVmSize.Price + " - Contract : " + myVmSize.Contract + " - OS : " + myVmSize.OperatingSystem);
                results.SetPrice(myVmSize.Price, myVmSize.Contract, myVmSize.OperatingSystem);
            }
            results.SetDifferences();

            // Convert to JSON & return it
            var json = JsonConvert.SerializeObject(results, Formatting.Indented);
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }

        static public string GetParameter(string name, string defaultvalue, HttpRequestMessage req)
        {
            string value = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, name, true) == 0)
                .Value;
            if (String.IsNullOrEmpty(value))
            {
                value = defaultvalue;
            }

            return value;
        }
    }
}
