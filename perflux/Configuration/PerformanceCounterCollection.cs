using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace perflux.Configuration
{
    public class PerformanceCounterCollection : ConfigurationElementCollection
    {
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new PerformanceCounterElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((PerformanceCounterElement)element).SeriesNameFormat;
        }

        public PerformanceCounterElement this[int index]
        {
            get
            {
                return (PerformanceCounterElement)BaseGet(index);
            }
        }

        new public PerformanceCounterElement this[string Name]
        {
            get
            {
                return (PerformanceCounterElement)BaseGet(Name);
            }
        }

        public int IndexOf(PerformanceCounterElement url)
        {
            return BaseIndexOf(url);
        }

        public void Add(PerformanceCounterElement url)
        {
            BaseAdd(url);
        }
    }
}
