using System.Diagnostics;
using System.Text;

namespace LandSim.Extensions
{
    public static class StopWatchExtensions
    {
        public static long GetElapsedMillisecondsAndRestart(this Stopwatch stopWatch)
        {
            var elapsed = stopWatch.ElapsedMilliseconds;
            stopWatch.Restart();
            return elapsed;
        }
    }
}
