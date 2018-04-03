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
    [BsonIgnoreExtraElements] // Ignore all non-declared objects
    public class VmSize
    {
        /*
        { 
            OK "name" : "f1s", 
            NA "type" : "vm", 
            OK "contract" : "ri3y", 
            OK "tier" : "standard", 
            OK "cores" : 1, 
            NA "pcores" : 1, 
            OK "mem" : 2, 
            OK "region" : "europe-west", 
            OK "price" : 0.028199999999999999, 
            OK "ACU" : 210, 
            OK "SSD" : "Yes", 
            OK "MaxNics" : 2, 
            OK "Bandwidth" : 750, 
            OK "MaxDataDiskCount" : 4, 
            OK "MaxDataDiskSizeGB" : 16384, 
            OK "MaxDataDiskIops" : 30000, 
            OK "MaxDataDiskThroughputMBs" : 1000, 
            OK "MaxVmIops" : 3200, 
            OK "MaxVmThroughputMBs" : 48, 
            NA "ResourceDiskSizeInMB" : 4096, 
            NA "TempDiskSizeInGB" : 4, 
            NA "TempDiskIops" : 4000, 
            NA "TempDiskReadMBs" : 32, 
            NA "TempDiskWriteMBs" : -1, 
            NA "SAPS2T" : -2, 
            NA "SAPS3T" : -2, 
            NA "HANA" : -2, 
            OK "Hyperthreaded" : "No", 
            OK "OfferName" : "linux-f1s-standard", 
            NA "_id" : "linux-f1s-standard-europe-west-ri3y", 
            OK "price_USD" : 0.028199999999999999, 
            OK "price_EUR" : 0.02378106, 
            OK "price_GBP" : 0.021017713800000001, 
            OK "price_AUD" : 0.03591834, 
            OK "price_JPY" : 2.8763999999999998, 
            OK "price_CAD" : 0.03428838, 
            OK "price_DKK" : 0.17747388, 
            OK "price_CHF" : 0.025467420000000001, 
            OK "price_SEK" : 0.22196784, 
            OK "price_IDR" : 385.21199999999999, 
            OK "price_INR" : 1.8639142499999999, 
            OK "burstable" : "No", 
            OK "isolated" : "No" 
        }
        */

        [Display(Description = "The name of the VM Size")]
        [BsonElement("name")]
        public string Name { get; set; }

        [Display(Description = "The name of the Tier")]
        [BsonElement("tier")]
        public string Tier { get; set; }

        [Display(Description = "The region linked to the pricing & availability of this VM Size")]
        [BsonElement("region")]
        public string Region { get; set; }

        [Display(Description = "The contract used for the pricing this VM Size")]
        [BsonElement("contract")]
        public string Contract { get; set; }

        [Display(Description = "Azure Compute Unit")]
        [BsonElement("ACU")]
        public Int16 ACU { get; set; }

        [Display(Description = "Is the VM SSD (Premium Storage) capable? [Yes/No]")]
        [BsonElement("SSD")]
        public string SSD { get; set; }

        [Display(Description = "# of cores")]
        [BsonElement("cores")]
        public Int16 Cores { get; set; }

        [Display(Description = "# of memory (in GB)")]
        [BsonElement("mem")]
        public Decimal Memory { get; set; }

        [Display(Description = "The price per hour for this VMsize expressed in the indicated currency")]
        [BsonElement("price")]
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public Decimal Price { get; set; }

        [Display(Description = "The currency used for the indicated pricing")]
        public String Currency = "USD";

        [Display(Description = "Is the VM burstable? [Yes/No]")]
        [BsonElement("burstable")]
        public string Burstable { get; set; }

        [Display(Description = "Is the VM isolated? [Yes/No]")]
        [BsonElement("isolated")]
        public string Isolated { get; set; }

        [Display(Description = "The OfferName as it's known for Azure CLI/PowerShell/RestAPI/...")]
        [BsonElement("OfferName")]
        public string OfferName { get; set; }

        [Display(Description = "Are the cores of this VM Hyperthreaded? [Yes/No]")]
        [BsonElement("Hyperthreaded")]
        public string Hyperthreaded { get; set; }

        [Display(Description = "Maximum number of Network Interface Cards")]
        [BsonElement("MaxNics")]
        public Decimal MaxNics { get; set; }

        [Display(Description = "Bandwidth (in Mbps)")]
        [BsonElement("Bandwidth")]
        public Decimal Bandwidth { get; set; }

        [Display(Description = "Maximum number of Data Disks (not including the OS disk)")]
        [BsonElement("MaxDataDiskCount")]
        public Decimal MaxDataDiskCount { get; set; }

        [Display(Description = "Maximum amount of data possible (Max # disks * Max Disks Size)")]
        [BsonElement("MaxDataDiskSizeGB")]
        public Decimal MaxDataDiskSizeGB { get; set; }

        [Display(Description = "Maximum amount of IOPS via standard disks")]
        [BsonElement("MaxDataDiskIops")]
        public Decimal MaxDataDiskIops { get; set; }

        [Display(Description = "Maximum throughput (in MB/s) via standard disks")]
        [BsonElement("MaxDataDiskThroughputMBs")]
        public Decimal MaxDataDiskThroughputMBs { get; set; }

        [Display(Description = "Maximum IOPS on VM Throttling level")]
        [BsonElement("MaxVmIops")]
        public Decimal MaxVmIops { get; set; }

        [Display(Description = "Maximum throughput (in MB/s) on VM Throttling level")]
        [BsonElement("MaxVmThroughputMBs")]
        public Decimal MaxVmThroughputMBs { get; set; }

        [Display(Description = "Price in US Dollar")]
        [BsonElement("price_USD")]
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public Decimal price_USD { get; set; }

        [Display(Description = "Price in Euro")]
        [BsonElement("price_EUR")]
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public Decimal price_EUR { get; set; }

        [Display(Description = "Price in GB Pound")]
        [BsonElement("price_GBP")]
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public Decimal price_GBP { get; set; }

        [Display(Description = "Price in Australian Dollar")]
        [BsonElement("price_AUD")]
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public Decimal price_AUD { get; set; }

        [Display(Description = "Price in Japanese Yen")]
        [BsonElement("price_JPY")]
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public Decimal price_JPY { get; set; }

        [Display(Description = "Price in Canadian Dollar")]
        [BsonElement("price_CAD")]
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public Decimal price_CAD { get; set; }

        [Display(Description = "Price in Danish Krones")]
        [BsonElement("price_DKK")]
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public Decimal price_DKK { get; set; }

        [Display(Description = "Price in Swiss Franks")]
        [BsonElement("price_CHF")]
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public Decimal price_CHF { get; set; }

        [Display(Description = "Price in Swedish Krones")]
        [BsonElement("price_SEK")]
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public Decimal price_SEK { get; set; }

        [Display(Description = "Price in Indonese Rupees")]
        [BsonElement("price_IDR")]
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public Decimal price_IDR { get; set; }

        [Display(Description = "Price in Indian Rupees")]
        [BsonElement("price_INR")]
        [BsonRepresentation(BsonType.Decimal128, AllowTruncation = true)]
        public Decimal price_INR { get; set; }

        // Set the Price & Currency on a requested currency name
        public void setCurrency(string currency)
        {
            Currency = currency;
            Price = Convert.ToDecimal(this.GetType().GetProperty("price_" + currency).GetValue(this, null));
        }
    }

    public static class GetVmSize
    {
        [FunctionName("GetVmSize")]
        [ResponseType(typeof(VmSize))]
        [Display(Name = "GetVmSize", Description = "Find the best VM T-Shirt Size for your given specifications")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, Route = null)]HttpRequestMessage req, get, post, options, TraceWriter log)
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
            // Cores (Min) #
            decimal cores = Convert.ToDecimal(GetParameter("cores", "0", req));
            cores = SetMinimum(cores, 0);
            log.Info("Cores : "+cores.ToString());
            // Physical Cores (Min) #
            decimal pcores = Convert.ToDecimal(GetParameter("pcores", "0", req));
            pcores = SetMinimum(pcores, 0);
            log.Info("PCores : " + pcores.ToString());
            // Azure Compute Unit (Min) #
            decimal acu = Convert.ToDecimal(GetParameter("acu", "0", req));
            acu = SetMinimum(pcores, 0);
            log.Info("ACU : " + acu.ToString());
            // Memory (Min) #
            decimal memory = Convert.ToDecimal(GetParameter("memory", "0", req));
            memory = SetMinimum(memory, 0);
            log.Info("Memory : " + memory.ToString());
            // NICS (Min) #
            decimal nics = Convert.ToDecimal(GetParameter("nics", "0", req));
            nics = SetMinimum(nics, 0);
            log.Info("NICs : " + nics.ToString());
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
            decimal results = Convert.ToDecimal(GetParameter("maxresults", "5", req));
            results = SetMinimum(results, 1);
            results = SetMaximum(results, 100);
            log.Info("Results : " + results.ToString());
            // Right Sizing CPU 95pct (Peak Util) %
            decimal avgcpupeak = Convert.ToDecimal(GetParameter("avgcpupeak", "100", req));
            avgcpupeak = SetMinimum(avgcpupeak, 0);
            avgcpupeak = SetMaximum(avgcpupeak, 100);
            log.Info("AvgCpuPeak : " + avgcpupeak.ToString());
            // Right Sizing Memory 95pct (Peak GB) %
            decimal avgmempeak = Convert.ToDecimal(GetParameter("avgmempeak", "100", req));
            avgmempeak = SetMinimum(avgmempeak, 0);
            avgmempeak = SetMaximum(avgmempeak, 100);
            log.Info("AvgMemPeak : " + avgmempeak.ToString());
            
            // Right Sizing CPU
            cores = cores * avgcpupeak / 100;
            pcores = pcores * avgcpupeak / 100;
            log.Info("Cores* : " + cores.ToString());
            log.Info("PCores* : " + pcores.ToString());

            // Right Sizing Memory
            memory = memory * avgmempeak / 100;
            log.Info("Memory* : " + memory.ToString());

            var filterBuilder = Builders<BsonDocument>.Filter;
            var filter  = filterBuilder.Eq("type", "vm")
                        & filterBuilder.Gte("cores", Convert.ToInt16(cores)) 
                        & filterBuilder.Gte("mem", Convert.ToInt16(memory))
                        & filterBuilder.Gte("pcores", Convert.ToInt16(pcores))
                        & filterBuilder.Gte("ACU", Convert.ToInt16(acu))
                        & filterBuilder.Gte("MaxNics", Convert.ToInt16(nics))
                        & filterBuilder.Gte("MaxVmIops", Convert.ToInt16(iops))
                        & filterBuilder.Gte("MaxVmThroughputMBs", Convert.ToInt16(throughput))
                        & filterBuilder.Gte("MaxDataDiskSizeGB", Convert.ToInt16(data))
                        & filterBuilder.Gte("TempDiskSizeInGB", Convert.ToInt16(temp))
                        & filterBuilder.Eq("region", region)
                        & filterBuilder.Eq("tier", tier)
                        & filterBuilder.In("Hyperthreaded", htfilter)
                        & filterBuilder.In("SSD", ssdfilter)
                        //& filterBuilder.In("burstable", burstablefilter)
                        ;
            var sort = Builders<BsonDocument>.Sort.Ascending("price");
            var cursor = collection.Find<BsonDocument>(filter).Sort(sort).Limit(Convert.ToInt16(results)).ToCursor();

            // Get results and put them into a list of objects
            List<VmSize> documents = new List<VmSize>();
            foreach (var document in cursor.ToEnumerable())
            {
                log.Info(document.ToString());
                VmSize myVmSize = BsonSerializer.Deserialize<VmSize>(document);
                log.Info(myVmSize.Name);
                myVmSize.setCurrency(currency);
                documents.Add(myVmSize);
            }

            // Convert to JSON & return it
            var json = JsonConvert.SerializeObject(documents, Formatting.Indented);
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
        static public string[] YesNoAll(string value) {
            string[] response = new string[2];

            switch (value.ToLower()) {
                case "yes":
                    response[0] = "Yes";
                    response[1] = "Yes";
                    break;
                case "no":
                    response[0] = "No";
                    response[1] = "No";
                    break;
                default:
                    response[0] = "Yes";
                    response[1] = "No";
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
