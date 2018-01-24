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
    public static class GetVmDetail
    {
        [FunctionName("GetVmDetail")]
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

            // parse query parameter
            string name = req.GetQueryNameValuePairs()
                .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
                .Value;

            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();

            // Set name to query string or body data
            name = name ?? data?.name;

            // First Logging
            log.Info("Retrieving the VMsize for " + name);

            // Query Builder
            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Eq("name", name);

            using (var cursor = await collection.Find(filter).Limit(1).ToCursorAsync())
            {
                while (await cursor.MoveNextAsync())
                {
                    foreach (var doc in cursor.Current)
                    {
                        object result = Newtonsoft.Json.Linq.JObject.Parse(doc.ToString());
                        return req.CreateResponse(HttpStatusCode.OK, result, new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
                    }
                }
            }

            return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a valid vm name on the query string or in the request body"); 
        }
    }
}
