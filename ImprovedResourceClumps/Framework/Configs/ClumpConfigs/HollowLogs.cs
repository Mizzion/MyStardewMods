using System.Collections.Generic;

namespace ImprovedResourceClumps.Framework.Configs.ClumpConfigs
{
    internal class HollowLogs
    {
        public bool EnableCustomHollowLogs { get; set; } = true;
        public Dictionary<int, int[]> ItemsAndCounts { get; set; } = new Dictionary<int, int[]>()
        {
            {304, new int[]{1, 10} }
        };
    }
}
