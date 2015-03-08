using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace perflux.Configuration
{
    public class SeriesNameFormatterElement : ConfigurationElement
    {
        [ConfigurationProperty("findExpression", IsKey = true, IsRequired = true)]
        public string findExpression
        {
            get { return (string)this["findExpression"]; }
        }

        [ConfigurationProperty("replaceWith", IsKey = false, IsRequired = false)]
        public string replaceWith
        {
            get { return (string)this["replaceWith"]; }
        }
    }
}
