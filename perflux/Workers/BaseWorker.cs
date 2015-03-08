using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace perflux.Workers
{
    public abstract class BaseWorker : IDisposable
    {
        protected NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

        private Timer timer;
        protected BackgroundWorker worker;

        protected Monitor context;

        public BaseWorker(Monitor context, int intervalSeconds)
        {
            this.context = context;

            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.DoWork += DoWork;

            timer = new Timer();
            timer.Interval = TimeSpan.FromSeconds(intervalSeconds).TotalMilliseconds;
            timer.Elapsed += timer_Elapsed;
        }

        public void Start()
        {
            this.timer.Start();
        }

        public virtual void Stop()
        {
            timer.Stop();
            if (worker.IsBusy && !worker.CancellationPending) worker.CancelAsync();
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!worker.IsBusy)
                worker.RunWorkerAsync();
        }

        protected abstract void DoWork(object sender, DoWorkEventArgs e);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) 
            {
                if (worker != null) worker.Dispose();
                worker = null;

                if (timer != null) timer.Dispose();
                timer = null;
            }
        }
    }
}
