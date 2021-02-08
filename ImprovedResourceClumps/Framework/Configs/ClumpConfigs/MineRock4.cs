using System.Collections.Generic;

namespace ImprovedResourceClumps.Framework.Configs.ClumpConfigs
{
    internal class MineRock4
    {
        public bool EnableCustomMineRock4 { get; set; } = false;
        public Dictionary<int, int[]> ItemsAndCounts { get; set; } = new Dictionary<int, int[]>()
        {
            {304, new int[]{1, 10} }
        };
    }
}
