namespace CP.Azure.Samples.MultiComponentRole.Model
{
    using System;

    public class StorageNames
    {
        public const string ConnectionStringSettingKey = "StorageConnectionString";

        public const string EntityTableName = "downloads";
        public const string TicketQueueName = "download-requests";
        public const string BlobContainerName = "downloaded-files";
    }
}
