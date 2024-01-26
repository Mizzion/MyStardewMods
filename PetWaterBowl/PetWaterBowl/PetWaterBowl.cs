using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using SObject = StardewValley.Object;

namespace PetWaterBowl
{
    public class PetWaterBowl : Mod
    {
        private ModConfig _config;

        //Config Settings
        private bool _enableMod;
        private bool _enableSnowWatering;
        private bool _enableSprinklers;

        public override void Entry(IModHelper helper)
        {
            _config = helper.ReadConfig<ModConfig>();
            _enableMod = _config.EnableMod;
            _enableSnowWatering = _config.EnableSnowWatering;
            _enableSprinklers = _config.EnableSprinklerWatering;
            
           
            //Events
             helper.Events.GameLoop.DayStarted += OnDayStarted;
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            //Vector2 preScan = CheckBowlLocation();
            var farm = Game1.getFarm();
            //farm.setMapTileIndex(Convert.ToInt32(preScan.X), Convert.ToInt32(preScan.Y), 1938, "Buildings");
            WaterPetBowl(new Vector2(farm.petBowlPosition.X, farm.petBowlPosition.Y));
        }
        
        /// <summary>
        /// Fills the pet bowl with water
        /// </summary>
        /// <param name="tileLocation">The location of the petbowl.</param>
        private void WaterPetBowl(Vector2 tileLocation)
        {
            if (!_enableMod)
                return;
            var farm = Game1.getFarm();
            if (Game1.isRaining || Game1.isLightning || (Game1.isSnowing && _enableSnowWatering) ||
                (CheckForSprinklers(tileLocation) && _enableSprinklers))
            {
                farm.petBowlWatered.Set(true);
                //farm.setMapTileIndex(Convert.ToInt32(tileLocation.X), Convert.ToInt32(tileLocation.Y), 1939, "Buildings");
                Monitor.Log("Water bowl should be filled.");
            }
            else
                farm.petBowlWatered.Set(false);//farm.setMapTileIndex(Convert.ToInt32(tileLocation.X), Convert.ToInt32(tileLocation.Y), 1938, "Buildings");


        }

        /// <summary>
        /// Scans looking to see if the player has Iridium Sprinklers around the bowl.
        /// </summary>
        /// <param name="tileLocation">Vector2 of scan spot</param>
        /// <returns>True/False depending on if it found the sprinkler</returns>
        private bool CheckForSprinklers(Vector2 tileLocation)
        {
            var sprinklerFound = false;
            var farm = Game1.getFarm();
            foreach (var farmObjects in farm.objects.Pairs)
            {
                if (_config.EnableSprinklerWatering && farmObjects.Value.ParentSheetIndex == 645)
                {
                    for (var x = (int)tileLocation.X - 2; x <= tileLocation.X + 2; x++)
                    {
                        for (var y = (int) tileLocation.Y - 2; y <= tileLocation.Y + 2; y++)
                        {
                            var newLoc = new Vector2(x, y);
                            if (farm.getTileIndexAt(Convert.ToInt32(newLoc.X), Convert.ToInt32(newLoc.Y),
                                    "Buildings") == 1938)
                                sprinklerFound = true;
                        }
                    }
                }
            }
            return sprinklerFound;
        }
        
        /// <summary>
        /// Scans the entire map looking for the waterbowl.
        /// </summary>
        /// <returns>Returns a Vector2 of where it found the waterbowl.</returns>
        private Vector2 CheckBowlLocation()
        {
            var farm = Game1.getFarm();
            for (var xTile = 0; xTile < farm.Map.Layers[0].LayerWidth; ++xTile)
            {
                for (var yTile = 0; yTile < farm.Map.Layers[0].LayerHeight; ++yTile)
                {
                    if (farm.getTileIndexAt(xTile, yTile, "Buildings") == 1938)
                    {
                        Monitor.Log($"Found WaterBowl: X:{xTile}, Y:{yTile}.", LogLevel.Trace);
                        return new Vector2(xTile, yTile);
                    }
                        
                }
            }
            Monitor.Log("Couldn't find waterbowl.", LogLevel.Trace);
            return  new Vector2(0, 0);
        }
    }
}