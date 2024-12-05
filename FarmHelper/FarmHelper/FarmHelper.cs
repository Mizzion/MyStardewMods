#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using Mizzion.Stardew.Common;
using FarmHelper.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mizzion.Stardew.Common.Integrations.GenericModConfigMenu;
using MyStardewMods.Common;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.GameData.Crops;
using StardewValley.GameData.Locations;
using StardewValley.Inventories;
using StardewValley.Locations;
using StardewValley.Minigames;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using SFarmer = StardewValley.Farmer;
using SObject = StardewValley.Object;

namespace FarmHelper
{
    public class FarmHelper : Mod
    {
        private ModConfig Config;

        private IGenericModConfigMenuApi? _cfgMenu = null;

        private Lazy<Pickaxe> fakePickAxe;
        private Axe fakeAxe;
        private Hoe fakeHoe;
        private WateringCan fakeWatering;
        private MeleeWeapon fakeWeapon;
        private readonly TimeSpan TillDirtDelay = TimeSpan.FromSeconds(1);
        private readonly Dictionary<string, bool> IsFlowerCache = new();

        private int StaminaIncreaseAmt = 0;
        private bool debugging = false;

        private readonly IDictionary<int, int> ResourceUpgradeLevelsNeeded = new Dictionary<int, int>
        {
            [ResourceClump.stumpIndex] = Tool.copper,
            [ResourceClump.hollowLogIndex] = Tool.steel
        };


        public override void Entry(IModHelper helper)
        {
            //Load up the config file
            Config = helper.ReadConfig<ModConfig>();

            //Set Up Events
            IModEvents events = Helper.Events;

            events.GameLoop.GameLaunched += OnGameLaunched;
            events.Input.ButtonPressed += OnButtonPressed;
            //events.GameLoop.DayStarted += OnDayStarted;
            //events.GameLoop.DayEnding += OnDayEnding;
            events.Display.RenderedWorld += OnRenderedWorld;
            //events.GameLoop.UpdateTicked += OnUpdateTicked;
            events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTicked;

        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            #region GMM Set ups
            //Set Up Generic Mod Menu Here
            _cfgMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (_cfgMenu is null) return;
            
            _cfgMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
                );
            _cfgMenu.AddSectionTitle(mod: ModManifest,text: () => "Farm Helper Settings", tooltip: null);
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Mod Enabled",
                tooltip: () => "Should the mod be enabled.",
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
                );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Show Affected Area",
                tooltip: () => "Should the affected area be shown.",
                getValue: () => Config.ShowAffectedArea,
                setValue: value => Config.ShowAffectedArea = value
            );
            _cfgMenu.AddNumberOption(
                mod: ModManifest,
                name: () => "Affected Area",
                tooltip: () => "How many tiles around the player should we look",
                getValue: () => Config.AffectedArea,
                setValue: value => Config.AffectedArea = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Enable Affected Area From Player",
                tooltip: () => "Whether or not we should have the affected area from the player or mouse pointer",
                getValue: () => Config.UsePlayerOrMouse,
                setValue: value => Config.UsePlayerOrMouse = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Disable Player Exhaustion",
                tooltip: () => "Whether or not we should disable getting exhausted from using the tools. If true, it will make us passout if we run out of stamina during use.",
                getValue: () => Config.DisableExhaustion,
                setValue: value => Config.DisableExhaustion = value
            );
            #region Keybinds
            //Keys
            _cfgMenu.AddSectionTitle(mod: ModManifest,text: () => "Key Binds", tooltip: null);
            _cfgMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Activation Key",
                tooltip: () => "The keybind to activate.",
                getValue: () => Config.Keys.ActivationKey,
                setValue: value => Config.Keys.ActivationKey = value
            );
            _cfgMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Use Tool Key",
                tooltip: () => "The keybind to use tool on tile.",
                getValue: () => Config.Keys.UseToolKey,
                setValue: value => Config.Keys.UseToolKey = value
            );
            _cfgMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Clear Current Location Key",
                tooltip: () => "The keybind to clear current location.",
                getValue: () => Config.Keys.ClearLocationKey,
                setValue: value => Config.Keys.ClearLocationKey = value
            );
            _cfgMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Gather Forage",
                tooltip: () => "The keybind to gather forage.",
                getValue: () => Config.Keys.GatherForageKey,
                setValue: value => Config.Keys.GatherForageKey = value
            );
            #endregion
            #region WateringCan Settings
            _cfgMenu.AddSectionTitle(mod: ModManifest,text: () => " Watering Can Settings", tooltip: null);
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Unlimited Water",
                tooltip: () => "Should the watering can have unlimited water.",
                getValue: () => Config.WateringCan.UnlimitedWater,
                setValue: value => Config.WateringCan.UnlimitedWater = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Unlimited Stamina",
                tooltip: () => "Should the watering can have unlimited stamina.",
                getValue: () => Config.WateringCan.UnlimitedStamina,
                setValue: value => Config.WateringCan.UnlimitedStamina = value
            );
            #endregion
            #region Hoe Settings
            _cfgMenu.AddSectionTitle(mod: ModManifest,text: () => " Hoe Settings", tooltip: null);
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Till Dirt",
                tooltip: () => "Should dirt be tilled.",
                getValue: () => Config.Hoe.TillDirt,
                setValue: value => Config.Hoe.TillDirt = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Clear Weeds",
                tooltip: () => "Should weeds be cleared.",
                getValue: () => Config.Hoe.ClearWeeds,
                setValue: value => Config.Hoe.ClearWeeds = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Dig Artifact Spots",
                tooltip: () => "Should artifact spots be dug.",
                getValue: () => Config.Hoe.DigArtifactSpots,
                setValue: value => Config.Hoe.DigArtifactSpots = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Dig Seed Spots",
                tooltip: () => "Should seed spots be dug.",
                getValue: () => Config.Hoe.DigSeedSpots,
                setValue: value => Config.Hoe.DigSeedSpots = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Harvest Ginger",
                tooltip: () => "Should ginger be harvested.",
                getValue: () => Config.Hoe.HarvestGinger,
                setValue: value => Config.Hoe.HarvestGinger = value
            );
            #endregion
            #region PickAxe Settings
            _cfgMenu.AddSectionTitle(mod: ModManifest,text: () => " Pickaxe Settings", tooltip: null);
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Clear Debris",
                tooltip: () => "Should debris be cleared.",
                getValue: () => Config.PickAxe.ClearDebris,
                setValue: value => Config.PickAxe.ClearDebris = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Clear Dead Crops",
                tooltip: () => "Should dead crops be cleared.",
                getValue: () => Config.PickAxe.ClearDeadCrops,
                setValue: value => Config.PickAxe.ClearDeadCrops = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Clear Dirt",
                tooltip: () => "Clear tilled dirt.",
                getValue: () => Config.PickAxe.ClearDirt,
                setValue: value => Config.PickAxe.ClearDirt = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Clear Flooring",
                tooltip: () => "Should floor tiles be cleared.",
                getValue: () => Config.PickAxe.ClearFlooring,
                setValue: value => Config.PickAxe.ClearFlooring = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Mine Boulders and Meteors",
                tooltip: () => "Should boulders and meteors be cleared.",
                getValue: () => Config.PickAxe.ClearBouldersAndMeteorites,
                setValue: value => Config.PickAxe.ClearBouldersAndMeteorites = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Clear Objects",
                tooltip: () => "Should objects be cleared.",
                getValue: () => Config.PickAxe.ClearObjects,
                setValue: value => Config.PickAxe.ClearObjects = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Break Mine Containers",
                tooltip: () => "Should mine containers be broken.",
                getValue: () => Config.PickAxe.BreakMineContainers,
                setValue: value => Config.PickAxe.BreakMineContainers = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Clear Weeds",
                tooltip: () => "Should weeds be cleared.",
                getValue: () => Config.PickAxe.ClearWeeds,
                setValue: value => Config.PickAxe.ClearWeeds = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Harvest Mine Spawns",
                tooltip: () => "Should spawned items in the mine be harvested.",
                getValue: () => Config.PickAxe.HarvestMineSpawns,
                setValue: value => Config.PickAxe.HarvestMineSpawns = value
            );
            #endregion
            #region Axe Settings
            _cfgMenu.AddSectionTitle(mod: ModManifest,text: () => " Axe Settings", tooltip: null);
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Clear Fruit Tree Seeds",
                tooltip: () => "Should clear down fruit tree seeds.",
                getValue: () => Config.Axe.ClearFruitTreeSeeds,
                setValue: value => Config.Axe.ClearFruitTreeSeeds = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Cut Fruit Tree Saplings",
                tooltip: () => "Should non fully grown fruit trees.",
                getValue: () => Config.Axe.ClearFruitTreeSaplings,
                setValue: value => Config.Axe.ClearFruitTreeSaplings = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Cut Grown Fruit Tree",
                tooltip: () => "Should grown fruit trees be cut down..",
                getValue: () => Config.Axe.CutGrownFruitTrees,
                setValue: value => Config.Axe.CutGrownFruitTrees = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Cut Tree Stumps",
                tooltip: () => "Should tree stumps be cut. Not the gian ones, those are with debris.",
                getValue: () => Config.Axe.CutTreeStumps,
                setValue: value => Config.Axe.CutTreeStumps = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Clear Tree Seeds",
                tooltip: () => "Should tree seeds be cleared.",
                getValue: () => Config.Axe.ClearTreeSeeds,
                setValue: value => Config.Axe.ClearTreeSeeds = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Clear Tree Saplings",
                tooltip: () => "Should tree saplings be cleared.",
                getValue: () => Config.Axe.ClearTreeSaplings,
                setValue: value => Config.Axe.ClearTreeSaplings = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Clear Grown Trees",
                tooltip: () => "Should fully grown non fruit trees be cut down.",
                getValue: () => Config.Axe.CutGrownTrees,
                setValue: value => Config.Axe.CutGrownTrees = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Cut Tapped Trees",
                tooltip: () => "Should trees that have a tapper be cut down.",
                getValue: () => Config.Axe.CutTappedTrees,
                setValue: value => Config.Axe.CutTappedTrees = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Cut Choppable Bushes",
                tooltip: () => "Cut choppable bushes.",
                getValue: () => Config.Axe.CutBushes,
                setValue: value => Config.Axe.CutBushes = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Cut Giant Crops",
                tooltip: () => "Should cut giant crops.",
                getValue: () => Config.Axe.CutGiantCrops,
                setValue: value => Config.Axe.CutGiantCrops = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Clear Live Plants",
                tooltip: () => "Should live crops be cleared.",
                getValue: () => Config.Axe.ClearLiveCrops,
                setValue: value => Config.Axe.ClearLiveCrops = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Clear Dead Crops",
                tooltip: () => "Should dead crops be cleared.",
                getValue: () => Config.Axe.ClearDeadCrops,
                setValue: value => Config.Axe.ClearDeadCrops = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Clear Debris",
                tooltip: () => "Clear debris, weeds, twigs giant stumps and fallen logs .",
                getValue: () => Config.Axe.ClearDebris,
                setValue: value => Config.Axe.ClearDebris = value
            );
            #endregion
            #region Scythe Settings
            _cfgMenu.AddSectionTitle(mod: ModManifest,text: () => " Scythe Settings", tooltip: null);
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Harvest Crops",
                tooltip: () => "Should crops be harvested.",
                getValue: () => Config.Scythe.HarvestCrops,
                setValue: value => Config.Scythe.HarvestCrops = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Harvest Flowers",
                tooltip: () => "Should flowers be harvested.",
                getValue: () => Config.Scythe.HarvestFlowers,
                setValue: value => Config.Scythe.HarvestFlowers = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Harvest Forage",
                tooltip: () => "Should forage be harvested.",
                getValue: () => Config.Scythe.HarvestForage,
                setValue: value => Config.Scythe.HarvestForage = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Harvest Fruit Trees",
                tooltip: () => "Should fruit trees be harvested.",
                getValue: () => Config.Scythe.HarvestFruitTrees,
                setValue: value => Config.Scythe.HarvestFruitTrees = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Harvest Green Rain Bushes",
                tooltip: () => "Should green rain bushes be harvested.",
                getValue: () => Config.Scythe.HarvestGreenRainBushes,
                setValue: value => Config.Scythe.HarvestGreenRainBushes = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Harvest Tree Moss",
                tooltip: () => "Should tree moss be harvested.",
                getValue: () => Config.Scythe.HarvestTreeMoss,
                setValue: value => Config.Scythe.HarvestTreeMoss = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Harvest Tree seeds",
                tooltip: () => "Should tree seeds be harvested.",
                getValue: () => Config.Scythe.HarvestTreeSeeds,
                setValue: value => Config.Scythe.HarvestTreeSeeds = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Harvest Machines",
                tooltip: () => "Should machines get harvested.",
                getValue: () => Config.Scythe.HarvestMachines,
                setValue: value => Config.Scythe.HarvestMachines = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Harvest Grass",
                tooltip: () => "Should non blue grass be harvested.",
                getValue: () => Config.Scythe.HarvestNonBlueGrass,
                setValue: value => Config.Scythe.HarvestNonBlueGrass = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Harvest Blue Grass",
                tooltip: () => "Should blue grass be harvested.",
                getValue: () => Config.Scythe.HarvestBlueGrass,
                setValue: value => Config.Scythe.HarvestBlueGrass = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Clear Dead Crops",
                tooltip: () => "Should dead crops be cleared.",
                getValue: () => Config.Scythe.ClearDeadCrops,
                setValue: value => Config.Scythe.ClearDeadCrops = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Clear Weeds",
                tooltip: () => "should weeds be cleared.",
                getValue: () => Config.Scythe.ClearWeeds,
                setValue: value => Config.Scythe.ClearWeeds = value
            );
            #endregion
            #region Weapon Settings
            _cfgMenu.AddSectionTitle(mod: ModManifest,text: () => " Weapon Settings", tooltip: null);
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Attack Monsters",
                tooltip: () => "Should monsters be attacked",
                getValue: () => Config.Weapon.AttackMonsters,
                setValue: value => Config.Weapon.AttackMonsters = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Clear Dead Crops",
                tooltip: () => "Should dead crops be cut..",
                getValue: () => Config.Weapon.ClearDeadCrops,
                setValue: value => Config.Weapon.ClearDeadCrops = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Break Mine Containers",
                tooltip: () => "Should mine containers be broken.",
                getValue: () => Config.Weapon.BreakMineContainers,
                setValue: value => Config.Weapon.BreakMineContainers = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Cut Grass",
                tooltip: () => "Should grass be cut.",
                getValue: () => Config.Weapon.HarvestGrass,
                setValue: value => Config.Weapon.HarvestGrass = value
            );
            
            _cfgMenu.AddSectionTitle(mod: ModManifest,text: () => " Animal Settings", tooltip: null);
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Pet Animals",
                tooltip: () => "Should Farm Animals be pet.",
                getValue: () => Config.Animals.EnablePetting,
                setValue: value => Config.Animals.EnablePetting = value
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Harvest Animal Products",
                tooltip: () => "Should Farm Animals products be collected.",
                getValue: () => Config.Animals.HarvestAnimalProducts,
                setValue: value => Config.Animals.HarvestAnimalProducts = value
            );
            #endregion
            #region Cheat Settings
            _cfgMenu.AddSectionTitle(mod: ModManifest,text: () => " Cheat Settings", tooltip: null);
            _cfgMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Teleport Key",
                tooltip: () => "The keybind to teleport the player to the tile where the mouse cursor is.",
                getValue: () => Config.Cheats.TeleportKey,
                setValue: value => Config.Cheats.TeleportKey = value
            );
            
            _cfgMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Mine Down Level Key",
                tooltip: () => "The keybind to teleport the player to the next mine level.",
                getValue: () => Config.Cheats.MineDownLevelKey,
                setValue: value => Config.Cheats.MineDownLevelKey = value
            );
            
            _cfgMenu.AddKeybind(
                mod: ModManifest,
                name: () => "Add Festival Score Key",
                tooltip: () => "The keybind to add a set amount of score for your character during a festival.",
                getValue: () => Config.Cheats.AddFestivalScoreKey,
                setValue: value => Config.Cheats.AddFestivalScoreKey = value
            );
           
            _cfgMenu.AddNumberOption(
            mod: ModManifest,
            name: () => "Festival Score To Add",
            tooltip: () => "How much score to add during a festival.",
            getValue: () => Config.Cheats.FestivalScoreToAdd,
            setValue: value => Config.Cheats.FestivalScoreToAdd = value
            );
            #endregion
            #endregion
        }

        private void OnOneSecondUpdateTicked(object? sender, OneSecondUpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady) return;
            

            if (Game1.player.IsSitting() && Game1.player.Stamina < Game1.player.MaxStamina)
            {
                Game1.player.Stamina += StaminaIncreaseAmt;
                StaminaIncreaseAmt++;
            }
            else
            {
                StaminaIncreaseAmt = 0;
            }
        }
        private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
        {
            if(!Config.ModEnabled) return;

            if (Config.ShowAffectedArea && Context.IsWorldReady && Game1.activeClickableMenu == null)
                CommonHelper.DrawAffectedArea(Game1.spriteBatch, Helper, Config.AffectedArea, Config.UsePlayerOrMouse);
        }

        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
        {
            if (!Config.ModEnabled) return;
            
            if (e.IsDown(Config.Keys.ActivationKey))
            {
                //What happens after a player hit the activation key
                CreateFakeTools();
                Activate();
                AddBuff();
            }

            if (e.IsDown(Config.Keys.UseToolKey))
            {
                //What happens when a player hits the use tool button.
            }

            if (e.IsDown(Config.Keys.ClearLocationKey))
            {
                //Clear the current location.
            }

            if (e.IsDown(Config.Keys.GatherForageKey))
            {
                //Gather forage from current location
            }

            if (e.IsDown(Config.Cheats.TeleportKey))
            {
                //What happens when the teleport key is pressed
                Game1.player.Position = Helper.Input.GetCursorPosition().Tile * Game1.tileSize;
            }

            if (e.IsDown(Config.Cheats.MineDownLevelKey) && Game1.currentLocation != null && Game1.currentLocation is MineShaft shaft)
            {
                //Move player down one level in the mine
                if (shaft.mineLevel > 120)
                    shaft.enterMineShaft();
                else
                {
                    var currentLevel = (Game1.currentLocation as MineShaft)?.mineLevel ?? 0;
                    Game1.enterMine(currentLevel + 1);
                }
            }

            if (e.IsDown(Config.Cheats.AddFestivalScoreKey) && Game1.isFestival())
            {
                //Add score to the player during a festival
                Game1.player.festivalScore += Config.Cheats.FestivalScoreToAdd;
            }
            
        }

        #region Custom Methods

        private void GetToolInfo(Tool tool)
        {
            switch (tool)
            {
                case WateringCan wc:
                    Monitor.Log($"Water Level: {wc.WaterLeft}/{wc.waterCanMax} Upgrade Level: {wc.UpgradeLevel} IsBottomLess: {wc.IsBottomless} Name: {wc.Name} IsEfficien: {wc.IsEfficient}");
                    break;
                case Hoe hoe:
                    Monitor.Log($"IsEfficient: {hoe.IsEfficient} Upgrade Level: {hoe.UpgradeLevel} Name:{hoe.Name}");
                    break;
                case Pickaxe pick:
                    Monitor.Log($"IsEfficient: {pick.IsEfficient} Upgrade Level: {pick.UpgradeLevel} Name:{pick.Name}");
                    break;
                case Axe axe:
                    Monitor.Log($"IsEfficient: {axe.IsEfficient} Upgrade Level: {axe.UpgradeLevel} Name:{axe.Name}");
                    break;
            }
        }
        private void CreateFakeTools()
        {
            if (Game1.player.CurrentTool is null) return;
            
            var waterLeft = 0;
            if (Game1.player.CurrentTool != null && Game1.player.CurrentTool is WateringCan wc)
            {
                waterLeft = wc.WaterLeft;
            }
            //Create Fake tools
            fakeWatering = new WateringCan
            {
                IsBottomless = Config.WateringCan.UnlimitedWater,
                IsEfficient = Config.WateringCan.UnlimitedStamina,
                WaterLeft = Config.WateringCan.UnlimitedWater ? 100 : waterLeft
            };
            fakeAxe = ItemRegistry.Create<Axe>("(T)IridiumAxe");
            fakePickAxe = new(() => new Pickaxe());
            fakeHoe = ItemRegistry.Create<Hoe>("(T)IridiumHoe");
            fakeWeapon = ItemRegistry.Create<MeleeWeapon>("(W)62");
        }
        private void AddBuff()
        {
            Buff buff = new(
                id: "Mizzion.FarmHelper.HelperBuff",
                displayName: "Magnetic Radius Buff",
                iconTexture: Helper.ModContent.Load<Texture2D>("assets/bufficon.png"),
                iconSheetIndex: 0,
                duration: 60000,
                effects: new BuffEffects()
                {
                    MagneticRadius = { 680 }
                }
                );
            Game1.player.applyBuff(buff);
        }
        private void Activate()
        {
            SFarmer player = Game1.player;
            GameLocation location = Game1.currentLocation;
            Tool? tool = player.CurrentTool;
            Item? item = player.CurrentItem;
            Vector2 origin = Config.UsePlayerOrMouse ? Game1.player.Tile : Helper.Input.GetCursorPosition().Tile;
            Vector2[] grid = CommonHelper.GetAffectedTile(origin, Config.AffectedArea).ToArray();
            IsFlowerCache.Clear();

            foreach(Vector2 tile in grid)
            {
                //Stop if player will be exhausted.
                if(Config.DisableExhaustion)
                    if (Game1.player.Stamina <= 2) return;
                //GetRadialAdjacentTile(origin, tile, out Vector2 adjacentTile, out int facindDirection);

                location.objects.TryGetValue(tile, out SObject? tileObject);
                location.terrainFeatures.TryGetValue(tile, out TerrainFeature? tileFeature);
                
                //Item is fertilizer
                if( item is { Category: SObject.fertilizerCategory })
                    ProcessFertilizer(item, tileFeature, tileObject, player, location, tile);
                
                //Item is a grass
                if(item is {QualifiedItemId: "(O)297" or "(O)BlueGrassStarter", Stack: > 0})
                    ProcessGrass(item, tileFeature, tileObject, player, location, tile);
                
                if(item is {Category: SObject.SeedsCategory, Stack: > 0})
                    PlantSeed(tile, player, location, tileFeature, tileObject, item, tool);
                
                switch (tool)
                {
                    case WateringCan:
                        UseWateringCanOnTile(tile, player, location, tileFeature, tileObject, item, tool);
                        break;
                    case Pickaxe:
                        UsePickAxeOnTile(tile, player, location, tileFeature, tileObject, item, tool);
                        break;
                    case Axe:
                        UseAxeOnTile(tile, player, location, tileFeature, tileObject, item, tool);
                        break;
                    case Hoe:
                        UseHoeOnTile(tile, player, location, tileFeature, tileObject, item, tool);
                        break;
                    case MeleeWeapon weapon when weapon.isScythe():
                        UseScytheOnTile(tile, player, location, tileFeature, tileObject, item, tool);
                        break;
                    case MeleeWeapon weapon when !weapon.isScythe():
                        UseWeaponOnTile(tile, player, location, tileFeature, tileObject, item, tool);
                        break;
                }
            }
        }
        
        
        #region Uses

        private void UseWateringCanOnTile(Vector2 tile, SFarmer player, GameLocation location, TerrainFeature? tileFeature, 
            SObject? tileObject, Item? item, Tool? tool)
        {
            if (tool is null) return;
            
            if (tool is WateringCan wc && location.CanRefillWateringCanOnTile((int)tile.X, (int)tile.Y))
            {
                wc.WaterLeft = wc.waterCanMax;
                return;
            }

            tool = Config.WateringCan.UnlimitedWater || Config.WateringCan.UnlimitedStamina ? fakeWatering : tool;
            if(CommonHelper.GetHoeDirt(tileFeature, tileObject, out HoeDirt? dirt, out bool dirtCoveredByObj, out IndoorPot? pot))
            {
                //Water Any HoeDirt
                if (dirt.state.Value != HoeDirt.watered)
                    CommonHelper.UseTool(tool, tile, player, location);
            }

            if (location is VolcanoDungeon dungeon)
            {
                int x = (int)tile.X;
                int y = (int)tile.Y;
                if (dungeon.isTileOnMap(x, y) && dungeon.waterTiles[x, y] &&
                    !dungeon.cooledLavaTiles.ContainsKey(tile))
                {
                    CommonHelper.UseTool(fakeWatering, tile, player, location);
                }
            }
            if(debugging)
                GetToolInfo(tool);
        }

        private void PlantSeed(Vector2 tile, SFarmer player, GameLocation location, TerrainFeature? tileFeature, 
            SObject? tileObject, Item? item, Tool? tool)
        {
            if (item is null) return;
            
            if (CommonHelper.GetHoeDirt(tileFeature, tileObject, out HoeDirt? dirt, out bool dirtCoveredByObj,
                    out IndoorPot? pot))
            {
                //Try to add seed
                if (!dirtCoveredByObj && 
                    !CommonHelper.IsResourceClumpOnTile(location, tile, Helper.Reflection) && 
                    (dirt.crop == null ||
                    pot?.bush.Value is null) &&
                    dirt.plant(item.ItemId, player, false))
                {
                    CommonHelper.UseItem(player, item);
                    if (CommonHelper.TryGetEnricher(location, tile, out Chest? enricher, out Item? fertilizer) &&
                        dirt.plant(fertilizer.ItemId, player, true))
                    {
                        CommonHelper.UseItem(enricher, fertilizer);
                    }
                }
            }
        }

        private void UsePickAxeOnTile(Vector2 tile, SFarmer player, GameLocation location, TerrainFeature? tileFeature, 
            SObject? tileObject, Item? item, Tool? tool)
        {
            if (tool is null) return;
            bool useTool = false;
            bool objInMine = location is MineShaft && tileObject?.IsSpawnedObject == true;

            if ((Config.PickAxe.ClearDebris && tileObject?.IsBreakableStone() == true) ||
                (Config.PickAxe.ClearFlooring && tileFeature is Flooring) ||
                (Config.PickAxe.ClearObjects && tileObject != null && !objInMine) ||
                (Config.PickAxe.BreakMineContainers && CommonHelper.BreakContainer(tile, tileObject, player, tool)) ||
                (Config.PickAxe.ClearWeeds && tileObject?.IsWeeds() == true) ||
                (tileFeature is HoeDirt dirt && tileObject is null && ((Config.PickAxe.ClearDirt && dirt.crop == null) || (Config.PickAxe.ClearDeadCrops && dirt.crop.dead.Value))))
                useTool = true;

            if (Config.PickAxe.ClearBouldersAndMeteorites)
            {
                if (CommonHelper.CanBreakBoulder(location, tile, player, tool, Helper, out Func<Tool, bool> applyTool))
                {
                    applyTool(tool);
                }
            }

            if (Config.PickAxe.HarvestMineSpawns && objInMine && CommonHelper.CheckTileAction(location, tile, player))
            {
                CommonHelper.CancelAnimation(player, FarmerSprite.harvestItemDown, FarmerSprite.harvestItemLeft, FarmerSprite.harvestItemRight, FarmerSprite.harvestItemUp);
            }


            if (useTool)
                CommonHelper.UseTool(tool, tile, player, location);
            
            if(debugging)
                GetToolInfo(tool);

        }

        private void UseAxeOnTile(Vector2 tile, SFarmer player, GameLocation location, TerrainFeature? tileFeature, 
            SObject? tileObject, Item? item, Tool? tool)
        {
            if (tool == null) return;
            
            bool useTool = Config.Axe.ClearDebris && tileObject != null && (tileObject.IsTwig() || tileObject.IsWeeds());

            switch (tileFeature)
            {
                case Tree tree:
                    useTool = ShouldCut(tree, tile, location);
                    break;
                case FruitTree tree:
                    useTool = ShouldCut(tree);
                    break;
                case Bush bush:
                    useTool = ShouldCut(bush);
                    break;
                case HoeDirt {crop: not null} dirt:
                    useTool = (Config.Axe.ClearDeadCrops && dirt.crop.dead.Value) ||
                              (Config.Axe.ClearLiveCrops && !dirt.crop.dead.Value);
                    break;
            }

            if (Config.Axe.ClearDebris || Config.Axe.CutGiantCrops)
            {
                if (CommonHelper.GetResourceClumpOnTile(location, tile, player, Helper.Reflection,
                        out ResourceClump? clump, out Func<Tool, bool>? applyTool))
                {
                    if (Config.Axe.CutGiantCrops && clump is GiantCrop)
                    {
                        applyTool(tool);
                        useTool = true;
                    }

                    if (Config.Axe.ClearDebris &&
                        ResourceUpgradeLevelsNeeded.ContainsKey(clump.parentSheetIndex.Value) && tool?.UpgradeLevel >=
                        ResourceUpgradeLevelsNeeded[clump.parentSheetIndex.Value])
                    {
                        applyTool(tool);
                        useTool = true;
                    }
                }
            }

            if (Config.Axe.CutBushes)
            {
                foreach (Bush bush in location.largeTerrainFeatures.OfType<Bush>().Where(b => b.Tile == tile))
                {
                    if (ShouldCut(bush) && tool != null && CommonHelper.UseTool(tool, tile, player, location))
                        useTool = false;
                }
            }

            if (useTool && tool != null)
            {
                if(CommonHelper.UseTool(tool, tile, player, location))
                {
                    if (tool is Axe { IsEfficient: false }) player.Stamina -= (float)player.ForagingLevel * 0.1f;
                }
                if(debugging)
                    GetToolInfo(tool);
            }
        }

        private void UseHoeOnTile(Vector2 tile, SFarmer player, GameLocation location, TerrainFeature? tileFeature, 
            SObject? tileObject, Item? item, Tool? tool)
        {
            if (tool is null) return;
            bool useTool = (Config.Hoe.ClearWeeds && tileObject?.IsWeeds() == true) ||
                           (Config.Hoe.DigSeedSpots && tileObject?.QualifiedItemId == $"{ItemRegistry.type_object}SeedSpot") ||
                           (Config.Hoe.DigArtifactSpots && tileObject?.QualifiedItemId == SObject.artifactSpotQID) ||
                           (tileFeature == null && tileObject == null/* && CommonHelper.StartCoolDown(tile.ToString(), TillDirtDelay)*/);
            
            if(Config.Hoe.HarvestGinger && tileFeature is HoeDirt dirt && dirt.crop?.whichForageCrop.Value == Crop.forageCrop_ginger.ToString() && dirt.crop.hitWithHoe((int)tile.X, (int)tile.Y, location, dirt))
                dirt.destroyCrop(showAnimation: false);
            
            if(useTool)
                CommonHelper.UseTool(tool, tile, player, location);
            
            if(debugging)
                GetToolInfo(tool);
        }

        private void UseScytheOnTile(Vector2 tile, SFarmer player, GameLocation location, TerrainFeature? tileFeature, 
            SObject? tileObject, Item? item, Tool? tool)
        {
            if (tool is null) return;
            
            if(Config.Scythe.HarvestForage && tileObject?.IsSpawnedObject == true && CommonHelper.CheckTileAction(location, tile, player))
                CommonHelper.CancelAnimation(player, FarmerSprite.harvestItemDown, FarmerSprite.harvestItemLeft, FarmerSprite.harvestItemRight, FarmerSprite.harvestItemUp);

            if (CommonHelper.GetHoeDirt(tileFeature, tileObject, out HoeDirt? dirt, out bool dirtCoveredByObj,
                    out IndoorPot? pot))
            {
                if (!dirtCoveredByObj && TryHarvestCrop(dirt, location, tile, player))
                {
                    //Do Nothing
                }

                TryHarvestBush(pot?.bush.Value);
            }

            TryHarvestMachine(tileObject);

            if (tileFeature is Grass grass && ShouldHarvest(grass) &&
                CommonHelper.HarvestGrass(grass, location, tile, player, tool))
            {
                //Do nothing
            }

            TryHarvestTree(tileFeature, tile, tool);

            TryHarvestWeeds(tileObject, location, tile, player, tool);

            Rectangle tileArea = CommonHelper.GetAbsoluteTileArea(tile);
            if (Config.Scythe.HarvestForage)
            {
                Bush? bush = tileFeature as Bush ??
                             location.largeTerrainFeatures.FirstOrDefault(b =>
                                 b.getBoundingBox().Intersects(tileArea)) as Bush;
                if (TryHarvestBush(bush))
                {
                    //Do Nothing
                }
            }

            if (Config.Scythe.HarvestGreenRainBushes)
            {
                if (CommonHelper.GetResourceClumpOnTile(location, tile, player, Helper.Reflection,
                        out ResourceClump? clump, out Func<Tool, bool> applyTool) && clump.IsGreenRainBush())
                {
                    applyTool(tool);
                }
            }
            
            if(debugging)
                GetToolInfo(tool);
        }

        private bool UseWeaponOnTile(Vector2 tile, SFarmer player, GameLocation location, TerrainFeature? tileFeature,
            SObject? tileObject, Item? item, Tool? tool)
        {
            if (tool is null) return false;
            if (Config.Weapon.ClearDeadCrops && CommonHelper.ClearDeadCrop(location, tile, tileFeature, player))
                return true;
            if (Config.Weapon.BreakMineContainers && CommonHelper.BreakContainer(tile, tileObject, player, tool))
                return true;
            if (Config.Weapon.HarvestGrass && CommonHelper.HarvestGrass(tileFeature as Grass, location, tile, player, tool))
                return true;
            if (Config.Weapon.AttackMonsters && CommonHelper.UseWeapon((MeleeWeapon)tool, tile, player, location))
                return true;
            
            return false;
        }

        private void ProcessFertilizer(Item i, TerrainFeature tf, SObject? to, SFarmer player, GameLocation location, Vector2 tile)
        {
            if (i.QualifiedItemId == "(O)805")
            {
                if (tf is Tree tree && !tree.fertilized.Value && tree.growthStage.Value < Tree.treeStage &&
                    tree.fertilize())
                {
                    CommonHelper.UseItem(player, i);
                }
            }
            else
            {
                var isHoeDirt = CommonHelper.GetHoeDirt(tf, to, out HoeDirt? dirt, out bool dirtCoveredByObj, out _);

                if (dirt != null && isHoeDirt && dirt.CanApplyFertilizer(i.QualifiedItemId) &&
                    (!dirtCoveredByObj || !CommonHelper.IsResourceClumpOnTile(location, tile, Helper.Reflection)))
                {
                    if (dirt.plant(i.ItemId, player, isFertilizer: true))
                    {
                        CommonHelper.UseItem(player, i);
                    }
                }
            }
        }

        private void ProcessGrass(Item i, TerrainFeature tf, SObject? to, SFarmer player, GameLocation location,
            Vector2 tile)
        {
            if (i is { QualifiedItemId: "(O)297" or "(O)BlueGrassStarter", Stack: > 0 } && to != null &&
                to.canBePlacedHere(location, tile) && to.placementAction(location, (int)(tile.X * Game1.tileSize),
                    (int)(tile.Y * Game1.tileSize), player))
            {
                CommonHelper.UseItem(player, i);
            }
        }
        #endregion

        private bool ShouldCut(Tree tree, Vector2 tile, GameLocation location)
        {
            if (tree.stump.Value)
                return Config.Axe.CutTreeStumps;
            if (tree.growthStage.Value == Tree.seedStage)
                return Config.Axe.ClearTreeSeeds;
            if (tree.growthStage.Value < Tree.treeStage)
                return Config.Axe.ClearTreeSaplings;
            if (Config.Axe.CutGrownTrees == Config.Axe.CutTappedTrees)
                return Config.Axe.CutGrownTrees;

            return IsTapped(tree, tile, location) ? Config.Axe.CutTappedTrees : Config.Axe.CutGrownTrees;
        }

        private bool ShouldCut(FruitTree tree)
        {
            if (tree.growthStage.Value == FruitTree.seedStage)
                return Config.Axe.ClearFruitTreeSeeds;
            if (tree.growthStage.Value < FruitTree.treeStage)
                return Config.Axe.ClearFruitTreeSaplings;
            return Config.Axe.CutGrownFruitTrees;
        }

        private bool ShouldCut(Bush bush)
        {
            return Config.Axe.CutBushes;
        }

        private bool IsTapped(Tree tree, Vector2 tile, GameLocation location)
        {
            return tree.tapped.Value || (location.objects.TryGetValue(tile, out SObject obj) && obj.IsTapper());
        }
        
        /// <summary>Get whether a crop should be harvested.</summary>
        /// <param name="crop">The crop to check.</param>
        private bool ShouldHarvest(Crop crop)
        {
            // flower
            if (this.IsFlower(crop))
                return this.Config.Scythe.HarvestFlowers;

            // forage
            if (CommonHelper.IsItemId(crop.whichForageCrop.Value, allowZero: false))
                return this.Config.Scythe.HarvestForage;

            // crop
            return this.Config.Scythe.HarvestCrops;
        }

        /// <summary>Get whether a grass should be harvested.</summary>
        /// <param name="grass">The grass to check.</param>
        private bool ShouldHarvest(Grass grass)
        {
            return grass.grassType.Value switch
            {
                Grass.blueGrass => this.Config.Scythe.HarvestBlueGrass,
                _ => this.Config.Scythe.HarvestNonBlueGrass
            };
        }

        /// <summary>Get whether a crop counts as a flower.</summary>
        /// <param name="crop">The crop to check.</param>
        private bool IsFlower([NotNullWhen(true)] Crop? crop)
        {
            if (crop == null)
                return false;

            string cropId = crop.indexOfHarvest.Value;
            if (string.IsNullOrWhiteSpace(cropId))
                return false;

            if (!IsFlowerCache.TryGetValue(cropId, out bool isFlower))
            {
                try
                {
                    isFlower = ItemRegistry.GetData(cropId)?.Category == SObject.flowersCategory;
                }
                catch
                {
                    isFlower = false;
                }
                this.IsFlowerCache[cropId] = isFlower;
            }

            return isFlower;
        }
        
        /// <summary>Harvest a bush if it's ready.</summary>
    /// <param name="bush">The bush to harvest.</param>
    /// <returns>Returns whether it was harvested.</returns>
    private bool TryHarvestBush([NotNullWhen(true)] Bush? bush)
    {
        // harvest if ready
        if (bush?.tileSheetOffset.Value == 1)
        {
            bool isTeaBush = bush.size.Value == Bush.greenTeaBush;
            bool isBerryBush = !isTeaBush && bush.size.Value == Bush.mediumBush && !bush.townBush.Value;
            if ((isTeaBush && this.Config.Scythe.HarvestCrops) || (isBerryBush && this.Config.Scythe.HarvestForage))
            {
                bush.performUseAction(bush.Tile);
                return true;
            }
        }

        return false;
    }

    /// <summary>Try to harvest the crop on a hoed dirt tile.</summary>
    /// <param name="dirt">The hoed dirt tile.</param>
    /// <param name="location">The location being harvested.</param>
    /// <param name="tile">The tile being harvested.</param>
    /// <param name="player">The current player.</param>
    /// <returns>Returns whether it was harvested.</returns>
    /// <remarks>Derived from <see cref="HoeDirt.performUseAction"/> and <see cref="HoeDirt.performToolAction"/>.</remarks>
    private bool TryHarvestCrop([NotNullWhen(true)] HoeDirt? dirt, GameLocation location, Vector2 tile, Farmer player)
    {
        if (dirt?.crop == null)
            return false;

        // clear dead crop
        if (this.Config.Scythe.ClearDeadCrops && CommonHelper.ClearDeadCrop(location, tile, dirt, player))
            return true;

        // harvest
        if (this.ShouldHarvest(dirt.crop))
        {
            CropData? data = dirt.crop.GetData();
            HarvestMethod? wasHarvestMethod = data?.HarvestMethod;

            try
            {
                if (data != null)
                    data.HarvestMethod = HarvestMethod.Scythe; // prevent player from visually stooping off of tractor to grab crop

                // scythe or pick crops
                if (dirt.crop.harvest((int)tile.X, (int)tile.Y, dirt))
                {
                    bool isScytheCrop = dirt.crop.GetHarvestMethod() == HarvestMethod.Scythe;

                    dirt.destroyCrop(showAnimation: isScytheCrop);
                    if (!isScytheCrop && location is IslandLocation && Game1.random.NextDouble() < 0.05)
                        Game1.player.team.RequestLimitedNutDrops("IslandFarming", location, (int)tile.X * 64, (int)tile.Y * 64, 5);

                    return true;
                }

                // hoe crops (e.g. ginger)
                if (dirt.crop.hitWithHoe((int)tile.X, (int)tile.Y, location, dirt))
                {
                    dirt.destroyCrop(showAnimation: false);
                    return true;
                }
            }
            finally
            {
                if (data != null)
                    data.HarvestMethod = wasHarvestMethod!.Value;
            }
        }

        return false;
    }

    /// <summary>Try to harvest the output from a machine.</summary>
    /// <param name="machine">The machine to harvest.</param>
    /// <returns>Returns whether it was harvested.</returns>
    private bool TryHarvestMachine([NotNullWhen(true)] SObject? machine)
    {
        if (this.Config.Scythe.HarvestMachines && machine != null && machine.readyForHarvest.Value && machine.heldObject.Value != null)
        {
            machine.checkForAction(Game1.player);
            return true;
        }

        return false;
    }

    /// <summary>Try to harvest a tree.</summary>
    /// <param name="terrainFeature">The tree to harvest.</param>
    /// <param name="tile">The tile being harvested.</param>
    /// <param name="scythe">The scythe being used.</param>
    /// <returns>Returns whether it was harvested.</returns>
    private bool TryHarvestTree([NotNullWhen(true)] TerrainFeature? terrainFeature, Vector2 tile, Tool scythe)
    {
        switch (terrainFeature)
        {
            case FruitTree tree:
                if (this.Config.Scythe.HarvestFruitTrees && tree.fruit.Count > 0)
                {
                    tree.performUseAction(tile);
                    return true;
                }
                break;

            case Tree tree:
                if (tree.hasSeed.Value && !tree.tapped.Value)
                {
                    bool shouldHarvest = tree.treeType.Value is (Tree.palmTree or Tree.palmTree2)
                        ? Config.Scythe.HarvestFruitTrees
                        : Config.Scythe.HarvestTreeSeeds;

                    if (shouldHarvest && tree.performUseAction(tile))
                        return true;
                }

                if (tree.hasMoss.Value && Config.Scythe.HarvestTreeMoss)
                {
                    if (tree.performToolAction(scythe, 0, tile))
                        return true;
                }
                break;
        }

        return false;
    }

    /// <summary>Try to harvest weeds.</summary>
    /// <param name="weeds">The weeds to harvest.</param>
    /// <param name="location">The location being harvested.</param>
    /// <param name="tile">The tile being harvested.</param>
    /// <param name="player">The current player.</param>
    /// <param name="tool">The tool selected by the player (if any).</param>
    /// <returns>Returns whether it was harvested.</returns>
    private bool TryHarvestWeeds([NotNullWhen(true)] SObject? weeds, GameLocation location, Vector2 tile, Farmer player, Tool tool)
    {
        if (Config.Scythe.ClearWeeds && weeds?.IsWeeds() == true)
        {
            CommonHelper.UseTool(tool, tile, player, location); // doesn't do anything to the weed, but sets up for the tool action (e.g. sets last user)
            weeds.performToolAction(tool); // triggers weed drops, but doesn't remove weed
            location.removeObject(tile, false);
            return true;
        }

        return false;
    }
        #endregion
    }
}
