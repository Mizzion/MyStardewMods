﻿using Microsoft.Xna.Framework;

namespace FarmCleaner.Framework.Configs
{
    internal class FcConfig
    {
       // public List<string> LocationsToClear { get; set; } = new List<string>(){"Farm", "Mountain"};
        public bool GrassRemoval { get; set; } = true;
        public bool WeedRemoval { get; set; } = true;
        public bool StoneRemoval { get; set; } = true;

        public bool BreakOres { get; set; } = true;
        public bool TwigRemoval { get; set; } = true;
        public bool ForageRemoval { get; set; } = false;
        public bool StumpRemoval { get; set; } = false;
        public bool LargeLogRemoval { get; set; } = false;

        public bool LargeStoneRemoval { get; set; } = false;

        public Vector2 ChestLocation { get; set; } = new Vector2(58, 16);

        public TreeConfig TreeConfigs { get; set; } = new TreeConfig();

        /*
        public bool SaplingRemoval { get; set; }
        public int MaxTreeStage { get; set; }*/
    }
}
