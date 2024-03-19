using System;
using System.Collections.Generic;
using System.Linq;
using Common.Integrations;
using IncreaseAnimalHouseMaxPopulation.Framework;
using IncreaseAnimalHouseMaxPopulation.Framework.Configs;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;
using xTile;

namespace IncreaseAnimalHouseMaxPopulation
{
    public class ModEntry : Mod
    {
        public ModConfig Config;

        private Mizzion.Stardew.Common.Integrations.GenericModConfigMenu.IGenericModConfigMenuApi _cfgMenu;

        private PlayerData _data;

        public SButton RefreshConfig;

        public ITranslationHelper I18N;

        public Building CurrentHoveredBuilding;

        public Building CurrentHoveredBuildingDummy;

        public List<string> AnimalHouseBuildings = new()
        {
            "Deluxe Barn",
            "Deluxe Coop"
        };


        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();
            I18N = helper.Translation;


            helper.Events.GameLoop.GameLaunched += GameLaunched;
            helper.Events.GameLoop.DayStarted += DayStarted;
            helper.Events.GameLoop.DayEnding += DayEnding;
           // helper.Events.GameLoop.SaveLoaded += SaveLoaded;
            helper.Events.GameLoop.Saving += Saving;
            helper.Events.GameLoop.UpdateTicked += UpdateTicked;
            helper.Events.Input.ButtonPressed += ButtonPressed;
            helper.Events.Display.RenderingHud += RenderingHud;
            helper.Events.Content.AssetRequested += AssetRequested;

            //Commands
            helper.ConsoleCommands.Add("pop_reset", "Deletes the save data for Increased Animal House Population.",
                ResetSave);

            
            DoSanityCheck();
        }
        //GameLoop Events
        
        private void GameLaunched(object sender, GameLaunchedEventArgs e)
        {

            _cfgMenu = Helper.ModRegistry.GetApi<Mizzion.Stardew.Common.Integrations.GenericModConfigMenu.IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (_cfgMenu is null) return;

            //Register mod
            _cfgMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            _cfgMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => I18N.Get("mod_name_text"),
                tooltip: null
            );

            //Main Settings

            _cfgMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => I18N.Get("mod_main_settings_text"),
                tooltip: null
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => Config.MainSettings.EnableDebugMode,
                setValue: value => Config.MainSettings.EnableDebugMode = value,
                name: () => I18N.Get("setting_debug_text"),
                tooltip: () => I18N.Get("setting_debug_description")
                );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => Config.MainSettings.EnableHoverTip,
                setValue: value => Config.MainSettings.EnableHoverTip = value,
                name: () => I18N.Get("setting_hover_text"),
                tooltip: () => I18N.Get("setting_hover_description")
            );
            _cfgMenu.AddKeybind(
                mod: ModManifest,
                getValue: () => Config.MainSettings.RefreshConfigButton,
                setValue: value => Config.MainSettings.RefreshConfigButton = value,
                name: () => I18N.Get("setting_reload_config_button_text"),
                tooltip: () => I18N.Get("setting_reload_config_button_description")
            );

            //Building Settings
            _cfgMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => I18N.Get("mod_building_settings_text"),
                tooltip: null
            );
            _cfgMenu.AddNumberOption(
                mod: ModManifest,
                getValue: () => Config.BuildingSettings.MaxBarnPopulation,
                setValue: value => Config.BuildingSettings.MaxBarnPopulation = value,
                name: () => I18N.Get("setting_max_barn_population_text"),
                tooltip: () => I18N.Get("setting_max_barn_population_description")
                );
            _cfgMenu.AddNumberOption(
                mod: ModManifest,
                getValue: () => Config.BuildingSettings.MaxCoopPopulation,
                setValue: value => Config.BuildingSettings.MaxCoopPopulation = value,
                name: () => I18N.Get("setting_max_coop_population_text"),
                tooltip: () => I18N.Get("setting_max_coop_population_description")
                );
            _cfgMenu.AddNumberOption(
                mod: ModManifest,
                getValue: () => Config.BuildingSettings.CostPerPopulationIncrease,
                setValue: value => Config.BuildingSettings.CostPerPopulationIncrease = value,
                name: () => I18N.Get("setting_cost_per_population_increase_text"),
                tooltip: () => I18N.Get("setting_cost_per_population_increase_description")
                );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => Config.BuildingSettings.AutoFeedExtraAnimals,
                setValue: value => Config.BuildingSettings.AutoFeedExtraAnimals = value,
                name: () => I18N.Get("setting_auto_feed_extra_animals_text"),
                tooltip: () => I18N.Get("setting_auto_feed_extra_animals_description")
                );

            //Cheat Settings
            _cfgMenu.AddSectionTitle(
                mod: ModManifest,
                text: () => I18N.Get("mod_cheats_text"),
                tooltip: null
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => Config.Cheats.EnableFree,
                setValue: value => Config.Cheats.EnableFree = value,
                name: () => I18N.Get("setting_enable_free_food_text"),
                tooltip: () => I18N.Get("setting_enable_free_food_description")
                );
        }

        private void UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (Context.IsWorldReady && e.IsMultipleOf(4u))
            {

                CurrentHoveredBuilding = GetHoveredBuilding(Config.MainSettings.EnableDebugMode);
            }
        }


        private void AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Maps/Coop3"))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsMap();
                   Map map = Helper.ModContent.Load<Map>("assets/Coop3.tmx");
                   editor.ExtendMap(minHeight: 10, minWidth: 46);
                   editor.PatchMap(map, patchMode: PatchMapMode.Replace);
                });
                
            }

            if (e.NameWithoutLocale.IsEquivalentTo("Maps/Barn3"))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsMap();
                    Map map = Helper.ModContent.Load<Map>("assets/Barn3.tmx");
                    editor.ExtendMap(minHeight: 14, minWidth: 50);
                    editor.PatchMap(map, patchMode: PatchMapMode.Replace);
                });

            }
        }


        private void SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            _data = Helper.Data.ReadSaveData<PlayerData>(Helper.ModRegistry.ModID) ?? new PlayerData();

            var dataFound = _data is null ? "_data couldn't be found" : "_data was either found or created";

            if(Config.MainSettings.EnableDebugMode)
                Log($"{dataFound}");
        }
        private void DayStarted(object sender, DayStartedEventArgs e)
        {
            if(!Context.IsWorldReady)
                return;

            _data = Helper.Data.ReadSaveData<PlayerData>(Helper.ModRegistry.ModID) ?? new PlayerData();
            
            var dataFound = _data is null ? "_data couldn't be found" : "_data was either found or created";

            if (Config.MainSettings.EnableDebugMode)
                Log($"{dataFound}");

            if (_data != null)
                ModifyBuildings(Config.MainSettings.EnableDebugMode);
        }

        private void DayEnding(object sender, DayEndingEventArgs e)
        {
            
            if (_data != null)
            {
                ModifyBuildings(Config.MainSettings.EnableDebugMode, true);
            }
        }

        private void Saving(object sender, SavingEventArgs e)
        {
            Helper.Data.WriteSaveData(Helper.ModRegistry.ModID, _data);
        }
        //Input Events
        
        private void ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (e.IsDown(Config.MainSettings.RefreshConfigButton))
            {
                Config = Helper.ReadConfig<ModConfig>();
                DoSanityCheck();
                Log("Reloaded the configuration file.");
            }

            if (e.IsDown(SButton.MouseLeft) && Game1.player.CurrentItem == null && CurrentHoveredBuilding != null &&
                AnimalHouseBuildings.Contains(CurrentHoveredBuilding.buildingType.Value) && _data.Buildings != null &&
                !_data.Buildings.ContainsKey(CurrentHoveredBuilding.indoors.Value.uniqueName.Value) &&
                Game1.activeClickableMenu == null)
            {
                CurrentHoveredBuildingDummy = CurrentHoveredBuilding;

                var free = Config.Cheats.EnableFree ? 0 : Config.BuildingSettings.CostPerPopulationIncrease;
                var maxOccupants = ((AnimalHouse)CurrentHoveredBuilding.indoors.Value).animalLimit.Value;

                var cost = CurrentHoveredBuilding.buildingType.Value.Contains("Deluxe Barn")
                    ? (Config.BuildingSettings.MaxBarnPopulation - maxOccupants) * free
                    : (Config.BuildingSettings.MaxCoopPopulation - maxOccupants) * free;

                string question = I18N.Get("upgrade_question", new
                {
                    current_building = CurrentHoveredBuilding.buildingType.Value,
                    next_cost = cost,
                    current_max_occupants = maxOccupants,
                    config_max_occupants = CurrentHoveredBuilding.buildingType.Value.Contains("Deluxe Barn") ? Config.BuildingSettings.MaxBarnPopulation : Config.BuildingSettings.MaxCoopPopulation
                });

                Game1.currentLocation.createQuestionDialogue(question, Game1.currentLocation.createYesNoResponses(),
                    delegate(Farmer _, string answer)
                    {
                        if (answer == "Yes")
                        {
                            if (Game1.player.Money >= cost)
                            {
                                Game1.player.Money -= cost;
                                DoPopulationChange(CurrentHoveredBuildingDummy, Config.MainSettings.EnableDebugMode);
                            }
                            else
                            {
                                Game1.showRedMessage($"You don't have {cost} gold.");
                            }
                        }
                    });
            }

            if (e.IsDown(SButton.NumPad4))
            {
                foreach (GameLocation location in Game1.locations)
                {
                    Log($"{location.DisplayName}({location.Name})");
                }
            }

            if (e.IsDown(SButton.NumPad5))
            {
                var loc = Game1.currentLocation;

                if(loc is null)
                {
                    Monitor.Log("Loc was null");
                    return;
                }

                foreach (var building in loc.buildings)
                {
                    if (building.indoors.Value is not AnimalHouse animalHouse)
                    {
                        Monitor.Log("building.indoors wasnt an animal house");
                        continue;
                    }
                    
                    
                    var troughs = GetNumberTroughs(animalHouse);
                    Log($"{building.indoors.Value.uniqueName} has {troughs} troughs.");
                }


            }


        }

        //Display Events
        
        private void RenderingHud(object sender, RenderingHudEventArgs e)
        {
            if (CurrentHoveredBuilding is null || !ProperAnimalBuilding(CurrentHoveredBuilding))
                return;

            
            if (!Config.MainSettings.EnableHoverTip && !Config.MainSettings.EnableDebugMode)
                return;

            string tipText = "";

            //Show Hover Message
            if (Config.MainSettings.EnableHoverTip)
            {
                tipText = I18N.Get("upgrade_tooltip_text", new
                {
                    current_building = CurrentHoveredBuilding.buildingType.Value,
                    max_animals = ((AnimalHouse)CurrentHoveredBuilding.indoors.Value).animalLimit
                });
            }

            if (Config.MainSettings.EnableDebugMode)
            {
                tipText = I18N.Get("upgrade_tooltip_text_debug", new
                {
                    current_building = CurrentHoveredBuilding.buildingType.Value,
                    max_animals = ((AnimalHouse)CurrentHoveredBuilding.indoors.Value).animalLimit,
                    building_name = CurrentHoveredBuilding.buildingType.Value,
                    unique_building_name = CurrentHoveredBuilding.indoors.Value.uniqueName.Value
                });
            }
            if(!string.IsNullOrEmpty(tipText))
                IClickableMenu.drawHoverText(Game1.spriteBatch, tipText, Game1.smallFont);

            //Change Cursor
            var p = (CurrentHoveredBuilding.buildingType.Value.Contains("Deluxe Barn")
                ? Config.BuildingSettings.MaxBarnPopulation
                : Config.BuildingSettings.MaxCoopPopulation);

            var obj = (AnimalHouse)CurrentHoveredBuilding.indoors.Value;
            if ((obj == null || obj.animalLimit.Value != p) &&
                CurrentHoveredBuilding.buildingType.Value.Contains("Deluxe") &&
                Game1.activeClickableMenu == null)
            {
                Game1.mouseCursor = 4;
            }

        }



        //Custom Methods

        /// <summary>
        /// Tries to check if the hovered building is an animal building.
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        private bool ProperAnimalBuilding(Building b)
        {
            return b != null && Game1.activeClickableMenu == null &&
                   AnimalHouseBuildings.Any(ab =>
                       CurrentHoveredBuilding.buildingType.Contains(ab) &&
                       CurrentHoveredBuilding.indoors.Value != null);
        }

        /// <summary>
        /// Grabs the current building in front of the player. If debugging, it will grab where the mouse is.
        /// </summary>
        /// <param name="debugging"></param>
        /// <returns></returns>
        private Building GetHoveredBuilding(bool debugging = false)
        {
            var currentBuilding = debugging ? Helper.Input.GetCursorPosition().Tile : (Game1.player.Tile + new Vector2(0f, -1f));
            return Game1.currentLocation?.getBuildingAt(currentBuilding);
        }

        /// <summary>
        /// Method that checks to make sure the settings are valid. If not, it will reset  them.
        /// </summary>
        private void DoSanityCheck()
        {
            if (Config.BuildingSettings.MaxBarnPopulation <= 0)
            {
                Config.BuildingSettings.MaxBarnPopulation = 1;
                Log("The configured MaxBarnPopulation wasn't a proper number. It's been reset to 1.");
            }

            if (Config.BuildingSettings.MaxCoopPopulation <= 0)
            {
                Config.BuildingSettings.MaxCoopPopulation = 1;
                Log("The configured MaxCoopPopulation wasn't a proper number. It's been reset to 1.");
            }

            if (!Enum.TryParse(Config.MainSettings.RefreshConfigButton.ToString(), ignoreCase: true, out RefreshConfig))
            {
                RefreshConfig = SButton.F5;
                Log("There was an error parsing he RefreshConfigButton. It was reset to F5");
            }

            Helper.WriteConfig(Config);
        }

        /// <summary>
        /// Method that resets the save data
        /// </summary>
        /// <param name="command"></param>
        /// <param name="args"></param>
        private void ResetSave(string command, string[] args)
        {
            if (_data != null)
            {
                _data?.Buildings.Clear();
                Log("Save data was reset.");
                
            }
        }

        /// <summary>
        /// A method to easily write to the smapi monitor.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="useTrace"></param>
        private void Log(string message, bool useTrace = false)
        {
            if(!useTrace)
                Monitor.Log(message, LogLevel.Debug);
            else
                Monitor.Log(message);
        }
        

        private void ModifyBuildings(bool showLog, bool doRestore = false)
        {
            var locations = DataLoader.Locations(Game1.content);
            var loadedFarm = false;
            
            if (_data == null || !_data.Buildings.Any() || locations == null)
                return;
            

            foreach (var location in locations)
            {
                GameLocation loc;

                if (location.Key.Contains("Farm_") && !loadedFarm)
                {
                    loc = Game1.getLocationFromName("Farm");
                    loadedFarm = true;
                }
                else
                {
                    loc = Game1.getLocationFromName(location.Key);
                }

                if (loc is null) continue;

                if (!loc.IsBuildableLocation()) continue;


                if (Config.MainSettings.EnableDebugMode)
                    Log($"{loc.DisplayName} was buildable location.");

                foreach (var building in loc.buildings)
                {
                    if (building?.indoors.Value is null)
                        continue;

                    if (Config.MainSettings.EnableDebugMode)
                        Log($"Scanning for {building.indoors.Value.uniqueName.Value}");

                    if (building.indoors.Value is not AnimalHouse animalHouse)
                        continue;

                    
                    if (_data.Buildings.ContainsKey(building.indoors.Value.uniqueName.Value))
                    {
                        if (Config.MainSettings.EnableDebugMode)
                            Log($"_data found for {building.indoors.Value.uniqueName.Value}");

                        if (doRestore)
                        {
                            if (Config.MainSettings.EnableDebugMode)
                                Log($"Running restore");
                            //Run animal feeder
                            if(Config.BuildingSettings.AutoFeedExtraAnimals)
                                FeedExtraAnimals(animalHouse);

                            ResetPopulationChange(building, Config.MainSettings.EnableDebugMode);
                        }
                        else
                        {
                            if (Config.MainSettings.EnableDebugMode)
                                Log($"Running Change Population");
                            DoPopulationChange(building, Config.MainSettings.EnableDebugMode);
                        }

                    }
                }

            }

        }


        private void DoPopulationChange(Building build, bool showLog = false)
        {
            var pop = (build.buildingType.Value.Contains("Deluxe Barn")
                ? Config.BuildingSettings.MaxBarnPopulation
                : Config.BuildingSettings.MaxCoopPopulation);

            var curPop = ((AnimalHouse)build.indoors.Value).animalLimit.Value;

            if (build is null)
            {
                Log($"Build was null.");
                return;
            }

            if (((AnimalHouse)build.indoors.Value).animalLimit.Value != pop && !_data.Buildings.ContainsKey(build.indoors.Value.uniqueName.Value))
            {
                ((AnimalHouse)build.indoors.Value).animalLimit.Value = pop;
                build.maxOccupants.Value = pop;
                _data?.Buildings.TryAdd(build.indoors.Value.uniqueName.Value, true);                
            }
            else if (((AnimalHouse)build.indoors.Value).animalLimit.Value != pop)
            {
                ((AnimalHouse)build.indoors.Value).animalLimit.Value = pop;
                build.maxOccupants.Value = pop;
            }

            //Show Global message if debug is on
            if (Config.MainSettings.EnableDebugMode)
                Game1.showGlobalMessage($"Set {build.buildingType.Value}'s max occupants to {build.maxOccupants.Value}.");

            Log($"Set {build.buildingType.Value}'s max occupants to {build.maxOccupants.Value}.", showLog);
        }

        private void ResetPopulationChange(Building build, bool showLog = false)
        {
            var pop = (build.buildingType.Value.Contains("Deluxe Barn")
                ? Config.BuildingSettings.MaxBarnPopulation
                : Config.BuildingSettings.MaxCoopPopulation);

            if (((AnimalHouse)build.indoors.Value).animalLimit.Value == pop)
            {
                ((AnimalHouse)build.indoors.Value).animalLimit.Value = 12;
                
                build.maxOccupants.Value = 12;
            }

            //Show Global message if debug is on
            if (Config.MainSettings.EnableDebugMode)
                Game1.showGlobalMessage($"Reset {build.buildingType.Value}'s max occupants to {build.maxOccupants.Value}.");

            Log($"Reset {build.buildingType.Value}'s max occupants to {build.maxOccupants.Value}.", showLog);
        }


        private static int GetNumberTroughs(GameLocation location)
        {
            var num = 0;
            
            for (var x = 0; x < location.map.Layers[0].LayerWidth; x++)
            {
                for (var y = 0; y < location.map.Layers[0].LayerHeight; y++)
                {
                    if (location.doesTileHaveProperty(x, y, "Trough", "Back") == null)
                        continue;
                    
                    num++;
                }
            }

            return num;
        }

        private void FeedExtraAnimals(AnimalHouse ah)
        {
            if (ah is null) return;


            var rootLocation = ah.GetRootLocation();
            var numTroughs = GetNumberTroughs(ah);
            var numAnimals = ah.animalsThatLiveHere.Count;
            var numExtraAnimals = numAnimals - numTroughs > 0 ? numAnimals - numTroughs : 0;

            Log($"ExtraAnimals: {numExtraAnimals}");
            var fedAnimal = 0;

            if (numExtraAnimals == 0) return;

            if (!ah.Animals.Any()) return;

            foreach (var animal in ah.Animals.Pairs.ToArray())
            {

                var numHay = rootLocation.piecesOfHay.Value;
                if (fedAnimal < numExtraAnimals && numHay >= 1)
                {
                    animal.Value.fullness.Value = 255;
                    rootLocation.piecesOfHay.Value -= 1;
                    fedAnimal++;
                }
                
            }
        }

        public static IEnumerable<GameLocation> GetLocations(bool includeTempLevels = false)
        {
            var locations = Game1.locations
                .Concat(
                    from location in Game1.locations
                    from indoors in location.GetInstancedBuildingInteriors()
                    select indoors
                );

            if (includeTempLevels)
                locations = locations.Concat(MineShaft.activeMines).Concat(VolcanoDungeon.activeLevels);

            return locations;
        }
    }
}