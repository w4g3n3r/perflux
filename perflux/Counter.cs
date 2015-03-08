using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace perflux
{
    public class Counter
    {
        private const long WIN_EPOCH = 11644473600000;
        private ConcurrentQueue<CounterSample> samples;

        public PerformanceCounter PerformanceCounter { get; set; }
        public CounterSample LastSample { get; set; }
        public bool HasSamples { get { return samples.Count > 1; } }

        public Counter(PerformanceCounter performanceCounter) : this(performanceCounter, CounterSample.Empty) { }
        public Counter(PerformanceCounter performanceCounter, CounterSample lastSample)
        {
            PerformanceCounter = performanceCounter;
            samples = new ConcurrentQueue<CounterSample>();

            samples.Enqueue(lastSample);
        }

        public void Sample()
        {
            samples.Enqueue(PerformanceCounter.NextSample());
        }

        public Dictionary<long, float> Calculate()
        {
            var results = new Dictionary<long, float>();
            while(samples.Count > 1)
            {
                CounterSample previousSample, currentSample;
                if (!samples.TryDequeue(out previousSample)) break;
                if (!samples.TryPeek(out currentSample)) break;

                results.Add(
                    TimestampToEpoch(currentSample.TimeStamp100nSec),
                    CounterSample.Calculate(previousSample, currentSample));
            }
            return results;
        }

        private long TimestampToEpoch(long timestamp)
        {
            var epoch = ((timestamp / 10000) - WIN_EPOCH);
            return epoch;
        }

        public static string ToJson(Dictionary<string, Counter> counters)
        {            
            StringBuilder json = new StringBuilder();
            json.Append('[');

            int cc = 0;
            foreach (var series in counters.Keys)
            {
                var counter = counters[series];
                var samples = counter.Calculate();
                if (samples.Any())
                {
                    var seriesDelimiter = (cc++ == 0) ? "" : ",";
                    json.AppendFormat(@"{0}{{""name"": ""{1}"", ""columns"":[""time"", ""value""], ""points"":[", seriesDelimiter, series);
                    int sc = 0;
                    foreach (var epoch in samples.Keys)
                    {
                        var sample = samples[epoch];
                        var sampleDelimiter = (sc++ == 0) ? "" : ",";
                        json.AppendFormat(@"{2}[{0},{1}]", epoch, sample, sampleDelimiter);
                    }
                    json.Append("]}");
                }
            }
            json.Append(']');

            return json.ToString();
        }
    }
}
