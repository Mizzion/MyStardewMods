using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtifactDigger
{
    public class ModConfig
    {
        //The dig radius.
        public int DigRadius { get; set; } = 1;

        public bool HighlightArtifactSpots { get; set; } = true;

        //Whether or not the mod should auto scan or by button press.
        public bool AutoArtifactScan { get; set; } = false;

        //Button to Scan for artifacts
        //public string ArtifactScanKey { get; set; } = "Z";
        public KeybindList ArtifactScanKey = new KeybindList(SButton.Z);

        //Whether or not the mod should shake trees.
        public bool ShakeTrees { get; set; } = false;

        //Whether or not the mod should shake bushes.
        public bool ShakeBushes { get; set; } = false;
    }
}
