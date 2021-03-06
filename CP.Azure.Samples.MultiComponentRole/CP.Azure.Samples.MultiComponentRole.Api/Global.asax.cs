﻿using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace CP.Azure.Samples.MultiComponentRole.Api
{
    using CP.Azure.Samples.MultiComponentRole.Model;
    using Microsoft.WindowsAzure;
    using Microsoft.WindowsAzure.Storage;

    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Let's ensure table and queue exists
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting(StorageNames.ConnectionStringSettingKey));
            storageAccount
                .CreateCloudTableClient()
                .GetTableReference(StorageNames.EntityTableName)
                .CreateIfNotExists();
            storageAccount
                .CreateCloudQueueClient()
                .GetQueueReference(StorageNames.TicketQueueName)
                .CreateIfNotExists();
        }
    }
}