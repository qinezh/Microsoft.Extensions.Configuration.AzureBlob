using Microsoft.WindowsAzure.Storage.Auth;
using System;
using System.Text.RegularExpressions;

namespace Microsoft.Extensions.Configuration.AzureBlob
{
    public class BlobJsonConfigurationOption
    {
        private static readonly Regex s_blobUrlRegex = new Regex(@"(?<=http|https):\/\/(?<account>[a-z0-9]+)\.blob\.core\.windows\.net\/(?<container>[a-z0-9-]+)\/(?<file>.+)", RegexOptions.Compiled);

        public string BlobUrl { get; set; }
        public string AccessKey { get; set; }
        public bool ReloadOnChange { get; set; } = false;
        public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(5);

        internal static (string, string, string) Parse(string blobUrl)
        {
            var match = s_blobUrlRegex.Match(blobUrl);
            if (match.Success)
            {
                return (match.Groups["account"].Value, match.Groups["container"].Value, match.Groups["file"].Value);
            }

            throw new ArgumentException($"{nameof(blobUrl)} with value {blobUrl} can't be parsed.");
        }
    }
}
