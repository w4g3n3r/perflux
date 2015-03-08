using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace perflux.Configuration
{
    public class PerformanceCounterElement : ConfigurationElement
    {
        [ConfigurationProperty("seriesNameFormat", IsKey = true, IsRequired = true)]
        public string SeriesNameFormat
        {
            get
            {
                return (string)this["seriesNameFormat"];
            }
        }

        [ConfigurationProperty("categoryNameExpression", IsKey = false, IsRequired = true)]
        public string CategoryNameExpression
        {
            get
            {
                return (string)this["categoryNameExpression"];
            }
        }

        [ConfigurationProperty("counterNameExpression", IsKey = false, IsRequired = true)]
        public string CounterNameExpression
        {
            get
            {
                return (string)this["counterNameExpression"];
            }
        }

        [ConfigurationProperty("instanceNameExpression", IsKey = false, IsRequired = true)]
        public string InstanceNameExpression
        {
            get
            {
                return (string)this["instanceNameExpression"];
            }
        }
    }
}
