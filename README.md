# Microsoft.Extensions.Configuration.AzureBlob

Azure blob configuration provider implementation for Microsoft.Extensions.Configuration.

## Usage

```csharp
Configuration = new ConfigurationBuilder()
                .AddBlobJson(new BlobJsonConfigurationOption
                {
                    BlobUri = "https://qinezh.blob.core.windows.net/config/appsettings.json",
                    IsPublic = false,
                    ReloadOnChange = true,
                    LogReloadException = e => logger.LogError(e, e.Message),
                    ActionOnReload = () => logger.LogInformation("Reloaded.")
                })
                .Build();
```

## How to run the demo web app?

As this demo web app is using MSI to access azure storage, you need below steps before running it:

1. Add your user to the Data Reader / Data Contributor role on the appropriate resource
    > NOTE:
    > To access blob by MSI, it's not enough for the app and account to be added as owners, you need go to your storage account > IAM > Add role and add the special permission for this type of request, STORAGE BLOB DATA CONTRIBUTOR (PREVIEW)
    >
2. Login to the [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest) as the user, and make sure to select the right subscription. Or if you're using Visual Studio 2017, you could go to Tools -> Options -> Azure Service Authentication and authenticate there.
