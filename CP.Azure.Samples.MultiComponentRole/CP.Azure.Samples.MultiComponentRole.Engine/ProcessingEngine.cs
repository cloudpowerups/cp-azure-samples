namespace CP.Azure.Samples.MultiComponentRole.Engine
{
    using System;
    using System.IO;
    using System.Net;
    using System.ServiceProcess;
    using System.Threading;
    using System.Threading.Tasks;
    using CP.Azure.Samples.MultiComponentRole.Model;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Table;

    public partial class ProcessingEngine : ServiceBase
    {
        readonly CancellationTokenSource _token;
        readonly Task _processingTask;

        readonly CloudStorageAccount _storageAccount;

        public ProcessingEngine()
        {
            InitializeComponent();

            _token = new CancellationTokenSource();
            _processingTask = new Task(ProcessNewTickets, _token);

            _storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting(StorageNames.ConnectionStringSettingKey));
        }

        public void StartProcessing(string[] args)
        {
            // Let's ensure table, queue and blob exists
            _storageAccount
                .CreateCloudTableClient()
                .GetTableReference(StorageNames.EntityTableName)
                .CreateIfNotExists();
            _storageAccount
                .CreateCloudQueueClient()
                .GetQueueReference(StorageNames.TicketQueueName)
                .CreateIfNotExists();
            var containerReference = _storageAccount
                .CreateCloudBlobClient()
                .GetContainerReference(StorageNames.BlobContainerName);
            containerReference
                .CreateIfNotExists();
            // Allow public access
            containerReference.SetPermissions(new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                });

            // Start the task
            _processingTask.Start();
        }

        public void EndProcessing()
        {
            // Trigger stopping and wait until processing completes
            _token.Cancel();
            _processingTask.Wait();
        }

        protected override void OnStart(string[] args)
        {
            StartProcessing(args);
        }

        protected override void OnStop()
        {
            EndProcessing();
        }


        void ProcessNewTickets(object obj)
        {
            // Keep processing until stopped
            while (!_token.IsCancellationRequested)
            {
                // Get table, queue and blob references
                var table = _storageAccount
                    .CreateCloudTableClient()
                    .GetTableReference(StorageNames.EntityTableName);
                var queue = _storageAccount
                    .CreateCloudQueueClient()
                    .GetQueueReference(StorageNames.TicketQueueName);
                var container = _storageAccount
                    .CreateCloudBlobClient()
                    .GetContainerReference(StorageNames.BlobContainerName);

                var incomingTicket = queue.GetMessage(TimeSpan.FromSeconds(45)); // Take it for 45 seconds max
                if (incomingTicket != null)
                {
                    // Load the message (ticket must be in format <partition|<>|row>)
                    var messageKey = incomingTicket.AsString.Split(new [] {"|<>|"}, StringSplitOptions.RemoveEmptyEntries);
                    var retrieveOperation = TableOperation.Retrieve<FileDownloadEntity>(messageKey[0], messageKey[1]);
                    var entity = (FileDownloadEntity)table.Execute(retrieveOperation).Result;

                    // Let's download it and save the result
                    try
                    {
                        // Download it
                        // TODO: optimize downloading
                        var fileUri = new Uri(entity.SourceUrl);
                        var downloadedFile = new WebClient().DownloadData(fileUri);
                        Console.WriteLine("Downloaded '{0}' according to request #{1}", entity.SourceUrl,
                            entity.RowKey);

                        var fileName = fileUri.Segments[fileUri.Segments.Length - 1];
                        var blobLocation = string.Format("{0}/{1}", entity.RowKey, fileName);
                        var blobReference = container.GetBlockBlobReference(blobLocation);

                        // Upload to storage
                        using (var stream = new MemoryStream(downloadedFile))
                        {
                            blobReference.UploadFromStream(stream);

                            Console.WriteLine("Uploaded to storage into {0}. Request #{1}", blobLocation, entity.RowKey);
                        }

                        // Save location in storage
                        entity.ProcessedBy = Environment.MachineName;
                        entity.SavedLocation = blobReference.Uri.ToString();
                        entity.IsDownloaded = true;
                        
                        // Push changes back to table
                        var replaceOperation = TableOperation.Replace(entity);
                        table.Execute(replaceOperation);

                        // Ticket processed -> delete it
                        queue.DeleteMessage(incomingTicket);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error processing ticket: {0}", ex);
                    }

                }

                Thread.Sleep(TimeSpan.FromSeconds(5)); // Grab next ticket in 5 seconds
            }
        }
    }
}
