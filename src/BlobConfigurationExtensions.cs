using System;

namespace Microsoft.Extensions.Configuration.AzureBlob
{
    public static class BlobConfigurationExtensions
    {
        public static IConfigurationBuilder AddBlobJson(this IConfigurationBuilder builder, BlobJsonConfigurationOption option)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            return builder.Add(new BlobJsonConfigurationSource(option));
        }
    }
}
