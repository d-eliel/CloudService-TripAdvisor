using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TripCommon;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Azure;
using System.IO;
using TripWebRole.Models;

/* ******************************* *
 *      Eliel Dabush 204280036     *
 * ******************************  */
namespace TripWebRole.Controllers
{
    public class AdvisorsController : Controller
    {
        private TripDbContext db = new TripDbContext();
        private CloudBlobContainer imagesContainer;
        private CloudQueue imagesQueue;

        public AdvisorsController()
        {
            InitializeStorage();
        }

        private void InitializeStorage()
        {
            CloudStorageAccount account = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("AdvisorStorageConnection"));
            // blob container
            CloudBlobClient client = account.CreateCloudBlobClient();
            imagesContainer = client.GetContainerReference("images");
            // queue
            CloudQueueClient qClient = account.CreateCloudQueueClient();
            imagesQueue = qClient.GetQueueReference("images");
        }

        // GET: Advisors
        public ActionResult Index()
        {
            ManageAdvisorCache advisorCache = new ManageAdvisorCache();
            return View(advisorCache.GetAdvisesList());
            // async Task<ActionResult>
            //return View(await db.TripAdviceTable.ToListAsync());
        }

        // GET: Advisors/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest); 
            }
            
            TripAdvice tripAdvice = await db.TripAdviceTable.FindAsync(id);
            
            if(tripAdvice == null)
            {
                return HttpNotFound();  
            }

            return View(tripAdvice);
        }

        // GET: Advisors/Create
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// uploading image to azure blob - async 
        /// </summary>
        /// <param name="imageFile"></param>
        /// <returns></returns>
        private async Task<CloudBlockBlob> UploadBlobAsync(HttpPostedFileBase imageFile)
        {
            // Create a unique name for the blob
            string blobName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);

            // Create the blob
            CloudBlockBlob blob = imagesContainer.GetBlockBlobReference(blobName);

            // Upload the file to the blob
            using (Stream stream = imageFile.InputStream)
            {
                await blob.UploadFromStreamAsync(stream);
            }
            return blob;
        }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "TripAdviceID,UserName,AdviceType,PlaceName,PrimesCount,GeoLocation,Country,City,AdviceText,RankPlace,ImageURL,ThumbnailURL,AdviceDate")] TripAdvice tripAdvices, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    CloudBlockBlob blob = await UploadBlobAsync(imageFile);
                    tripAdvices.ImageURL = blob.Uri.ToString();
                }

                // save new advice to db
                db.TripAdviceTable.Add(tripAdvices);
                await db.SaveChangesAsync();

                // Add message to queue if there is an image
                if (tripAdvices.ImageURL != null)
                {
                    #region example for binary formatter
                    /*BinaryFormatter formatter = new BinaryFormatter();
                    MemoryStream stream = new MemoryStream();
                    formatter.Serialize(stream, product);
                    byte[] arr = stream.ToArray();*/

#endregion
                    CloudQueueMessage msg = new CloudQueueMessage(tripAdvices.TripAdviceID.ToString());
                    await imagesQueue.AddMessageAsync(msg);
                }
                return RedirectToAction("Index");
            }
            return View(tripAdvices);
        }

        // GET: Advisors/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TripAdvice tripAdvice = await db.TripAdviceTable.FindAsync(id);
            if (tripAdvice == null)
            {
                return HttpNotFound();
            }
            return View(tripAdvice);
        }

        // POST: Advisors/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "TripAdviceID,UserName,AdviceType,PlaceName,PrimesCount,GeoLocation,Country,City,AdviceText,RankPlace,ImageURL,ThumbnailURL,AdviceDate")] TripAdvice tripAdvices, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    CloudBlockBlob blob = await UploadBlobAsync(imageFile);
                    tripAdvices.ImageURL = blob.Uri.ToString();
                }

                // save changes to db
                db.Entry(tripAdvices).State = EntityState.Modified;
                await db.SaveChangesAsync(); 

                // Add message to queue if there is an image
                if (tripAdvices.ImageURL != null)
                {
                    #region example for binary formatter
                    /*BinaryFormatter formatter = new BinaryFormatter();
                    MemoryStream stream = new MemoryStream();
                    formatter.Serialize(stream, product);
                    byte[] arr = stream.ToArray();*/
                    #endregion
                    CloudQueueMessage msg = new CloudQueueMessage(tripAdvices.TripAdviceID.ToString());
                    await imagesQueue.AddMessageAsync(msg);
                }      
                return RedirectToAction("Index");
            }
            return View(tripAdvices);
        }

        // GET: Advisors/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TripAdvice tripAdvice = await db.TripAdviceTable.FindAsync(id);
            if (tripAdvice == null)
            {
                return HttpNotFound();
            }
            return View(tripAdvice);
        }

        // POST: Advisors/Delete/
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            TripAdvice tripAdvice = await db.TripAdviceTable.FindAsync(id);
            db.TripAdviceTable.Remove(tripAdvice);
            await db.SaveChangesAsync();
            if (tripAdvice.ImageURL != null)
            {
                await DeleteBlobAsync(tripAdvice.ImageURL);
            }
            if (tripAdvice.ThumbnailURL != null)
            {
                await DeleteBlobAsync(tripAdvice.ThumbnailURL);
            }
            return RedirectToAction("Index");
        }

        private async Task DeleteBlobAsync(string imageUrl)
        {
            Uri blobUri = new Uri(imageUrl);
            string blobName = blobUri.Segments[blobUri.Segments.Length - 1];

            CloudBlockBlob blob = imagesContainer.GetBlockBlobReference(blobName);
            await blob.DeleteAsync();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }   /* end class */
}   /* end namespace */
/* end of file */
