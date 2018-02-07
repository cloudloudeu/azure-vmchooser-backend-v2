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
            // Cores #
            decimal cores = Convert.ToDecimal(GetParameter("cores", "0", req));
            cores = SetMinimum(cores, 0);
            log.Info("Cores : "+cores.ToString());
            // Physical Cores #
            decimal pcores = Convert.ToDecimal(GetParameter("pcores", "0", req));
            pcores = SetMinimum(pcores, 0);
            log.Info("PCores : " + pcores.ToString());
            // Memory #
            decimal memory = Convert.ToDecimal(GetParameter("memory", "0", req));
            memory = SetMinimum(memory, 0);
            log.Info("Memory : " + memory.ToString());
            // IOPS #
            decimal iops = Convert.ToDecimal(GetParameter("iops", "-127", req));
            iops = SetMinimum(iops, -127);
            log.Info("IOPS : " + iops.ToString());
            // Data (Disk Capacity) #
            decimal data = Convert.ToDecimal(GetParameter("data", "-127", req));
            data = SetMinimum(data, -127);
            log.Info("Data : " + data.ToString());
            // Temp (Disk Capacity) #
            decimal temp = Convert.ToDecimal(GetParameter("temp", "-127", req));
            temp = SetMinimum(temp, -127);
            log.Info("Temp : " + temp.ToString());


            return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a name on the query string or in the request body");
        }

        static public string GetParameter(string name, string defaultvalue, HttpRequestMessage req) {
            string value = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, name, true) == 0)
                .Value;
            if (String.IsNullOrEmpty(value)) {
                value = defaultvalue;
            }

            return value;
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
