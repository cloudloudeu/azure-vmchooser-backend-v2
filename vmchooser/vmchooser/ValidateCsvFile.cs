using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace vmchooser
{
    public static class ValidateCsvFile
    {
        [FunctionName("ValidateCsvFile")]
        public static void Run([BlobTrigger("input/{name}", Connection = "vmchooser-sa-blob-input")]string myBlob, string name, TraceWriter log)
        {
            log.Info($"VMchooser Bulk Mapping \n Name:{name} \n Size: {myBlob.Length} Bytes");

            int expected_field_count = System.Int32.Parse(System.Environment.GetEnvironmentVariable("vmchooser-csv-fieldcount"));

            using (StringReader reader = new StringReader(myBlob))
            {
                string line;
                string vmchooser_sa_queue_batch = System.Environment.GetEnvironmentVariable("vmchooser-sa-queue-batch");
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(vmchooser_sa_queue_batch);
                CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                CloudQueue queue = queueClient.GetQueueReference("vmchooserbatch");
                queue.CreateIfNotExists();
                while ((line = reader.ReadLine()) != null)
                {
                    char delimiter = ',';
                    string[] fields = line.Split(delimiter);
                    int field_count = fields.Length;
                    string field_count_str = field_count.ToString();
                    if (field_count == expected_field_count)
                    {
                        CloudQueueMessage message = new CloudQueueMessage(line);
                        queue.AddMessageAsync(message);
                        log.Info("Message added to the queue");
                    }
                    else
                    {
                        log.Error("Wrong amount of fields ({field_count_str}) received. Was {delimiter} used as a delimiter?");
                    }
                }
            }
        }
    }
}
