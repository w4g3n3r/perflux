using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace perflux.Configuration
{
    public class PerfluxConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("performanceCounters", IsDefaultCollection=false)]
        [ConfigurationCollection(typeof(PerformanceCounterElement))]
        public PerformanceCounterCollection PerformanceCounters
        {
            get { return (PerformanceCounterCollection)base["performanceCounters"]; }
        }

        [ConfigurationProperty("seriesNameFormatters", IsDefaultCollection=false)]
        [ConfigurationCollection(typeof(SeriesNameFormatterElement))]
        public SeriesNameFormatterCollection SeriesNameFormatters
        {
            get { return (SeriesNameFormatterCollection)base["seriesNameFormatters"]; }
        }

        [ConfigurationProperty("connection", IsRequired=true)]
        public InfluxConnectionElement Connection
        {
            get { return (InfluxConnectionElement)base["connection"]; }
        }

        [ConfigurationProperty("monitorIntervalSeconds", IsRequired = false, DefaultValue = 1)]
        public int MonitorIntervalSeconds { get { return (int)this["monitorIntervalSeconds"]; } }

        [ConfigurationProperty("postIntervalSeconds", IsRequired = false, DefaultValue = 60)]
        public int PostIntervalSeconds { get { return (int)this["postIntervalSeconds"]; } }

        [ConfigurationProperty("counterCleanupIntervalSeconds", IsRequired = false, DefaultValue = 300)]
        public int CounterCleanupIntervalSeconds { get { return (int)this["counterCleanupIntervalSeconds"]; } }

        [ConfigurationProperty("rateLimit", IsRequired=false, DefaultValue = 10)]
        public int RateLimit { get { return (int)this["rateLimit"]; } }

        public Dictionary<string, PerformanceCounter> GetCounters()
        {
            var counters = new Dictionary<string, PerformanceCounter>();

            var categories = PerformanceCounterCategory.GetCategories();

            foreach (var configElement in PerformanceCounters)
            {
                var counterConfig = configElement as PerformanceCounterElement;
                if (counterConfig == null) continue;

                var categoryExpression = new Regex(counterConfig.CategoryNameExpression);
                var matchedCategories = categories.Where(c => categoryExpression.IsMatch(c.CategoryName));

                foreach (var category in matchedCategories)
                {
                    var instanceExpression = new Regex(counterConfig.InstanceNameExpression);
                    var allInstances = category.GetInstanceNames();

                    var matchedInstances = allInstances.Any() ?
                        allInstances.Where(i => instanceExpression.IsMatch(i)) :
                        new string[] { string.Empty };

                    foreach (var instanceName in matchedInstances)
                    {
                        var counterExpression = new Regex(counterConfig.CounterNameExpression);
                        var allCounters = string.IsNullOrEmpty(instanceName) ?
                            category.GetCounters() :
                            category.GetCounters(instanceName);

                        var matchedCounters = allCounters
                            .Where(c => counterExpression.IsMatch(c.CounterName));

                        foreach (var counter in matchedCounters)
                        {
                            var counterKey = string.Format(counterConfig.SeriesNameFormat,
                                FormatSeriesName(counter.CategoryName),
                                FormatSeriesName(counter.CounterName),
                                FormatSeriesName(counter.InstanceName));

                            if (counters.ContainsKey(counterKey)) continue;

                            counters.Add(
                                string.Format(counterConfig.SeriesNameFormat,
                                    FormatSeriesName(counter.CategoryName),
                                    FormatSeriesName(counter.CounterName),
                                    FormatSeriesName(counter.InstanceName)),
                                counter);
                        }
                    }
                }
            }

            return counters;
        }

        private string FormatSeriesName(string value)
        {
            foreach (var configElement in SeriesNameFormatters)
            {
                var formatterConfig = configElement as SeriesNameFormatterElement;
                if (formatterConfig == null) continue;

                var findExpression = new Regex(formatterConfig.findExpression);
                var replaceString = formatterConfig.replaceWith;

                value = findExpression.Replace(value, replaceString);
            }

            return value;
        }
    }
}
