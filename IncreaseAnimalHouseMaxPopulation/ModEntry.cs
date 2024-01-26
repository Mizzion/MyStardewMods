using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using GenericModConfigMenu;
using IncreaseAnimalHouseMaxPopulation.Framework;
using IncreaseAnimalHouseMaxPopulation.Framework.Configs;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Menus;

namespace IncreaseAnimalHouseMaxPopulation
{
    public class ModEntry : Mod
    {
        public ModConfig Config;

        private IGenericModConfigMenuApi _cfgMenu;

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

        public int Cost;

        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();
            I18N = helper.Translation;


            helper.Events.GameLoop.GameLaunched += GameLaunched;
            helper.Events.GameLoop.DayStarted += DayStarted;
            helper.Events.GameLoop.DayEnding += DayEnding;
            //ToDo helper.Events.GameLoop.Saving += Saving;
            helper.Events.GameLoop.UpdateTicked += UpdateTicked;
            helper.Events.Input.ButtonPressed += ButtonPressed;
            helper.Events.Display.RenderingHud += RenderingHud;
            helper.ConsoleCommands.Add("pop_reset", "Deletes the save data for Increased Animal House Population.",
                ResetSave);


            DoSanityCheck();
        }
        //GameLoop Events
        
        private void GameLaunched(object sender, GameLaunchedEventArgs e)
        {

            _cfgMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
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

        private void DayStarted(object sender, DayStartedEventArgs e)
        {
            if(!Context.IsWorldReady)
                return;

            //_data = Helper.Data.ReadSaveData<PlayerData>(Helper.ModRegistry.ModID) ?? new PlayerData();
        }

        private void DayEnding(object sender, DayEndingEventArgs e)
        {
            //todo
            
        }

        //Input Events
        
        private void ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            //ToDo
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
                /*
                !CurrentHoveredBuilding.buildingType.Value.Contains("Cabin") &&
                !CurrentHoveredBuilding.buildingType.Value.Contains("Silo") &&
                !CurrentHoveredBuilding.buildingType.Value.Contains("Mill") &&
                */
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
            if(useTrace)
                Monitor.Log(message, LogLevel.Debug);
            else
                Monitor.Log(message);
        }

        private void DoPopulationChange(Building build, bool DoRestore = false)
        {

        }
    }
}