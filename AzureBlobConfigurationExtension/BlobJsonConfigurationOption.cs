using Microsoft.WindowsAzure.Storage.Auth;
using System;

namespace Microsoft.Extensions.Configuration.AzureBlob
{
    public class BlobJsonConfigurationOption
    {
        public string StorageAccountName { get; set; }
        public string BlobContainerName { get; set; }
        public string ConfigurationFile { get; set; }
        public StorageCredentials StorageCredentials { get; set; }
        public bool ReloadOnChange { get; set; } = false;
        public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(5);
    }
}
