using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using PhotoManagement.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoManagement.Controllers
{
    [Authorize]
    public class HomeController_old : Controller
    {
        //private readonly ILogger<HomeController> _logger;

      //  public HomeController(ILogger<HomeController> logger)
       // {
       //     _logger = logger;
       // }

       // public IActionResult Index()
       // {
          //  return View();
       // }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        static CloudBlobClient blobClient;
        const string blobContainerName = "imagecontainer";
        private readonly IConfiguration configuration;
        static CloudBlobContainer blobContainer;

        public HomeController_old(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<ActionResult> Index()
        {
            try
            {
                var storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=csb10037ffe95bacabd;AccountKey=SubShk36YBwsaexzFMvSNxqXfyhMHZ9bVtyv+2PnMQUVH+RDD1Q+dlCzEH+eINVlKQeucNPIkODBgXpg/Sj5LA==;EndpointSuffix=core.windows.net";
                var storageAccount = CloudStorageAccount.Parse(storageConnectionString);

                blobClient = storageAccount.CreateCloudBlobClient();
                blobContainer = blobClient.GetContainerReference(blobContainerName);
                await blobContainer.CreateIfNotExistsAsync();
                await blobContainer.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

                List<Uri> allBlobs = new List<Uri>();
                BlobContinuationToken blobContinuationToken = null;
                do {
                    var responce = await blobContainer.ListBlobsSegmentedAsync(blobContinuationToken);
                    foreach (IListBlobItem blob in responce.Results)
                    {
                        if (blob.GetType() == typeof(CloudBlockBlob))
                            allBlobs.Add(blob.Uri);
                    }

                    blobContinuationToken = responce.ContinuationToken;
                } while (blobContinuationToken != null);

                return View(allBlobs);
                //return View();
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<ActionResult> UploadAsync()
        {
            try
            {
                var request = await HttpContext.Request.ReadFormAsync();
                if(request.Files == null)
                {
                    return BadRequest("Could not upload files");
                }
                var files = request.Files;
                if(files.Count == 0)
                {
                    return BadRequest("No files selected");
                }

                for (int i = 0; i < files.Count; i++)
                {
                    var blob = blobContainer.GetBlockBlobReference(GetRandomBlobName(files[i].FileName));
                    //var blob = blobContainer.GetBlockBlobReference(files[i].FileName);
                    using(var stream = files[i].OpenReadStream())
                    {
                        await blob.UploadFromStreamAsync(stream);
                    }
                    //await blob.UploadFromStreamAsync(files[i].OpenReadStream());
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<ActionResult> DeleteImage(string name)
        {
            try
            {
                Uri uri = new Uri(name);
                string filename = Path.GetFileName(uri.LocalPath);

                var blob = blobContainer.GetBlockBlobReference(filename);
                await blob.DeleteIfExistsAsync();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<ActionResult> DeleteAll()
        {
            try
            {
                BlobContinuationToken blobContinuationToken = null;
                do
                {
                    var responce = await blobContainer.ListBlobsSegmentedAsync(blobContinuationToken);
                    foreach (IListBlobItem blob in responce.Results)
                    {
                        if (blob.GetType() == typeof(CloudBlockBlob))
                            await ((CloudBlockBlob)blob).DeleteIfExistsAsync();
                    }

                    blobContinuationToken = responce.ContinuationToken;
                } while (blobContinuationToken != null);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["message"] = ex.Message;
                ViewData["trace"] = ex.StackTrace;
                return View("Error");
            }
        }

        private string GetRandomBlobName(string filename)
        {
            string ext = Path.GetExtension(filename);
            return string.Format("{0:10}_{1}{2}", DateTime.Now.Ticks, Guid.NewGuid(), ext);
        }
    }
}
