namespace CP.Azure.Samples.MultiComponentRole.Api.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Http;
    using CP.Azure.Samples.MultiComponentRole.Model;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Microsoft.WindowsAzure.Storage.Table;

    public class DownloadController : ApiController
    {
        // POST api/download
        public void Post([FromBody]string value)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
               CloudConfigurationManager.GetSetting(StorageNames.ConnectionStringSettingKey));

            var table = storageAccount
                .CreateCloudTableClient()
                .GetTableReference(StorageNames.EntityTableName);

            // Add file download record
            var downloadEntity = new FileDownloadEntity(value, Guid.NewGuid());
            var insertOperation = TableOperation.Insert(downloadEntity);
            table.Execute(insertOperation);

            // Now create a ticket to download file
            var queue = storageAccount
                .CreateCloudQueueClient()
                .GetQueueReference(StorageNames.TicketQueueName);

            var notification = new CloudQueueMessage(
                string.Format("{0}|<>|{1}", downloadEntity.PartitionKey, downloadEntity.RowKey));
            queue.AddMessage(notification);
        }
    }
}