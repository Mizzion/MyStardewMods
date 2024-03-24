using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using xTile.Dimensions;
using SObject = StardewValley.Object;

namespace PetWaterBowl
{
    public class PetWaterBowl : Mod
    {
        private ModConfig _config;

        //Config Settings
        private bool _debugging = false;
        
        private ITranslationHelper _i18N;
        private Mizzion.Stardew.Common.Integrations.GenericModConfigMenu.IGenericModConfigMenuApi _cfgMenu;
        

        public override void Entry(IModHelper helper)
        {
            _config = helper.ReadConfig<ModConfig>();
            _i18N = Helper.Translation;
            
            
            
           
            //Events
             helper.Events.GameLoop.DayStarted += OnDayStarted;
             helper.Events.GameLoop.GameLaunched += GameLaunched;
             helper.Events.Input.ButtonPressed += ButtonPressed;
        }

        private void GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            _cfgMenu = Helper.ModRegistry.GetApi<Mizzion.Stardew.Common.Integrations.GenericModConfigMenu.IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (_cfgMenu is null) return;

            //Register mod
            _cfgMenu.Register(
                mod: ModManifest,
                reset: () => _config = new ModConfig(),
                save: () => Helper.WriteConfig(_config)
            );

            _cfgMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => _i18N.Get("mod_setting_mod_name"),
                tooltip: null
            );

            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => _config.EnableMod,
                setValue: value => _config.EnableMod = value,
                name: () => _i18N.Get("mod_setting_enabled_text"),
                tooltip: () => _i18N.Get("mod_setting_enabled_description")
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => _config.EnableSnowWatering,
                setValue: value => _config.EnableSnowWatering = value,
                name: () => _i18N.Get("mod_setting_enable_snow_watering_text"),
                tooltip: () => _i18N.Get("mod_setting_enable_snow_watering_description")
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => _config.EnableSprinklerWatering,
                setValue: value => _config.EnableSprinklerWatering = value,
                name: () => _i18N.Get("mod_setting_enable_sprinkler_watering_text"),
                tooltip: () => _i18N.Get("mod_setting_enable_sprinkler_watering_description")
            );
        }


        
        
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
<<<<<<< Updated upstream
            //Vector2 preScan = CheckBowlLocation();
            Farm farm = Game1.getFarm();
            //farm.setMapTileIndex(Convert.ToInt32(preScan.X), Convert.ToInt32(preScan.Y), 1938, "Buildings");
            WaterPetBowl(new Vector2(farm.petBowlPosition.X, farm.petBowlPosition.Y));
=======
            if (!_config.EnableMod)
                return;
            
            if(_config.EnableSnowWatering || _config.EnableSprinklerWatering)
                WaterPetBowl();
        }

        private void ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if(!Context.IsWorldReady)
                return;

            if (e.IsDown(SButton.LeftShift))
            {
                _debugging = !_debugging;
                var isDebugging = _debugging ? "True" : "False";
                Monitor.Log($"Debugging set to: {isDebugging}");
            }
            if (e.IsDown(SButton.LeftControl) && _debugging)
            {
                foreach (var locations in Game1.locations)
                {
                    foreach (var bowl in locations.buildings.OfType<PetBowl>())
                    {
                                bowl.watered.Set(newValue: false);
                                Monitor.Log($"Took water out of Bowl at: X:{bowl.tileX.Value}, Y:{bowl.tileY.Value}");
                    }
                }
            }

            if (e.IsDown(SButton.RightShift) && _debugging)
            {
                WaterPetBowl();
            }
>>>>>>> Stashed changes
        }
        
        /// <summary>
        /// Fills the pet bowl with water
        /// </summary></param>
        private void WaterPetBowl()
        {
<<<<<<< Updated upstream
            if (!_enableMod)
                return;
            Farm farm = Game1.getFarm();
            if (Game1.isRaining || Game1.isLightning || (Game1.isSnowing && _enableSnowWatering) ||
                (CheckForSprinklers(tileLocation) && _enableSprinklers))
=======
            //Scan for water Bowl
            foreach (var locations in Game1.locations)
>>>>>>> Stashed changes
            {
                var sprinklers = CheckForSprinklers(locations);
                var bowls = CheckBowlLocation();
                
                if (sprinklers is null)
                    return;

                
                foreach (var bowl in locations.buildings.OfType<PetBowl>())
                {
                    foreach (var s in sprinklers)
                    {
                        if (s.Value.GetSprinklerTiles().Contains(new Vector2(bowl.tileX.Value, bowl.tileY.Value)) && !bowl.watered.Value)
                        {
                            if(_debugging)
                                Monitor.Log($"Watering Bowl at: X{bowl.tileX.Value}, Y: {bowl.tileY.Value}. With a radius of {s.Value.GetBaseRadiusForSprinkler()} from Object: {s.Value.DisplayName}");
                           
                            bowl.watered.Set(newValue: true);
                        }
                            
                    }
                }
            }
        }

        /// <summary>
        /// Scans looking to see if the player has Iridium Sprinklers around the bowl.
        /// </summary>
        
        private Dictionary<Vector2, SObject> CheckForSprinklers(GameLocation loc)
        {
<<<<<<< Updated upstream
            bool sprinklerFound = false;
            Farm farm = Game1.getFarm();
            foreach (KeyValuePair<Vector2, SObject> farmObjects in farm.objects.Pairs)
            {
                if (_config.EnableSprinklerWatering && farmObjects.Value.ParentSheetIndex == 645)
                {
                    for (int x = (int)tileLocation.X - 2; x <= tileLocation.X + 2; x++)
                    {
                        for (int y = (int) tileLocation.Y - 2; y <= tileLocation.Y + 2; y++)
                        {
                            Vector2 newLoc = new Vector2(x, y);
                            if (farm.getTileIndexAt(Convert.ToInt32(newLoc.X), Convert.ToInt32(newLoc.Y),
                                    "Buildings") == 1938)
                                sprinklerFound = true;
                        }
=======
            if (loc is null)
                return null;
            
            var sprinklers = new Dictionary<Vector2, SObject>();

            
                foreach (var i in loc.objects.Pairs)
                {
                    if (i.Value.IsSprinkler())
                    {
                        sprinklers.TryAdd(i.Key, i.Value);
>>>>>>> Stashed changes
                    }
                }
            
            return sprinklers;
        }
        
        /// <summary>
        /// Scans the entire map looking for the waterbowl.
        /// </summary>
        /// <returns>Returns a Vector2 of where it found the waterbowl.</returns>
        private List<Vector2> CheckBowlLocation()
        {
<<<<<<< Updated upstream
            Farm farm = Game1.getFarm();
            for (int xTile = 0; xTile < farm.Map.Layers[0].LayerWidth; ++xTile)
            {
                for (int yTile = 0; yTile < farm.Map.Layers[0].LayerHeight; ++yTile)
=======
            var bowlLocation = new List<Vector2>();
            foreach (var locations in Game1.locations)
            {
                foreach (var bowl in locations.buildings.OfType<PetBowl>())
>>>>>>> Stashed changes
                {
                    if (!bowlLocation.Contains(new Vector2(bowl.tileX.Value, bowl.tileY.Value)))
                        bowlLocation.Add(new Vector2(bowl.tileX.Value, bowl.tileY.Value));
                }
            }
            return bowlLocation;
        }
    }
}