using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmCleaner.Framework.Configs
{
    internal class ResourceClumpConfig
    {
        public bool RemoveStumps { get; set; } = true; // Whether or not to remove stumps
        public bool RemoveLargeLogs { get; set; } = true; // Whether or not to remove large logs
        public bool RemoveLargeStones { get; set; } = true; // Whether or not to remove large stones
    }
}
