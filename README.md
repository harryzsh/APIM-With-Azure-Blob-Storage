# APIMWithBlobStorage


I will use the following example to illustrate how to get file from Azure Blob Storage through APIM


![alt text](/Images/api-management-download-a-file.png)

1. Let's create a private container of a blob within a storage account

![alt text](/Images/PrivateBlobContainer.png "")


2. Create an API App (kind of app service) and a method to get URL+SAS token (URL + SAS Detail can be found in here https://docs.microsoft.com/en-us/azure/storage/blobs/storage-dotnet-shared-access-signature-part-2)

```sh

    [HttpGet]
    [System.Web.Http.Route("Api/GetBlobUri")]
    public string GetBlobUri(string filePath)
    {
        CloudStorageAccount storageAccount = CloudStorageAccount.Parse("StorageAccountConnectionString");

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
```

3. Import the API App into APIM

    ![alt text](/Images/apim.PNG)

4. Then add an outbound policy for the GetBlobUri method
```sh
    
<policies>
    <inbound>
        <base />
    </inbound>
    <backend>
        <base />
    </backend>
    <outbound>
        <set-variable name="fileSAS" value="@(context.Response.Body.As<string>())" />
        <send-request mode="new" response-variable-name="file" timeout="60" ignore-error="false">
            <set-url>@{
                var fileUri = (string)context.Variables["fileSAS"];
                fileUri= fileUri.Substring(1, fileUri.Length -2);
                return fileUri;
            }</set-url>
            <set-method>GET</set-method>
        </send-request>
        <set-body>@(((IResponse)context.Variables["file"]).Body.As<string>())</set-body>
        <set-header name="Content-Type" exists-action="override">
            <value>text/plain</value>
        </set-header>
        <base />
    </outbound>
    <on-error>
        <base />
    </on-error>
</policies>

```

5. In Test Panel, We can start to test this API

   ![alt text](/Images/apim_parameters.PNG)


6. We now should get the content out from the Text.txt file stored in the private blob storage container

   ![alt text](/Images/apim.PNG)
