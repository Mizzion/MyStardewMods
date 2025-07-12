using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FarmCleaner.Framework.Configs
{
    internal class GeneralConfig
    {
        public bool EnableMod { get; set; } = true;
        public Vector2 ItemChestLocation { get; set; } = new Vector2(0, 0); // Default location for the item chest

    }
}
