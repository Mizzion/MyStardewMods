using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmCleaner.Framework.Configs
{
    internal class ResourceConfig
    {
        public bool GrassRemoval { get; set; } = true;
        public bool WeedRemoval { get; set; } = true;
        public bool StoneRemoval { get; set; } = true;
        public bool BreakOres { get; set; } = true;
        public bool TwigRemoval { get; set; } = true;
    }
}
