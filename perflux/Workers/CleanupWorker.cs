using perflux.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace perflux.Workers
{
    public class CleanupWorker : BaseWorker
    {
        public CleanupWorker(Monitor context, int intervalSeconds)
            : base(context, intervalSeconds) { }

        protected override void DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            log.Debug("CleanupWorker is working.");

            var config = ConfigurationManager.GetSection("perflux") as PerfluxConfigurationSection;

            if (config == null) return;

            var configCounters = config.GetCounters();

            lock(context.key)
            {
                log.Debug("CleanupWorker aquired counter lock.");
                foreach (var key in configCounters.Keys)
                {
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    if (!context.Counters.ContainsKey(key))
                    {
                        log.Info("Adding new counter {0}.", key);
                        context.Counters.Add(key, new Counter(configCounters[key]));
                    }
                }

                var removeKeys = new List<string>();
                foreach (var key in context.Counters.Keys)
                {
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    if (!configCounters.ContainsKey(key) && !context.Counters[key].HasSamples)
                    {
                        log.Info("Removing counter {0}.", key);
                        removeKeys.Add(key);
                    }
                }

                removeKeys.ForEach(k =>
                {
                    context.Counters[k].PerformanceCounter.Close();
                    context.Counters.Remove(k);
                });
            }

            log.Debug("CleanupWorker finished.");
        }
    }
}
