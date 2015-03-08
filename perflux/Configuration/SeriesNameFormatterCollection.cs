using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace perflux.Configuration
{
    public class SeriesNameFormatterCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new SeriesNameFormatterElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SeriesNameFormatterElement)element).findExpression;
        }
    }
}
