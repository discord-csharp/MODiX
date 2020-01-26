using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Modix.Services.UserInfo
{
    public class LappingStopwatch
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public void Start()
        {
            _stopwatch.Start();
        }

        public void Lap(string lapName)
        {
            _stopwatch.Stop();

            Laps.Add(new LappingStopwatchResult(lapName, _stopwatch.Elapsed));

            _stopwatch.Reset();

            _stopwatch.Start();
        }

        public void Stop()
        {
            _stopwatch.Stop();
        }

        public List<LappingStopwatchResult> Laps { get; } = new List<LappingStopwatchResult>();

        public class LappingStopwatchResult
        {
            public LappingStopwatchResult(string lapName, TimeSpan elapsed)
            {
                LapName = lapName;
                Elapsed = elapsed;
            }

            public string LapName { get; }
            public TimeSpan Elapsed { get; }
        }
    }
}
