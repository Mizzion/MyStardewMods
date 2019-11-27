﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using SFarmer = StardewValley.Farmer;
using SObject = StardewValley.Object;

namespace UltimateTool.Framework.Tools
{
    internal class MilkPailTool : BaseTool
    {
        public MilkPailTool()
        {

        }

        public override bool IsEnabled(SFarmer who, Tool tool, Item item, GameLocation location)
        {
            return tool is MilkPail;
        }

        public override bool Apply(Vector2 tile, SObject tileObj, TerrainFeature tileFeature, SFarmer who, Tool tool, Item item, GameLocation location)
        {
            return UseToolOnTile(tool, tile);
        }
    }
}
