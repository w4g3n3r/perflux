using perflux.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Topshelf;

using perflux.Configuration;
using System.Configuration;
using perflux.Workers;

namespace perflux
{
    public class Monitor : ServiceControl
    {
        private NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        private CountWorker countWorker;
        private PostWorker postWorker;
        private CleanupWorker cleanupWorker;

        internal object key = new Object();

        public Dictionary<string, Counter> Counters { get; set; }
        public Uri ConnectionUri { get; set; }

        public Monitor()
        {
            Counters = new Dictionary<string, Counter>();
        }

        public bool Start(HostControl hostControl)
        {
            PerfluxConfigurationSection config = 
                ConfigurationManager.GetSection("perflux") as PerfluxConfigurationSection;

            if (config == null)
            {
                log.Error("Could not load configuration.");
                throw new ConfigurationErrorsException("Configuration section \"perflux\" is missing.");
            }

            ConnectionUri = config.Connection.GetConnectionUri();
            log.Info("InfluxDb host:{0} Port:{1} Database:{2}",
                config.Connection.HostName,
                config.Connection.Port,
                config.Connection.DatabaseName);

            var configCounters = config.GetCounters();
            foreach (var series in configCounters.Keys)
            {
                log.Info("Adding counter {0}.", series);
                Counters.Add(series, new Counter(configCounters[series]));
            }

            countWorker = new CountWorker(this, config.MonitorIntervalSeconds, config.RateLimit);
            postWorker = new PostWorker(this, config.PostIntervalSeconds);
            cleanupWorker = new CleanupWorker(this, config.CounterCleanupIntervalSeconds);

            countWorker.Start();
            postWorker.Start();
            cleanupWorker.Start();

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            log.Info("Perflux service stopping.");

            countWorker.Stop();
            postWorker.Stop();
            cleanupWorker.Stop();            

            return true;
        }
    }
}
