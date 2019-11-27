﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Better_Resource_Clumps
{
    internal class ResourcePrefix
    {
        public static IMonitor monitor;
        public static IModHelper helper;
        public ResourcePrefix(IMonitor _monitor, IModHelper _helper)
        {
            monitor = _monitor;
            helper = _helper;
        }
        public static bool performToolAction(ref ResourceClump __instance, ref Tool t, ref int damage,
            ref Vector2 tileLocation, ref GameLocation location)
        {
            if (__instance.parentSheetIndex.Value == 622)
            {
                monitor.Log("Found a Meteor.");
                return false;
            }
            return true;
        }
    }
}
