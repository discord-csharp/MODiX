using System.Collections.Generic;

namespace StatsdClient
{
    public static class DogStatsdExtensions
    {
        public static void Gauge<T>(this IDogStatsd stats, string statsName, T value, double sampleRate = 1, List<string> tags = null)
        {
            stats?.Gauge(statsName, value, sampleRate, tags?.ToArray());
        }

        public static void Increment(this IDogStatsd stats, string statsName, int value = 1, double sampleRate = 1, List<string> tags = null)
        {
            stats?.Increment(statsName, value, sampleRate, tags?.ToArray());
        }

        public static void Decrement(this IDogStatsd stats, string statsName, int value = 1, double sampleRate = 1, List<string> tags = null)
        {
            stats?.Decrement(statsName, value, sampleRate, tags?.ToArray());
        }
    }
}
