using perflux.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace perflux.Workers
{
    public class PostWorker : BaseWorker
    {
        public PostWorker(Monitor context, int intervalSeconds)
            : base(context, intervalSeconds) { }

        protected override void DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            log.Debug("PostWorker is working.");

            string json = string.Empty;

            lock (context.key)
            {
                log.Debug("PostWorker aquired counter lock.");

                var counters = context.Counters;

                json = Counter.ToJson(counters);
            }

            if (worker.CancellationPending)
            {
                e.Cancel = true;
                return;
            }

            log.Info("Counters reset, posting to influx db.");

            HttpClient http = new HttpClient(new GzipHandler(new HttpClientHandler()), true);

            var content = new StringContent(json);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            Task<HttpResponseMessage> result = http.PostAsync(context.ConnectionUri, content);

            result.Wait();

            if (result.Result.IsSuccessStatusCode)
            {
                log.Info("Successfully posted counter data to influx db.");
            }
            else
            {
                log.Error("Failed to post data to influx db. ({1}) {0}", 
                    result.Result.ReasonPhrase, 
                    result.Result.StatusCode);
            }
        }
    }
}
