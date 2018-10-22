using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Swashbuckle.Swagger.Annotations;

namespace BlobManagementApi.Controllers
{
    public class ValuesController : ApiController
    {
        [HttpGet]
        [System.Web.Http.Route("Api/GetBlobUri")]
        public string GetBlobUri(string filePath)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse("StorageConnectionString");

            //Create the blob client object.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            //Get a reference to a container to use for the sample code, and create it if it does not exist.
            CloudBlobContainer container = blobClient.GetContainerReference("harrytestcontainer");

            CloudBlockBlob blob = container.GetBlockBlobReference(filePath);

            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.SharedAccessStartTime = DateTimeOffset.UtcNow.AddMinutes(-5);
            sasConstraints.SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddHours(24);
            sasConstraints.Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.Write;

            //Generate the shared access signature on the blob, setting the constraints directly on the signature.
            string sasBlobToken = blob.GetSharedAccessSignature(sasConstraints);

            //Return the URI string for the container, including the SAS token.
            var uri = blob.Uri + sasBlobToken;

            return uri;
        }
    }
}
