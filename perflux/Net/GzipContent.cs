using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace perflux.Net
{
    internal sealed class GzipContent : HttpContent
    {
        private readonly HttpContent content;

        public GzipContent(HttpContent content)
        {
            this.content = content;
            foreach (KeyValuePair<string, IEnumerable<string>> header in content.Headers)
            {
                Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            Headers.ContentEncoding.Add("gzip");
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            using (GZipStream gzip = new GZipStream(stream, CompressionMode.Compress, true))
            {
                await content.CopyToAsync(gzip);
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}
