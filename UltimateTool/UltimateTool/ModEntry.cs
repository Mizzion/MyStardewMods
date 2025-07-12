using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using UltimateTool.Framework;
using UltimateTool.Framework.Tools;
using UltimateTool.Framework.Configuration;
using MyStardewMods.Common;
using SFarmer = StardewValley.Farmer;
using SObject = StardewValley.Object;


namespace UltimateTool
{
    internal class ModEntry : Mod
    {
        private ModConfig _config;
        
        private ITool[] _tools;

        private SButton _actionKey;
       private bool _showGrid = false;

        //Player Data
        private PlayerData PlayerData;

        public override void Entry(IModHelper helper)
        {
            _config = helper.ReadConfig<ModConfig>();
            DoTools();
            UBush.helper = helper;

            //Events
            var events = helper.Events;
            events.GameLoop.SaveLoaded += OnSaveLoaded;
            events.GameLoop.Saving += OnSaving;
            events.Input.ButtonPressed += OnButtonPressed;
            events.Display.Rendered += OnRendered;
            //events.Input.MouseWheelScrolled += OnMouseScrolled;
            //events.GameLoop.UpdateTicked += OnUpdateTicked;
        }

    }
}
