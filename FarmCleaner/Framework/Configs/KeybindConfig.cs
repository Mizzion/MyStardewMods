using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmCleaner.Framework.Configs
{
    internal class KeybindConfig
    {
        public SButton GetLocationData { get; set; } = SButton.NumPad8;
        public SButton ClearLocation { get; set; } = SButton.NumPad9;
        public SButton SetChestLocationAndSpawnIt { get; set; } = SButton.End;
    }
}
