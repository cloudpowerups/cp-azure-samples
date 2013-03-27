namespace CP.RavenDB.ReplicationSetup
{
    using System;
    using System.Linq;
    using System.Threading;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Raven.Abstractions.Data;
    using Raven.Abstractions.Replication;
    using Raven.Client.Document;

    class Program
    {
        static void Main(string[] args)
        {
            RavenConnectionStringOptions writesDbConnection;
            RavenConnectionStringOptions readsDbConnection;
            try
            {
                // Load connection string to WritesDB
                var connectionString = CloudConfigurationManager.GetSetting("WritesDBConnectionString");
                Console.WriteLine(connectionString);
                // Parse it
                var parser = ConnectionStringParser<RavenConnectionStringOptions>.FromConnectionString(connectionString);
                parser.Parse();
                writesDbConnection = parser.ConnectionStringOptions;

                // Load connection string to WritesDB
                connectionString = CloudConfigurationManager.GetSetting("ReadsDBConnectionString");
                Console.WriteLine(connectionString);
                // Parse it
                parser = ConnectionStringParser<RavenConnectionStringOptions>.FromConnectionString(connectionString);
                parser.Parse();
                readsDbConnection = parser.ConnectionStringOptions;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to read DB connection strings. Error: {0}", ex);
                return;
            }

            var replicationUpdated = false;
            while (!replicationUpdated)
            {
                try
                {
                    // Connect
                    var documentStore = new DocumentStore();
                    SetConnectionOptions(documentStore, writesDbConnection);
                    documentStore.Initialize();

                    // Update replication
                    using (var session = documentStore.OpenSession())
                    {
                        var replicationDoc = session.Load<ReplicationDocument>("Raven/Replication/Destinations");
                        if (replicationDoc == null)
                        {
                            replicationDoc = new ReplicationDocument();
                            session.Store(replicationDoc);
                        }

                        // Read replication endpoint
                        var replicationEndpoint = RoleEnvironment.CurrentRoleInstance.InstanceEndpoints["RavenReplication"].IPEndpoint;
                        var replicationUrl = string.Format("http://{0}", replicationEndpoint);
                        Console.WriteLine("Will setup replication to: {0}", replicationUrl);

                        // Add new replication destination
                        if (!replicationDoc.Destinations.Any(p => p.Url == replicationUrl 
                            && p.Database == readsDbConnection.DefaultDatabase))
                        {
                            Console.WriteLine("Adding...");
                            replicationDoc.Destinations.Add(new ReplicationDestination
                                {
                                    ApiKey = readsDbConnection.ApiKey,
                                    Database = readsDbConnection.DefaultDatabase,
                                    Disabled = false,
                                    Url = replicationUrl,
                                    ClientVisibleUrl = readsDbConnection.Url
                                });
                            session.SaveChanges();
                        }
                    }

                    documentStore.Dispose();

                    replicationUpdated = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Attempt to update replication on WritesDB failed. Error: {0}", ex);
                    Thread.Sleep(5000); // Wait 5 seconds and retry
                }
            }
        }

        private static void SetConnectionOptions(DocumentStore store, RavenConnectionStringOptions options)
        {
            if (options.ResourceManagerId != Guid.Empty)
            {
                store.ResourceManagerId = options.ResourceManagerId;
            }
            if (options.Credentials != null)
            {
                store.Credentials = options.Credentials;
            }
            if (!string.IsNullOrEmpty(options.Url))
            {
                store.Url = options.Url;
            }
            if (!string.IsNullOrEmpty(options.DefaultDatabase))
            {
                store.DefaultDatabase = options.DefaultDatabase;
            }
            if (!string.IsNullOrEmpty(options.ApiKey))
            {
                store.ApiKey = options.ApiKey;
            }
            store.EnlistInDistributedTransactions = options.EnlistInDistributedTransactions;
        }
    }
}
