# Getting a user's Gravatar data using Azure Functions

Years back I created a Gravatar library for C# to use in some projects where I needed some basic user profile information. You can read more about that in [an article](https://blog.jepsen.ninja/gravatar-c-api) I wrote about that subject.

The [library](https://www.nuget.org/packages/GravatarSharp.Core/) has around 6K downloads on Nuget and recently I decided to update it to 2021 standards mainly because people are actually using it and also because I wanted to use it in an Azure Function.

## Why Azure Functions

The overall idea with this is that whenever a user signs up in our system, we want to enrich that user profile with as much data as possible, with as little interaction needed from the user as possible.
This is where Gravatar comes into play, because you can query their service, just providing them with an email address, and they will return whatever user profile info they have. Since we want as much profile information as possible, we also want to query other services for information but that has nothing to do with the data we get from Gravatar. Therefore, by splitting the logic into several small micro services that query each different external service, we will make a more robust, more scalable and hopefully also easier to maintain solution. Azure Functions is the perfect match in this kind of scenario, as we can have several Functions triggered when a user is created in our system.

So here's the high-level idea:

1. User sign up for an account in our system
2. Our system creates the account in our database, etc
3. Our system kicks off several processes to grab more user data, one of them is the Gravatar Function

Since grabbing data from different external services could take some time, perhaps not minutes, but definitely several seconds, we don't want the user to wait for that, so we run those in the background using Azure Functions.

## Implementation

### Considerations

We are going to create a new Azure Function with a `QueueTrigger`, this will ensure that our front-end system can quickly add a queue message and don't have to let the user wait for process to run. We could also have chosen a `HttpTrigger`, but then our front-end would have to wait for the function to return, which is what we want to avoid. 

### Hands-on coding

So to get started, fire up Visual Studio, create a new Azure Function project and when you get to the New Azure Function dialog, select the `Queue trigger`:

![image.png](https://cdn.hashnode.com/res/hashnode/image/upload/v1618989539026/mlHiUrsGr.png)

Once this is done, we need to setup a storage connection so that our Function can read queue messages and persist the Gravatar images to a blob.

>Azure Functions uses a storage account to handle it's internal workings, like the triggers, state, etc. Microsoft recommends to use a separate storage account for your own code, for instance if you need to write to a blob, then use a separate storage account to store those blobs. Read more about that  [here](https://docs.microsoft.com/en-us/azure/azure-functions/storage-considerations#storage-account-guidance)

Here are the steps:

1. If you don't have a storage account in Azure, then create one
2. Go to the storage account in the Azure Portal
3. Create a new Queue, name it gravatar-queue
4. Select Access Keys and copy the connection string from **key1**:

![image.png](https://cdn.hashnode.com/res/hashnode/image/upload/v1618990061112/VMUp7DFzt.png)

**Remember, for production scenarios the storage account used internally by the function should be separate from this storage account.**
5. In VS, in your functions project, open `local.settings.json`
6. Add a new property name "StorageConnectionString" and insert the copied connection string:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "StorageConnectionString": "DefaultEndpointsProtocol=https;AccountName=...",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet"
  }
}

```

You then need to reference that connection string in your function trigger, like this:

```csharp
[FunctionName("Function1")]
public async Task Run([QueueTrigger("gravatar-queue", Connection = "StorageConnectionString")]string email, ILogger log)
{
    ...

}
```

#### Talking to Gravatar

All we need to do now is to grab the Nuget, call Gravatar through that Nuget and persist the data into our storage account:

```ps
Install-Package GravatarSharp.Core -Version 0.9.0.2
```

Setup up an `output binding` on the function to allow the function code to send the Gravatar image to our blob storage, this is done by adding an attribute to our function signature:

```cs
[Blob("gravatar-images", Connection = "StorageConnectionString")] CloudBlobContainer outputContainer
```

`gravatar-images` is a blob container that you need to create in the storage account that the `StorageConnectionString` points to.

Putting the pieces together here is how our entire function class should look like:

```cs
public static class Function1
{
    [FunctionName("Function1")]
    public static async Task Run(
        [QueueTrigger("gravatar-queue", Connection = "StorageConnectionString")]string email,
        [Blob("gravatar-images", Connection = "StorageConnectionString")] CloudBlobContainer outputContainer,
        ILogger log)
    {
        // The initialization of the GravatarController is only for demo purposes, in real life you would inject the controller and the HttpClient using DI
        var gravatar = new GravatarController(new HttpClient());
        var result = await gravatar.GetProfile(email);
     
        // You probably don't want to do this in a production scenario as it makes a request against the storage account
        await outputContainer.CreateIfNotExistsAsync();
        var cloudBlockBlob = outputContainer.GetBlockBlobReference(result.Profile.Id);
        await cloudBlockBlob.UploadFromStreamAsync(await new HttpClient().GetStreamAsync(result.Profile.ImageThumbUrl));
    }
}
```

> Please take note of the comments in the code, especially the first on about using dependency injection, the above code is just a simple demo, in real life you should not create new objects like this. I have written an article about [dependency injection](https://blog.jepsen.ninja/dependency-injection-in-azure-functions) and another one specifically about [using HttpClient correctly in Azure Functions](https://blog.jepsen.ninja/using-ihttpclientfactory-in-azure-functions).

## Demo time

With the above in place, let's take a look at how it all works out.
First, run your function project. Then, add an item to the queue (I use Azure Storage Explorer, more on that in the bottom of this article):

![image.png](https://cdn.hashnode.com/res/hashnode/image/upload/v1619158918637/WBySgGJZ_.png)

Then hit <kbd>OK</kbd> and your function should be executed shortly after. Once executed, you should find a file in the blob container, like here:

![image.png](https://cdn.hashnode.com/res/hashnode/image/upload/v1619159093262/a9fOQEPqh.png)

## Wrapping Up

That should cover it all, here is what we have accomplished:

- Creating an Azure Function that triggers on a queue message
- Using a library to get user profile data from Gravatar
- Use a storage output binding to persist the data

**Thanks for reading** and please feel free to reach out to me in the comments if you have any questions, feedback or anything else. I will be happy to hear about how you use Azure Functions or if you are planning to use Gravatar or the library mentioned.

## Related Resources

Here are some relevant links about the stuff we have discussed in this article.

[Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/) - a great tool for everything related to Azure Storage

[GravatarSharp Nuget package](https://www.nuget.org/packages/GravatarSharp.Core/)

%[https://blog.jepsen.ninja/dependency-injection-in-azure-functions]

%[https://blog.jepsen.ninja/using-ihttpclientfactory-in-azure-functions]
