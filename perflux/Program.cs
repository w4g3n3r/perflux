using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;
using Topshelf.ServiceConfigurators;

namespace perflux
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
                {
                    x.Service<Monitor>();
                    x.RunAsNetworkService();
                    x.SetDescription("Performance counter logger for InfluxDB.");
                    x.SetDisplayName("PerFlux");
                    x.SetServiceName("PerFlux");
                    x.StartAutomaticallyDelayed();
                });
        }
    }
}
