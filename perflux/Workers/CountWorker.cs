using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace perflux.Workers
{
    public class CountWorker : BaseWorker
    {
        private readonly int rateLimit;

        public CountWorker(Monitor context, int intervalSeconds, int rateLimit)
            : base(context, intervalSeconds) 
        {
            this.rateLimit = rateLimit;
        }

        public override void Stop()
        {
            base.Stop();

            PerformanceCounter.CloseSharedResources();
        }

        protected override void DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            log.Debug("CountWorker is working.");

            var counters = context.Counters;

            lock (context.key)
            {                
                log.Debug("CountWorker aquired counter lock.");

                int counterCount = 0;
                Stopwatch timer = new Stopwatch();
                timer.Start();

                foreach (var c in counters.Keys)
                {
                    counterCount++;
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        break;
                    }

                    var counter = counters[c];
                    var performanceCounter = counter.PerformanceCounter;

                    if (string.IsNullOrEmpty(performanceCounter.InstanceName) || 
                        PerformanceCounterCategory.InstanceExists(performanceCounter.InstanceName, performanceCounter.CategoryName))
                    {
                        counter.Sample();
                        // Rate limit to prevent spiking the CPU.
                        Thread.Sleep(this.rateLimit);
                    }
                }

                timer.Stop();
                log.Debug("{0} counters in {1} seconds.", counterCount, timer.Elapsed.TotalSeconds);
                log.Debug("{0} counters per second.", counterCount / timer.Elapsed.TotalSeconds);
            }

            log.Debug("CountWorker finished.");
        }
    }
}
