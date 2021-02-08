using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace ResetTerrainFeaturesRedux.Framework
{
    public static class DoLog
    {
        internal static IMonitor monitor;
        internal static void Log(string _log, LogLevel level = LogLevel.Trace)
        {
            monitor?.Log(_log, level);
        }
    }
}
