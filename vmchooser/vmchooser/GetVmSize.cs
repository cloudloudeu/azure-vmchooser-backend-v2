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

namespace vmchooser
{
    public static class GetVmSize
    {
        [FunctionName("GetVmSize")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            // CosmosDB Parameters, retrieved via environment variables
            string databaseName = Environment.GetEnvironmentVariable("cosmosdbDatabaseName");
            string collectionName = Environment.GetEnvironmentVariable("cosmosdbCollectionName");
            string mongodbConnectionString = Environment.GetEnvironmentVariable("cosmosdbMongodbConnectionString");

            // This endpoint is valid for all MongoDB
            var client = new MongoClient(mongodbConnectionString);
            var database = client.GetDatabase(databaseName);
            var collection = database.GetCollection<BsonDocument>(collectionName);

            // Get Parameters
            dynamic contentdata = await req.Content.ReadAsAsync<object>();
            // Cores (Min) #
            decimal cores = Convert.ToDecimal(GetParameter("cores", "0", req));
            cores = SetMinimum(cores, 0);
            log.Info("Cores : "+cores.ToString());
            // Physical Cores (Min) #
            decimal pcores = Convert.ToDecimal(GetParameter("pcores", "0", req));
            pcores = SetMinimum(pcores, 0);
            log.Info("PCores : " + pcores.ToString());
            // Memory (Min) #
            decimal memory = Convert.ToDecimal(GetParameter("memory", "0", req));
            memory = SetMinimum(memory, 0);
            log.Info("Memory : " + memory.ToString());
            // IOPS (Min) #
            decimal iops = Convert.ToDecimal(GetParameter("iops", "-127", req));
            iops = SetMinimum(iops, -127);
            log.Info("IOPS : " + iops.ToString());
            // Throughput (Min) #
            decimal throughput = Convert.ToDecimal(GetParameter("throughput", "-127", req));
            throughput = SetMinimum(throughput, -127);
            log.Info("Throughput : " + throughput.ToString());
            // Data (Disk Capacity) (Min) #
            decimal data = Convert.ToDecimal(GetParameter("data", "-127", req));
            data = SetMinimum(data, -127);
            log.Info("Data : " + data.ToString());
            // Temp (Disk Capacity) (Min) #
            decimal temp = Convert.ToDecimal(GetParameter("temp", "-127", req));
            temp = SetMinimum(temp, -127);
            log.Info("Temp : " + temp.ToString());
            // Hyperthreading #
            string ht = GetParameter("ht", "all", req).ToLower();
            string[] htfilter = new string[2];
            htfilter = YesNoAll(ht);
            log.Info("HyperTreading : " + ht.ToString());
            log.Info("HyperTreading[0] : " + htfilter[0]);
            log.Info("HyperTreading[1] : " + htfilter[1]);
            // Tier #
            string tier = GetParameter("tier", "standard", req).ToLower();
            log.Info("Tier : " + tier.ToString());
            // Ssd #
            string ssd = GetParameter("ssd", "all", req).ToLower();
            string[] ssdfilter = new string[2];
            ssdfilter = YesNoAll(ssd);
            log.Info("SSD : " + ssd.ToString());
            log.Info("SSD[0] : " + ssdfilter[0]);
            log.Info("SSD[1] : " + ssdfilter[1]);
            // Burstable #
            string burstable = GetParameter("burstable", "all", req).ToLower();
            string[] burstablefilter = new string[2];
            burstablefilter = YesNoAll(burstable);
            log.Info("Burstable : " + burstable.ToString());
            log.Info("Burstable[0] : " + burstablefilter[0]);
            log.Info("Burstable[1] : " + burstablefilter[1]);
            // Region #
            string region = GetParameter("region", "europe-west", req).ToLower();
            log.Info("Region : " + region.ToString());
            // Currency #
            string currency = GetParameter("currency", "EUR", req).ToUpper();
            log.Info("Currency : " + currency.ToString());
            // Contract #
            string contract = GetParameter("contract", "payg", req).ToLower();
            log.Info("Contract : " + contract.ToString());
            // Results (Max) #
            decimal results = Convert.ToDecimal(GetParameter("results", "5", req));
            results = SetMinimum(results, 1);
            results = SetMaximum(results, 100);
            log.Info("Results : " + results.ToString());

            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter  = filterBuilder.Gte("cores", Convert.ToInt16(cores)) 
                        & filterBuilder.Gte("mem", Convert.ToInt16(memory))
                        & filterBuilder.Gte("pcores", Convert.ToInt16(pcores))
                        & filterBuilder.Gte("MaxVmIops", Convert.ToInt16(iops))
                        & filterBuilder.Gte("MaxVmThroughputMBs", Convert.ToInt16(throughput))
                        & filterBuilder.Gte("MaxDataDiskSizeGB", Convert.ToInt16(data))
                        & filterBuilder.Gte("TempDiskSizeInGB", Convert.ToInt16(temp))
                        & filterBuilder.Eq("region", region)
                        ;
            var sort = Builders<BsonDocument>.Sort.Ascending("price");
            var cursor = collection.Find(filter).Sort(sort).Limit(Convert.ToInt16(results)).ToCursor();
            foreach (var document in cursor.ToEnumerable())
            {
                log.Info(document.ToString());
            }

            return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body");
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
        static public string[] YesNoAll(string value) {
            string[] response = new string[2];

            switch (value.ToLower()) {
                case "yes":
                    response[0] = "yes";
                    response[1] = "yes";
                    break;
                case "no":
                    response[0] = "no";
                    response[1] = "no";
                    break;
                default:
                    response[0] = "yes";
                    response[1] = "no";
                    break;
            }

            return response;
        }

        static public decimal SetMinimum(decimal value, decimal minimumvalue)
        {
            if (value < minimumvalue) { value = minimumvalue; }
            return value;
        }
        static public decimal SetMaximum(decimal value, decimal maximumvalue)
        {
            if (value > maximumvalue) { value = maximumvalue;  }
            return value;
        }
    }
}
