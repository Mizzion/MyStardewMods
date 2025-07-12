using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmCleaner.Framework.Configs
{
    internal class ForageConfig
    {
        public bool CollectForage { get; set; } = true; // Whether or not to collect forage items
        public bool ShakeBushes { get; set; } = true; // Whether or not to shake bushes for forage items
    }
}
