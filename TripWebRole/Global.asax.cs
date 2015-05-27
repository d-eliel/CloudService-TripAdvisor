using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace TripWebRole
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            CreateImagesBlobContainer();
            CreateImagesQueue();
            ExecuteCodeFirstMigrations();
        }

        /// <summary>
        /// 
        /// </summary>
        private void CreateImagesQueue()
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("AdvisorStorageConnection"));
            CloudQueueClient queueClient = account.CreateCloudQueueClient();
            CloudQueue imagesQueue = queueClient.GetQueueReference("images");
            imagesQueue.CreateIfNotExists();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ExecuteCodeFirstMigrations()
        {
            var commonConfiguration = new TripCommon.Migrations.Configuration();
            DbMigrator commonMigrator = new DbMigrator(commonConfiguration);
            commonMigrator.Update();
        }

        /// <summary>
        /// 
        /// </summary>
        private void CreateImagesBlobContainer()
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("AdvisorStorageConnection"));
            CloudBlobClient client = account.CreateCloudBlobClient();
            CloudBlobContainer container = client.GetContainerReference("images");
            if (container.CreateIfNotExists())
            {
                container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob }
                );
            }
        }
    }
}
