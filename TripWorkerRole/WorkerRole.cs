using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using TripCommon;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

/* ******************************* *
 *      Eliel Dabush 204280036     *
 * ****************************** */
namespace TripWorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        private TripDbContext db;
        private CloudBlobContainer imagesBlobContainer;
        private CloudQueue imagesQueue;

        private void InitializeStorage()
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("AdvisorStorageConnection"));
            // blob container
            CloudBlobClient client = account.CreateCloudBlobClient();
            imagesBlobContainer = client.GetContainerReference("images");
            // queue
            CloudQueueClient qClient = account.CreateCloudQueueClient();
            imagesQueue = qClient.GetQueueReference("images");
        }

        public override void Run()
        {
            Trace.TraceInformation("TripWorkerRole is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            string dbConnString = CloudConfigurationManager.GetSetting("TripDbConnString");
            db = new TripDbContext(dbConnString);
            InitializeStorage();
            
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("TripWorkerRole has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("TripWorkerRole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("TripWorkerRole has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            CloudQueueMessage msg = null;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    msg = imagesQueue.GetMessage();
                    if (msg != null)
                    {
                        Trace.TraceInformation("Got new message");
                        ProcessQueueMessage(msg);
                    }
                    else
                    {
                        await Task.Delay(1000);
                    }
                }
                catch (StorageException)
                {
                    // Handle poison messages
                    if (msg != null && msg.DequeueCount > 5)
                    {
                        imagesQueue.DeleteMessage(msg);
                    }
                    Thread.Sleep(5000);
                }
            }

        }

        private void ProcessQueueMessage(CloudQueueMessage msg)
        {
            // Read the product ID from the message
            int adviceId = int.Parse(msg.AsString);
            TripAdvice advice = db.TripAdviceTable.Find(adviceId);
            if (advice == null)
            {
                throw new Exception(String.Format("AdviceId {0} not found, can't create thumbnail", adviceId.ToString()));
            }

            // Get the product's image blob
            string blobName = new Uri(advice.ImageURL).Segments.Last();
            CloudBlockBlob inputBlob = imagesBlobContainer.GetBlockBlobReference(blobName);

            string thumbnailName = Path.GetFileNameWithoutExtension(inputBlob.Name) + "thumb.jpg";
            CloudBlockBlob outputBlob = imagesBlobContainer.GetBlockBlobReference(thumbnailName);
            using (Stream input = inputBlob.OpenRead())
            using (Stream output = outputBlob.OpenWrite())
            {
                ConvertImageToThumbnailJPG(input, output);
                outputBlob.Properties.ContentType = "image/jpeg";
            }

            advice.ThumbnailURL = outputBlob.Uri.ToString();
            db.SaveChanges();

            imagesQueue.DeleteMessage(msg);
            Trace.TraceInformation("Thumbnail created: " + thumbnailName);
        }

        private void ConvertImageToThumbnailJPG(Stream input, Stream output)
        {
            int thumbnailsize = 80;
            int width;
            int height;
            Bitmap originalImage = new Bitmap(input);

            if (originalImage.Width > originalImage.Height)
            {
                width = thumbnailsize;
                height = thumbnailsize * originalImage.Height / originalImage.Width;
            }
            else
            {
                height = thumbnailsize;
                width = thumbnailsize * originalImage.Width / originalImage.Height;
            }
            Bitmap thumbnailImage = null;
            try
            {
                thumbnailImage = new Bitmap(width, height);

                using (Graphics graphics = Graphics.FromImage(thumbnailImage))
                {
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.DrawImage(originalImage, 0, 0, width, height);
                }
                thumbnailImage.Save(output, ImageFormat.Jpeg);
            }
            finally
            {
                if (thumbnailImage != null)
                {
                    thumbnailImage.Dispose();
                }
            }
        }
    }
}
