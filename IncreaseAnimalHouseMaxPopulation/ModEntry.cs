using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        private PlayerData _data;

        public SButton RefreshConfig;

        public ITranslationHelper I18N;

        public Building CurrentHoveredBuilding;

        public Building CurrentHoveredBuildingDummy;

        public List<string> AnimalHouseBuildings = new List<string>
        {
            "Deluxe Barn",
            "Deluxe Coop"
        };

        public int Cost;

        public bool IsTestBuild = true;

        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();
            I18N = helper.Translation;
            helper.Events.GameLoop.GameLaunched += GameLaunched;
            helper.Events.GameLoop.DayStarted += DayStarted;
            helper.Events.GameLoop.DayEnding += DayEnding;
            helper.Events.GameLoop.Saving += Saving;
            helper.Events.GameLoop.UpdateTicked += UpdateTicked;
            helper.Events.Input.ButtonPressed += ButtonPressed;
            helper.Events.Display.RenderingHud += RenderingHud;
            helper.ConsoleCommands.Add("pop_reset", "Deletes the save data for Increased Animal House Population.", ResetSave);
            DoSanityCheck();
        }

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
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => Config.MainSettings.EnableBuildingMapReplacements,
                setValue: value => Config.MainSettings.EnableBuildingMapReplacements = value,
                name: () => I18N.Get("setting_mapedit_text"),
                tooltip: () => I18N.Get("setting_mapedit_description")
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

        private void ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
            {
                CurrentHoveredBuilding = GetHoveredBuilding(Config.MainSettings.EnableDebugMode);
            }
        }


        private void AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!Config.MainSettings.EnableBuildingMapReplacements)
                return;
            
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
        }

        if (e.IsDown(RefreshConfig))
        {
            Config = Helper.ReadConfig<ModConfig>();
            DoSanityCheck();
            Monitor.Log("Reloaded the configuration file.", LogLevel.Debug);
        }

    if (e.IsDown(SButton.F6) && IsTestBuild)
            {
                DoPopChange(Config.MaxBarnPopulation, Config.MaxCoopPopulation);
            }

            if (e.IsDown(SButton.MouseLeft) && CurrentHoveredBuilding != null &&
                AnimalHouseBuildings.Contains(CurrentHoveredBuilding.buildingType.Value) &&
                !_data.Buildings.ContainsKey(CurrentHoveredBuilding.indoors.Value.uniqueName.Value) && _data.Buildings != null &&
                Game1.activeClickableMenu == null && Game1.player.CurrentItem == null)
            {
                Vector2 tLocation = GetCursorLocation();
                if (!AnimalHouseBuildings.Any(ab =>
                        CurrentHoveredBuilding.buildingType.Contains(ab) &&
                        CurrentHoveredBuilding.indoors.Value != null) || CurrentHoveredBuilding == null)
                {
                    return;
                }

                int freeOrNot = ((!Config.Cheats.EnableFree) ? Config.CostPerPopulationIncrease : 0);
                //Lets calculate the difference between max and current max population
                int currentMaxOccupants = ((AnimalHouse)CurrentHoveredBuilding.indoors.Value).animalLimit.Value; //(AnimalHouse)this.CurrentHoveredBuilding.indoors
                Cost = (CurrentHoveredBuilding.buildingType.Value.Contains("Deluxe Barn")
                    ? ((Config.MaxBarnPopulation - currentMaxOccupants) * freeOrNot)
                    : ((Config.MaxCoopPopulation - currentMaxOccupants) * freeOrNot));

                CurrentHoveredBuildingDummy = CurrentHoveredBuilding;
                string question = I18N.Get("upgrade_question", new
                {
                    current_building = CurrentHoveredBuilding.buildingType.Value,
                    next_cost = Cost,
                    current_max_occupants = currentMaxOccupants,
                    config_max_occupants = CurrentHoveredBuilding.buildingType.Value.Contains("Deluxe Barn") ? Config.MaxBarnPopulation : Config.MaxCoopPopulation
                });
                Game1.getFarm().createQuestionDialogue(question, Game1.getFarm().createYesNoResponses(),
                    delegate (Farmer _, string answer)
                    {
                        if (answer == "Yes")
                        {
                            if (Game1.player.Money >= Cost)
                            {
                                Game1.player.Money -= Cost;
                                DoPopChange(CurrentHoveredBuildingDummy);
                            }
                            else
                            {
                                Game1.showRedMessage($"You don't have {Cost} gold.");
                            }
                        }
                    });
            }

            
        }

        private void Saving(object sender, SavingEventArgs e)
        {
            Helper.Data.WriteSaveData(Helper.ModRegistry.ModID, _data);
        }

        private void RenderingHud(object sender, RenderingHudEventArgs e)
        {
            if (CurrentHoveredBuilding != null && Game1.activeClickableMenu == null &&
                (Config.EnableDebugMode || Config.EnableHoverTip) && AnimalHouseBuildings.Any(
                    ab => CurrentHoveredBuilding.buildingType.Contains(ab) &&
                            CurrentHoveredBuilding.indoors.Value != null))
            {
                Translation tipText = I18N.Get("upgrade_tooltip_text", new
                {
                    current_building = CurrentHoveredBuilding.buildingType.Value,
                    max_animals = ((AnimalHouse) CurrentHoveredBuilding.indoors.Value).animalLimit
                });
                IClickableMenu.drawHoverText(Game1.spriteBatch, tipText, Game1.smallFont);
            }

            if (CurrentHoveredBuilding != null)
            {
                int p = (CurrentHoveredBuilding.buildingType.Value.Contains("Deluxe Barn")
                    ? Config.MaxBarnPopulation
                    : Config.MaxCoopPopulation);
                AnimalHouse obj = CurrentHoveredBuilding.indoors.Value as AnimalHouse;
                if ((obj == null || obj.animalLimit.Value != p) &&
                    CurrentHoveredBuilding.buildingType.Value.Contains("Deluxe") &&
                    !CurrentHoveredBuilding.buildingType.Value.Contains("Cabin") &&
                    !CurrentHoveredBuilding.buildingType.Value.Contains("Silo") &&
                    !CurrentHoveredBuilding.buildingType.Value.Contains("Mill") &&
                    Game1.activeClickableMenu == null)
                {
                    Game1.mouseCursor = 4;
                }
            }
        }

        private void UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (Context.IsWorldReady && e.IsMultipleOf(4u))
            {
                CurrentHoveredBuilding =
                    (Game1.currentLocation as BuildableGameLocation)?.getBuildingAt(Game1.currentCursorTile);
            }
        }

        private void OneSecondUpdateTicking(object sender, OneSecondUpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady)
            {
                return;
            }

            if (_data != null && _data.Buildings.Count > 0)
            {
                foreach (KeyValuePair<string, bool> b in _data.Buildings)
                {
                    IEnumerable<Building> bb = Game1.getFarm().buildings
                        .Where(bbb => bbb.indoors.Value.uniqueName.Value.Equals(b.Key));
                    foreach (Building build in bb)
                    {
                        int pop = (build.buildingType.Value.Contains("Deluxe Barn")
                            ? Config.MaxBarnPopulation
                            : Config.MaxCoopPopulation);
                        if (((AnimalHouse) build.indoors.Value).animalLimit.Value != pop)
                        {
                            DoPopChange(build, showLog: true, doRestore: true);
                        }
                    }
                }
            }

            if (Config.Cheats.EnableFree)
            {
                DoPopChange(Config.MaxBarnPopulation, Config.MaxCoopPopulation);
            }
        }

        private void DayEnding(object sender, DayEndingEventArgs e)
        {
            if (Config.AutoFeedExtraAnimals)
            {
                DoFeeding(Game1.getFarm());
            }
        }

        private void DayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Context.IsWorldReady)
            {
                return;
            }

            _data = Helper.Data.ReadSaveData<PlayerData>(Helper.ModRegistry.ModID) ?? new PlayerData();
            if (!_data.Buildings.Any())
            {
                return;
            }

            IEnumerator<Building> enumerator = Game1.getFarm().buildings.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    Building b = enumerator.Current;
                    if (b != null)
                    {
                        AnimalHouse bb = b.indoors.Value as AnimalHouse;
                        if (bb == null)
                        {
                            continue;
                        }

                        foreach (KeyValuePair<string, bool> building in _data.Buildings)
                        {
                            if (building.Key.Equals(bb.uniqueName.Value))
                            {
                                DoPopChange(bb.getBuilding(), showLog: true, doRestore: true);
                            }
                        }
                    }
                }
            }
            finally
            {
                enumerator.Dispose();
            }
        }

        private bool CheckClick()
        {
            if (CurrentHoveredBuilding == null)
            {
                return false;
            }

            Vector2 curTile = GetCursorLocation();
            if (AnimalHouseBuildings.Any(ahb => CurrentHoveredBuilding.buildingType.Contains(ahb)) &&
                CurrentHoveredBuilding.indoors.Value != null &&
                Utility.tileWithinRadiusOfPlayer((int) curTile.X, (int) curTile.Y, 4, Game1.player))
            {
                return true;
            }

            return false;
        }

        private void DoPopChange(int maxBarnPop, int maxCoopPop, bool showLog = true)
        {
            foreach (Building b2 in Game1.getFarm().buildings.Where(b =>
                b.buildingType.Value.Contains("Deluxe Barn") || b.buildingType.Value.Contains("Deluxe Coop")))
            {
                int pop = (b2.buildingType.Value.Contains("Deluxe Barn") ? maxBarnPop : maxCoopPop);
                if (Config.AutoFeedExtraAnimals)
                {
                    DoFeeding((AnimalHouse) b2.indoors.Value);
                }

                if (((AnimalHouse) b2.indoors.Value).animalLimit.Value != pop)
                {
                    ((AnimalHouse) b2.indoors.Value).animalLimit.Value = pop;
                    b2.maxOccupants.Value = pop;
                    _data.Buildings.Add(b2.indoors.Value.uniqueName.Value, value: true);
                    if (Config.EnableDebugMode)
                    {
                        Game1.showGlobalMessage($"Set {b2.buildingType.Value} to {b2.maxOccupants.Value}");
                    }

                    Monitor.Log($"Set {b2.buildingType.Value} to {b2.maxOccupants.Value}",
                        showLog ? LogLevel.Debug : LogLevel.Trace);
                }
            }
        }

        private void DoPopChange(Building b, bool showLog = true, bool doRestore = false)
        {
            int pop = (b.buildingType.Value.Contains("Deluxe Barn")
                ? Config.MaxBarnPopulation
                : Config.MaxCoopPopulation);
            if (Config.AutoFeedExtraAnimals)
            {
                DoFeeding((AnimalHouse) b.indoors.Value);
            }

            if (((AnimalHouse) b.indoors.Value).animalLimit.Value != pop)
            {
                ((AnimalHouse) b.indoors.Value).animalLimit.Value = pop;
                b.maxOccupants.Value = pop;
                if (!doRestore)
                {
                    _data.Buildings.Add(b.indoors.Value.uniqueName.Value, value: true);
                }

                if (Config.EnableDebugMode)
                {
                    Game1.showGlobalMessage($"Set {b.buildingType.Value} to {b.maxOccupants.Value}");
                }

                Monitor.Log($"Set {b.buildingType.Value} to {b.maxOccupants.Value}",
                    showLog ? LogLevel.Debug : LogLevel.Trace);
            }
        }

        private void DoFeeding(AnimalHouse ah)
        {
            if (ah == null)
            {
                Monitor.Log("There was an error while trying to load the animal house. Code:1");
                return;
            }

            foreach (KeyValuePair<long, FarmAnimal> a in ah.animals.Pairs)
            {
                if (a.Value.fullness.Value != byte.MaxValue && Game1.getFarm().piecesOfHay.Value >= 1)
                {
                    a.Value.fullness.Value = byte.MaxValue;
                    a.Value.daysSinceLastFed.Value = 1;
                    Game1.getFarm().piecesOfHay.Value--;
                    if (Config.EnableDebugMode)
                    {
                        Monitor.Log($"Fed: {a.Value.Name}, new Fullness: {a.Value.fullness.Value}");
                    }
                }
            }
        }

        private void DoFeeding(Farm loc)
        {
            if (loc == null)
            {
                return;
            }

            foreach (Building b2 in loc.buildings.Where(b =>
                b.buildingType.Value.Contains("Deluxe Barn") || b.buildingType.Value.Contains("Deluxe Coop")))
            {
                AnimalHouse ah = b2.indoors.Value as AnimalHouse;
                if (ah == null)
                {
                    break;
                }

                foreach (KeyValuePair<long, FarmAnimal> a in ah.animals.Pairs)
                {
                    if (a.Value.fullness.Value != byte.MaxValue && Game1.getFarm().piecesOfHay.Value >= 1)
                    {
                        //a.Value.
                        a.Value.fullness.Value = byte.MaxValue;
                        a.Value.daysSinceLastFed.Value = 1;
                        Game1.getFarm().piecesOfHay.Value--;
                        if (Config.EnableDebugMode)
                        {
                            Monitor.Log($"Fed: {a.Value.Name}, new Fullness: {a.Value.fullness.Value}");
                        }
                    }
                }
            }
        }

        private bool doHayRemoval(AnimalHouse ah)
        {

            return false;
        }
        private Vector2 GetCursorLocation()
        {
            return new Vector2((Game1.getOldMouseX() + Game1.viewport.X) / 64,
                (Game1.getOldMouseY() + Game1.viewport.Y) / 64);
        }

        private void DoRobinMenu()
        {
        }

        private void DoSanityCheck()
        {
            if (Config.MaxBarnPopulation <= 0)
            {
                Config.MaxBarnPopulation = 1;
                Helper.WriteConfig(Config);
                Monitor.Log("The configured MaxBarnPopulation wasn't higher than Zero, it has been set to 1",
                    LogLevel.Debug);
            }

            if (Config.MaxCoopPopulation <= 0)
            {
                Config.MaxCoopPopulation = 1;
                Helper.WriteConfig(Config);
                Monitor.Log("The configured MaxCoopPopulation wasn't higher than Zero, it has been set to 1",
                    LogLevel.Debug);
            }

            if (!Enum.TryParse(Config.RefreshConfigButton.ToString(), ignoreCase: true,
                out RefreshConfig))
            {
                RefreshConfig = SButton.F5;
                Monitor.Log("There was an error parsing the RefreshConfigButton. It was reset to F5");
            }
        }

        private void ResetSave(string command, string[] args)
        {
            if (_data != null)
            {
                _data?.Buildings.Clear();
                Monitor.Log("Save data was reset.", LogLevel.Debug);
            }
        }
    }
}