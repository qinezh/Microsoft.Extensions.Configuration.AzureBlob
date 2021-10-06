using System;
using Microsoft.Extensions.Configuration.AzureBlob.Core;

namespace Microsoft.Extensions.Configuration.AzureBlob
{
    public static class BlobConfigurationExtensions
    {
        public static IConfigurationBuilder AddBlobJson(this IConfigurationBuilder builder, BlobJsonConfigurationOption option)
        {
            return builder.Add(new BlobJsonConfigurationSource(option ?? throw new ArgumentNullException(nameof(builder))));
        }
    }
}
