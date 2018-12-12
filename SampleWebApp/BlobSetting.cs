using System.Collections.Generic;

namespace SampleWebApp
{
    public class BlobSetting
    {
        public string StorageAccountName { get; set; }

        public string BlobContainerName { get; set; }

        public string ConfigurationFile { get; set; }

        public Dictionary<string, string> Metadata { get; set; }
    }
}