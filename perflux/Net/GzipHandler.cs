using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace perflux.Net
{
    internal sealed class GzipHandler : DelegatingHandler
    {
        public GzipHandler(HttpMessageHandler innerHandler)
        {
            if ((InnerHandler = innerHandler) == null) throw new ArgumentNullException("innerHandler");
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, 
            System.Threading.CancellationToken cancellationToken)
        {
            var content = request.Content;

            if (request.Method == HttpMethod.Post)
            {
                request.Content = new GzipContent(request.Content);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
