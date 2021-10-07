# DDzia.Microsoft.Extensions.Configuration.AzureBlob

[![Build Status](https://dev.azure.com/ddziaddzia/GH/_apis/build/status/DDzia.Extensions.Configuration.AzureBlob?branchName=master)](https://dev.azure.com/ddziaddzia/GH/_build/latest?definitionId=132&branchName=master)
[![Nuget](https://img.shields.io/nuget/v/DDzia.Extensions.Configuration.AzureBlob?style=flat-square)](https://www.nuget.org/packages/DDzia.Extensions.Configuration.AzureBlob/)

Azure blob configuration provider implementation for Microsoft.Extensions.Configuration.


With this extension, multiple instances can share the application settings saved in Azure Blob, and below functionalities are supported:
* integrated with ASP .NET Core framework.
* auto reload for configuration updates. (**NOTE: require to work with [IOptionsSnapshot](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-3.1#options-interfaces)**)
* version control with the git repo. 
* high avalibility as the configuration is stored at Azure Blob.

## Usage

Install package:
```
dotnet add package DDzia.Microsoft.Extensions.Configuration.AzureBlob
```

Code sample:
```csharp
using Microsoft.Extensions.Configuration.AzureBlob;

Configuration = new ConfigurationBuilder()
                .AddBlobJson(
                        new BlobJsonConfigurationOption
                        {
                            BlobUri = new Uri("uri-to-blob-object"),
                            ClientProvider = new ConnectionStringClientProvider("storage-account-connection-string")
                        })
                .Build();
```