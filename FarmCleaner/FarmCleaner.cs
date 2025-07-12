using FarmCleaner.Framework.Configs;
using GenericModConfigMenu;
using Microsoft.Xna.Framework;
using MyStardewMods.Common;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Constants;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Logging;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using xTile.Dimensions;
using SObject = StardewValley.Object;
using STree = StardewValley.TerrainFeatures.Tree;

namespace FarmCleaner
{
    public class FarmCleaner : Mod
    {
        private ModConfig _config;
        public ITranslationHelper I18N;
        
        private readonly List<Vector2> _mineLadders = new();

        #region Lists
        private List<string> weeds = new() { "313", "314", "315", "316", "317", "318", "31", "320", "321", "674", "675", "784", "785" };
        private List<string> stones = new() { "32", "34", "36", "38", "40", "42", "48", "50", "52", "54", "56", "58", "343", "450", "668", "670", "760", "762" };
        private List<string> ores = new() { "95", "290", "751", "764", "765", "849", "850" };
        private List<string> gems = new() { "2", "4", "6", "8", "10", "12", "14", "44" };
        private List<string> twigs = new() { "294", "295" };
        private List<string> barrels = new() { "118", "120", "122" };
        #endregion

        //Debugging
        bool debugging = false;
        public override void Entry(IModHelper helper)
        {
            _config = helper.ReadConfig<ModConfig>();
            I18N = helper.Translation;

            //Events
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.Input.ButtonPressed += OnButtonPressed;
            helper.Events.GameLoop.GameLaunched += GameLaunched;
            //Add the commands we will need. 
            //helper.ConsoleCommands.Add("fc_restore", "Restores your farm back to before the clean was ran.", Restore);

        }

        private void GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            #region Generic Mod Config Menu
            var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            //Lets set this up
            // register mod
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => this._config = new ModConfig(),
                save: () => this.Helper.WriteConfig(this._config)
            );
            #region General Settings
            configMenu.AddSectionTitle(ModManifest, I18N.Get("generalsettings").ToString);
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18N.Get("general.modenabled.title"),
                tooltip: () => I18N.Get("general.modenabled.description"),
                getValue: () => this._config.GeneralConfigs.EnableMod,
                setValue: value => this._config.GeneralConfigs.EnableMod = value
            );
            #endregion

            #region Resource Settings
            configMenu.AddSectionTitle(ModManifest, I18N.Get("resourcesettings").ToString);
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18N.Get("resource.removegrass.title"),
                tooltip: () => I18N.Get("resource.removegrass.description"),
                getValue: () => this._config.ResourceConfigs.GrassRemoval,
                setValue: value => this._config.ResourceConfigs.GrassRemoval = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18N.Get("resource.weed.title"),
                tooltip: () => I18N.Get("resource.weed.description"),
                getValue: () => this._config.ResourceConfigs.WeedRemoval,
                setValue: value => this._config.ResourceConfigs.WeedRemoval = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18N.Get("resource.removestone.title"),
                tooltip: () => I18N.Get("resource.removestone.description"),
                getValue: () => this._config.ResourceConfigs.StoneRemoval,
                setValue: value => this._config.ResourceConfigs.StoneRemoval = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18N.Get("resource.breakores.title"),
                tooltip: () => I18N.Get("resource.breakores.description"),
                getValue: () => this._config.ResourceConfigs.BreakOres,
                setValue: value => this._config.ResourceConfigs.BreakOres = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18N.Get("resource.removetwig.title"),
                tooltip: () => I18N.Get("resource.removetwig.description"),
                getValue: () => this._config.ResourceConfigs.TwigRemoval,
                setValue: value => this._config.ResourceConfigs.TwigRemoval = value
            );
            #endregion

            #region Resource Clump Settings
            configMenu.AddSectionTitle(ModManifest, I18N.Get("resourceclumpsettings").ToString);
            //Stump Removal
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18N.Get("clump.removestump.title"),
                tooltip: () => I18N.Get("clump.removestump.description"),
                getValue: () => this._config.ResourceClumpConfigs.RemoveStumps,
                setValue: value => this._config.ResourceClumpConfigs.RemoveStumps = value
            );
            //Large Log Removal
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18N.Get("clump.removelog.title"),
                tooltip: () => I18N.Get("clump.removelog.description"),
                getValue: () => this._config.ResourceClumpConfigs.RemoveLargeLogs,
                setValue: value => this._config.ResourceClumpConfigs.RemoveLargeLogs = value
            );
            //Large Stone Removal
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18N.Get("clump.largestone.title"),
                tooltip: () => I18N.Get("clump.largestone.description"),
                getValue: () => this._config.ResourceClumpConfigs.RemoveLargeStones,
                setValue: value => this._config.ResourceClumpConfigs.RemoveLargeStones = value
            );
            #endregion

            #region Forage Settings
            configMenu.AddSectionTitle(ModManifest, I18N.Get("foragesettings").ToString);
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18N.Get("forage.collect.title"),
                tooltip: () => I18N.Get("forage.collect.description"),
                getValue: () => this._config.ForageConfigs.CollectForage,
                setValue: value => this._config.ForageConfigs.CollectForage = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18N.Get("forage.shakebush.title"),
                tooltip: () => I18N.Get("forage.shakebush.description"),
                getValue: () => this._config.ForageConfigs.ShakeBushes,
                setValue: value => this._config.ForageConfigs.ShakeBushes = value
            );
            #endregion            

            #region Tree Settings
            configMenu.AddSectionTitle(ModManifest, I18N.Get("treeremovesettings").ToString);
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18N.Get("tree.cleartrees.title"),
                tooltip: () => I18N.Get("tree.cleartrees.description"),
                getValue: () => this._config.TreeConfigs.RemoveTrees,
                setValue: value => this._config.TreeConfigs.RemoveTrees = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18N.Get("tree.leaverandomtree.title"),
                tooltip: () => I18N.Get("tree.leaverandomtree.description"),
                getValue: () => this._config.TreeConfigs.LeaveRandomTree,
                setValue: value => this._config.TreeConfigs.LeaveRandomTree = value
            );
            configMenu.AddNumberOption(
                mod: this.ModManifest,
                name: () => I18N.Get("tree.randomchance.title"),
                tooltip: () => I18N.Get("tree.randomchance.description"),
                min: 0,
                max: 100,
                getValue: () => this._config.TreeConfigs.RandomTreeChance,
                setValue: value => this._config.TreeConfigs.RandomTreeChance = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18N.Get("tree.shake.title"),
                tooltip: () => I18N.Get("tree.shake.description"),
                getValue: () => this._config.TreeConfigs.ShakeTree,
                setValue: value => this._config.TreeConfigs.ShakeTree = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18N.Get("tree.removeseed.title"),
                tooltip: () => I18N.Get("tree.removeseed.description"),
                getValue: () => this._config.TreeConfigs.RemoveSeed,
                setValue: value => this._config.TreeConfigs.RemoveSeed = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18N.Get("tree.removesprout.title"),
                tooltip: () => I18N.Get("tree.removesprout.description"),
                getValue: () => this._config.TreeConfigs.RemoveSprout,
                setValue: value => this._config.TreeConfigs.RemoveSprout = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18N.Get("tree.removesapling.title"),
                tooltip: () => I18N.Get("tree.removesapling.description"),
                getValue: () => this._config.TreeConfigs.RemoveSapling,
                setValue: value => this._config.TreeConfigs.RemoveSapling = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18N.Get("tree.removesmalltree.title"),
                tooltip: () => I18N.Get("tree.removesmalltree.description"),
                getValue: () => this._config.TreeConfigs.RemoveSmallTree,
                setValue: value => this._config.TreeConfigs.RemoveSmallTree = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18N.Get("tree.removemature.title"),
                tooltip: () => I18N.Get("tree.removemature.description"),
                getValue: () => this._config.TreeConfigs.RemoveMatureTree,
                setValue: value => this._config.TreeConfigs.RemoveMatureTree = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18N.Get("tree.removetapped.title"),
                tooltip: () => I18N.Get("tree.removetapped.description"),
                getValue: () => this._config.TreeConfigs.RemoveTappedTree,
                setValue: value => this._config.TreeConfigs.RemoveTappedTree = value
            );
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18N.Get("tree.removefruit.title"),
                tooltip: () => I18N.Get("tree.removefruit.description"),
                getValue: () => this._config.TreeConfigs.RemoveFruitTree,
                setValue: value => this._config.TreeConfigs.RemoveFruitTree = value
            );
            #endregion

            #region Misc Settings
            configMenu.AddSectionTitle(ModManifest, I18N.Get("miscsettings").ToString);
            configMenu.AddBoolOption(
                mod: this.ModManifest,
                name: () => I18N.Get("misc.breakbarrels.title"),
                tooltip: () => I18N.Get("misc.breakbarrels.description"),
                getValue: () => this._config.MiscConfigs.BreakBarrels,
                setValue: value => this._config.MiscConfigs.BreakBarrels = value
            );
            #endregion
            
            #region Keybind Settings
            configMenu.AddSectionTitle(ModManifest, I18N.Get("keybinds").ToString);
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => I18N.Get("keybind.getlocationdata.title"),
                tooltip: () => I18N.Get("keybind.getlocationdata.description"),
                getValue: () => _config.KeybindConfigs.GetLocationData,
                setValue: value => _config.KeybindConfigs.GetLocationData = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => I18N.Get("keybind.chestlocation.title"),
                tooltip: () => I18N.Get("keybind.chestlocation.description"),
                getValue: () => _config.KeybindConfigs.SetChestLocationAndSpawnIt,
                setValue: value => _config.KeybindConfigs.SetChestLocationAndSpawnIt = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => I18N.Get("keybind.clearlocation.title"),
                tooltip: () => I18N.Get("keybind.clearlocation.description"),
                getValue: () => _config.KeybindConfigs.ClearLocation,
                setValue: value => _config.KeybindConfigs.ClearLocation = value
            );
            #endregion

            #endregion
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || !Game1.getFarm().IsOutdoors || !_config.GeneralConfigs.EnableMod)
                return;
            if (e.IsDown(SButton.NumPad4))
            {
                GetLocationData();
            }

            if (e.IsDown(_config.KeybindConfigs.ClearLocation))
            {
                /*
                if (Game1.player.currentLocation is not null && !Game1.player.currentLocation.Name.Contains("Farm"))
                {
                    DoLocationClean();
                }
                else if(Game1.player.currentLocation is not null)
                {
                    //DoClean();
                    Game1.showGlobalMessage($"Can't clean {Game1.player.currentLocation.NameOrUniqueName}.");
                    return;
                }*/
                Farmer player = Game1.player;
                //bool isDebugging = player.Name.Contains("Debug") || player.Name.Contains("debug") || player.Name.Contains("Test") || player.Name.Contains("test");

                if (debugging)
                {
                    CleanWholeMap(player);
                }
                else if (Game1.player.currentLocation is not null && Game1.player.currentLocation.NameOrUniqueName.Contains("Farm") && !debugging)
                {
                    DoLocationClean();
                }
                else
                {
                    Game1.showGlobalMessage($"Can't clean {Game1.player.currentLocation.NameOrUniqueName}.");
                    return;
                }
                if (e.IsDown(_config.KeybindConfigs.GetLocationData))
                {
                    GetObjects(Game1.player.currentLocation);
                }
                if (e.IsDown(SButton.NumPad7))
                {
                    if (Game1.player.currentLocation is MineShaft mine)
                    {
                        var x = (Game1.player.getStandingPosition().X / 64) + 1;
                        var y = (Game1.player.getStandingPosition().Y / 64) + 1;
                        mine.createLadderAt(new Vector2(x, y));
                        Monitor.Log($"X: {x}, Y: {y}", LogLevel.Debug);
                    }
                }
                if (e.IsDown(_config.KeybindConfigs.SetChestLocationAndSpawnIt))
                {

                    //Spawn Chest
                    Chest myChest = new(true, _config.GeneralConfigs.ItemChestLocation);
                    GameLocation loc = Game1.player.currentLocation;
                    loc.objects.TryGetValue(_config.GeneralConfigs.ItemChestLocation, out SObject chest);
                    if (chest is not null && chest is Chest)
                    {
                        Game1.showGlobalMessage($"Theres already a chest at : {_config.GeneralConfigs.ItemChestLocation.ToString()}");
                    }
                    else
                    {
                        _config.GeneralConfigs.ItemChestLocation = Helper.Input.GetCursorPosition().Tile;//new Vector2(Game1.getMousePosition().X, Game1.getMousePosition().Y);
                        Helper.WriteConfig(_config);

                        _config = Helper.ReadConfig<ModConfig>();
                        loc.objects.Add(_config.GeneralConfigs.ItemChestLocation, myChest);
                        Game1.showGlobalMessage($"Chest was added Chest location set to: {_config.GeneralConfigs.ItemChestLocation}");
                    }

                }
                if (e.IsDown(SButton.F5))
                {
                    _config = Helper.ReadConfig<ModConfig>();
                    Monitor.Log("Config was reloaded.");
                }
                if (e.IsDown(SButton.NumPad5))
                {
                    CleanWholeMap(Game1.player);
                }
            }
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            Farmer player = Game1.player;
            debugging = player.Name.Contains("Debug") || player.Name.Contains("debug") || player.Name.Contains("Test") || player.Name.Contains("test");//Game1.player.Name.Contains("Debug") || Game1.player.Name.Contains("debug");
            Monitor.Log($"Debugging is set to: {debugging}", LogLevel.Error);
        }

        private void GetLocationData()
        {
            var loc = Game1.player.currentLocation;
            //Make sure loc isn't null
            if (loc is null)
            {
                Monitor.Log($"Couldn't find a game location.");
                return;
            }    
            foreach(var objs in loc.objects.Pairs.ToList())
            {
                Monitor.Log($"Object: {objs.Value.DisplayName}({objs.Value.Name}) with ID: {objs.Key} at {objs.Value.TileLocation.ToString()}");
            }
        }
       
        private void CleanWholeMap(Farmer who)
        {
            if(who is null)
            {
                Monitor.Log("No player found.");
                return;
            }

            var location = who.currentLocation;
            if (location is null)
            {
                Monitor.Log("No location found.");
                return;
            }

            if (location is not GameLocation)
            {
                Monitor.Log("Current location is not a valid game location.");
                return;
            }

            var objs = location.objects.Pairs.ToList();
            var terrains = location.terrainFeatures.Pairs.ToList();
            var resourceClumps = location.resourceClumps.ToList();
            var characters = location.characters.ToList();

            //Create tools
            var newAxe = ItemRegistry.Create<Axe>("(T)IridiumAxe");
            var newPickaxe = ItemRegistry.Create<Pickaxe>("(T)IridiumPickaxe");
            var newScythe = ItemRegistry.Create<MeleeWeapon>("(W)66");
            var newWep = ItemRegistry.Create<MeleeWeapon>("(W)62");

            Random rnd = Utility.CreateDaySaveRandom((int)Game1.stats.DaysPlayed + (int)who.getStandingPosition().X * 7 + (int)who.getStandingPosition().Y * 11);
            if (objs.Count != 0) 
            {
                foreach (var obj in objs)
                {
                    if (obj.Value.IsWeeds() == true && _config.ResourceConfigs.WeedRemoval)
                    {
                        if(debugging)
                            Monitor.Log($"Weed found at {obj.Value.TileLocation.ToString()} with ID: {obj.Value.ItemId}.");
                        //obj.Value.cutWeed(who);
                        //CommonHelper.UseTool(newAxe, obj.Value.TileLocation, who, location);
                        Tool tool = newScythe as Tool;
                        if(tool is null)
                            Monitor.Log("Weed(scythe) is null. This should not happen.", LogLevel.Error);
                        //obj.Value.MinutesUntilReady = 0;
                        CommonHelper.UseTool(tool, obj.Value.TileLocation, who, location);
                        obj.Value.performToolAction(tool);
                         location.removeObject(obj.Value.TileLocation, false);

                        //CommonHelper.
                    }

                    if (obj.Value.IsTwig() == true && _config.ResourceConfigs.TwigRemoval)
                    {
                        Tool tool = newAxe as Tool;
                        if (debugging)
                            Monitor.Log($"Twig found at {obj.Value.TileLocation} with ID: {obj.Value.ItemId}. Tool: {tool.Name} Upgrade: {tool.UpgradeLevel}");
                        if (tool is null)
                            Monitor.Log("twig(scythe) is null. This should not happen.", LogLevel.Error);
                        //obj.Value.MinutesUntilReady = 0;
                        CommonHelper.UseTool(tool, obj.Value.TileLocation, who, location);
                        obj.Value.performToolAction(tool);
                        location.removeObject(obj.Value.TileLocation, false);
                    }

                    if (obj.Value.IsBreakableStone() == true && (_config.ResourceConfigs.StoneRemoval || _config.ResourceConfigs.BreakOres))
                    {
                        if(debugging)
                            Monitor.Log($"Stone found at {obj.Value.TileLocation.ToString()} with ID: {obj.Value.ItemId}.");
                        Tool tool = newPickaxe as Tool;
                        if (tool is null)
                            Monitor.Log("stone(pickaxe) is null. This should not happen.", LogLevel.Error);
                        //obj.Value.MinutesUntilReady = 0;
                        CommonHelper.UseTool(tool, obj.Value.TileLocation, who, location);
                        obj.Value.performToolAction(tool);
                        location.removeObject(obj.Value.TileLocation, false);
                    }

                    if(obj.Value is BreakableContainer)
                    {
                        Tool tool = newWep as Tool;
                        if (debugging)
                            Monitor.Log($"Barrel found at {obj.Value.TileLocation.ToString()} with ID: {obj.Value.ItemId}.");
                        if (tool is null)
                            Monitor.Log("Barrel(Weapon) is null. This should not happen.", LogLevel.Error);

                        CommonHelper.UpdateToolBeforeUse(tool, obj.Value.TileLocation, who);
                        obj.Value.performToolAction(tool);
                    }

                    if(obj.Value is { TypeDefinitionId: ItemRegistry.type_object, Name: "SupplyCrate" } and not Chest && CommonHelper.UpdateToolBeforeUse((newWep as Tool), obj.Value.TileLocation, who) && obj.Value.performToolAction(newWep as Tool))
                    {
                        if (debugging)
                            Monitor.Log($"Supply Crate found at {obj.Value.TileLocation.ToString()} with ID: {obj.Value.ItemId}.");
                        location.removeObject(obj.Value.TileLocation, false);
                        //Game1.createItemDebris(obj.Value, obj.Value.TileLocation, 0, location);
                        location.objects.Remove(obj.Value.TileLocation);
                    }

                    if ((obj.Value.isForage() || obj.Value.IsSpawnedObject) && _config.ForageConfigs.CollectForage)
                    {
                        if(debugging)
                            Monitor.Log($"Forage found at {obj.Value.TileLocation.ToString()} with ID: {obj.Value.ItemId}.");
                        CommonHelper.CheckTileAction(location, obj.Value.TileLocation, who);
                    }
                }
            }

            if (terrains.Count != 0) 
            {
                foreach (var ter in terrains)
                {
                    if (_config.TreeConfigs.LeaveRandomTree && rnd.Next(1, 100) < _config.TreeConfigs.RandomTreeChance)
                        continue;

                    Tool tool = newAxe as Tool;

                    if (ter.Value is Tree tree && _config.TreeConfigs.RemoveTrees)
                    {
                        //Remove trees
                        if (_config.TreeConfigs.RemoveSeed && STree.seedStage == (int)CommonHelper.TreeStage.Seed)
                        {
                            tree.health.Value = 1;
                            CommonHelper.UseTool(tool, ter.Key, who, location);
                        }
                        else if (_config.TreeConfigs.RemoveSprout && STree.sproutStage == (int)CommonHelper.TreeStage.Sprout)
                        {
                            tree.health.Value = 1;
                            CommonHelper.UseTool(tool, ter.Key, who, location);
                        }
                        else if (_config.TreeConfigs.RemoveSapling && STree.saplingStage == (int)CommonHelper.TreeStage.Sapling)
                        {
                            tree.health.Value = 1;
                            CommonHelper.UseTool(tool, ter.Key, who, location);
                        }
                        else if (_config.TreeConfigs.RemoveSmallTree && (STree.treeStage - 1) == (int)CommonHelper.TreeStage.SmallTree)
                        {
                            tree.health.Value = 1;
                            CommonHelper.UseTool(tool, ter.Key, who, location);
                        }
                        else if (_config.TreeConfigs.RemoveMatureTree && STree.treeStage == (int)CommonHelper.TreeStage.Tree)
                        {
                            tree.health.Value = 1;
                            CommonHelper.UseTool(tool, ter.Key, who, location);
                            ter.Value.performUseAction(ter.Key); //This is needed to remove the tree from the location.
                        }                        
                        else if (_config.TreeConfigs.RemoveTappedTree && CommonHelper.IsTapped(tree, tree.Tile, location))
                        {
                            tree.health.Value = 1;
                            CommonHelper.UseTool(tool, ter.Key, who, location);
                        }
                    }
                    else if (ter.Value is FruitTree fruitTree && _config.TreeConfigs.RemoveFruitTree)
                    {
                        //Remove fruit trees
                        fruitTree.health.Value = 1;
                        CommonHelper.UseTool(tool, ter.Key, who, location);

                    }
                    else if (ter.Value is Grass grass && _config.ResourceConfigs.GrassRemoval)
                    {
                        tool = newScythe as Tool;
                        bool harvestedGrass = CommonHelper.HarvestGrass(grass, location, ter.Value.Tile, who, tool);

                        if(!harvestedGrass)
                        {
                            var amt = 1 * rnd.Next(1, 2);

                            location.terrainFeatures.Remove(ter.Key);
                            //Item i = SObject("(O)178", amt);
                            Game1.createObjectDebris("(O)178", (int)ter.Value.Tile.X, (int)ter.Value.Tile.Y, amt, location);
                        }

                    }
                }
            }


            if (resourceClumps.Count != 0)
            {
                //Resource Clumps
                foreach (var clumps in resourceClumps)
                {
                    if(clumps.parentSheetIndex.Value == 600 && _config.ResourceClumpConfigs.RemoveStumps)
                    {
                        Tool tool = newAxe as Tool;
                        if (debugging)
                            Monitor.Log($"Stump found at {clumps.Tile} with ID: {clumps.parentSheetIndex.Value}.");
                        if (tool is null)
                            Monitor.Log("Stump(Axe) is null. This should not happen.", LogLevel.Error);
                        clumps.health.Value = 0;
                        CommonHelper.UseTool(tool, clumps.Tile, who, location);
                        clumps.performToolAction(tool, 1, clumps.Tile);
                        location.resourceClumps.Remove(clumps);
                    }
                    if (clumps.parentSheetIndex.Value == 602 && _config.ResourceClumpConfigs.RemoveLargeLogs)
                    {
                        Tool tool = newAxe as Tool;
                        if (debugging)
                            Monitor.Log($"Large Log found at {clumps.Tile.ToString()} with ID: {clumps.parentSheetIndex.Value}.");
                        if (tool is null)
                            Monitor.Log("Large Log(Axe) is null. This should not happen.", LogLevel.Error);
                        clumps.health.Value = 0;
                        CommonHelper.UseTool(tool, clumps.Tile, who, location);
                        clumps.performToolAction(tool, 1, clumps.Tile);
                        location.resourceClumps.Remove(clumps);
                    }
                    if (clumps.parentSheetIndex.Value == 672 && _config.ResourceClumpConfigs.RemoveLargeStones)
                    {
                        Tool tool = newPickaxe as Tool;
                        if (debugging)
                            Monitor.Log($"Large Stone found at {clumps.Tile.ToString()} with ID: {clumps.parentSheetIndex.Value}.");
                        if (tool is null)
                            Monitor.Log("Large Stone(Pickaxe) is null. This should not happen.", LogLevel.Error);
                        clumps.health.Value = 0;
                        CommonHelper.UseTool(tool, clumps.Tile, who, location);
                        clumps.performToolAction(tool, 1, clumps.Tile);
                        location.resourceClumps.Remove(clumps);
                    }
                }
            }

            if(characters.Count != 0)
            {
                //Characters
                foreach (var character in characters)
                {
                    if(character is Monster monster)
                    {
                        CommonHelper.UseWeapon(newWep, monster.Tile, who, location);
                    }
                }
            }
            CommonHelper.AddBuff(Helper, "assets/bufficon.png", durationTime: 60, magneticRadius: true, addedMagneticRadius: 6600);

        }


        private void DoLocationClean()
        {
            GameLocation location = Game1.player.currentLocation;
            Farmer farmer = Game1.player;

            if (location is null || farmer is null)
            {
                Monitor.Log("No location or player found.");
                return;
            }

            bool isFarm = location.IsFarm;
            Random rnd = Utility.CreateDaySaveRandom((int)Game1.stats.DaysPlayed + (int)farmer.getStandingPosition().X * 7 + (int)farmer.getStandingPosition().Y * 11);
            var farm = Game1.getFarm();
            Chest itemChest = null;

            farm.objects.TryGetValue(_config.GeneralConfigs.ItemChestLocation, out SObject chest);
            if(chest is not null && chest is Chest)
            {
                itemChest = chest as Chest;
            }

            bool createDebri = itemChest is null;

            

            #region locaction.objects
            if (location.objects.Count() > 0)
            {
                var locObjs = location.objects.Pairs;

                foreach (var obj in locObjs)
                {
                    if (weeds.Contains(obj.Value.ItemId) && _config.ResourceConfigs.WeedRemoval)
                    {
                        CommonHelper.ClearLocationWeeds(location, itemChest, createDebri, obj.Value, obj.Key, farmer);
                    }
                    if (stones.Contains(obj.Value.ItemId) && _config.ResourceConfigs.StoneRemoval)
                    {
                        CommonHelper.ClearLocationStones(location, itemChest, createDebri, obj.Value, obj.Key, farmer);
                    }
                    if(ores.Contains(obj.Value.ItemId) && _config.ResourceConfigs.BreakOres)
                    {
                        CommonHelper.ClearLocationOres(location, itemChest, createDebri, obj.Value, obj.Key, farmer);
                    }
                    if (gems.Contains(obj.Value.ItemId) && _config.ResourceConfigs.BreakOres)
                    {
                        CommonHelper.ClearLocationGems(location, itemChest, createDebri, obj.Value, obj.Key, farmer);
                    }
                    if (obj.Value.isForage() && _config.ForageConfigs.CollectForage)
                    {
                        CommonHelper.ClearLocationForage(location, itemChest, createDebri, obj.Value, obj.Key, farmer);
                    }
                    if (twigs.Contains(obj.Value.ItemId) && _config.ResourceConfigs.TwigRemoval)
                    {
                        CommonHelper.ClearLocationTwigs(location, itemChest, createDebri, obj.Value, obj.Key, farmer);
                    }
                    if (barrels.Contains(obj.Value.ItemId) && _config.MiscConfigs.BreakBarrels)
                    {
                        CommonHelper.ClearLocationBarrels(location, itemChest, createDebri, obj.Value, obj.Key, farmer);
                    }
                }
            }

            #endregion

            #region location.terrainFeatures
            if (location.terrainFeatures.Count() > 0)
            {

                foreach (var ter in location.terrainFeatures.Pairs.ToList())
                {
                    //STree t = (STree) ter.Value;
                    if (ter.Value is Grass && _config.ResourceConfigs.GrassRemoval)
                    {
                        CommonHelper.ClearLocationGrass(location, itemChest, createDebri, ter.Value, ter.Key, farmer);
                    }
                    if (ter.Value is Tree && _config.TreeConfigs.RemoveTrees)
                    {
                        CommonHelper.ClearLocationTrees(location, itemChest, createDebri, ter.Value, ter.Key, farmer);
                    }
                }
            }
            #endregion

            #region location.resourceClumps
            if (location.resourceClumps.Count > 0)
            {
                foreach (var clumps in location.resourceClumps.ToList())
                {
                    if (clumps.parentSheetIndex.Value == 600 && _config.ResourceClumpConfigs.RemoveStumps)
                    {
                        CommonHelper.ClearLocationStumps(location, itemChest, createDebri, clumps, farmer);
                    }
                    if (clumps.parentSheetIndex.Value == 602 && _config.ResourceClumpConfigs.RemoveLargeLogs)
                    {
                        CommonHelper.ClearLocationLargeLogs(location, itemChest, createDebri, clumps, farmer);
                    }
                    if (clumps.parentSheetIndex.Value == 672 && _config.ResourceClumpConfigs.RemoveLargeStones)
                    {
                        CommonHelper.ClearLocationLargeStones(location, itemChest, createDebri, clumps, farmer);
                    }
                }
            }
            #endregion


        }

        public static void GetObjects(GameLocation loc)
        {
            var foundObjects = "Found Objects: ";
            var foundTerrain = "Found Terrain: ";
            var terrain = "";
            var forage = "";
            var foundForage = "Found Forage: ";
            int objNum = 0, terNum = 0, forNum = 0;
            //Objects.
            foreach (var obj in loc.objects.Pairs.ToList())
            {
                foundObjects += $" {obj.Value.Name} (ID: {obj.Value.ItemId}) X:{obj.Key.X.ToString(CultureInfo.InvariantCulture)} Y: {obj.Key.Y.ToString(CultureInfo.InvariantCulture)} in Map: {loc.Name}.\n";
                objNum++;
            }


            //TerrainFeatures
            foreach (var ter in loc.terrainFeatures.Pairs.ToList())
            {
                if (ter.Value is Grass)
                    terrain = "Grass";
                if (ter.Value is Tree tree)
                    terrain = "Tree(More Coming Soon)";
                foundTerrain += $" {terrain} X:{ter.Key.X.ToString(CultureInfo.InvariantCulture)} Y: {ter.Key.Y.ToString(CultureInfo.InvariantCulture)} in Map: {loc.Name}.\n";
                terNum++;
            }

            //Resource Clumps
            /*
            foreach (var fora in loc.)
            {
                
            }*/

            //Spit out the totals.
            //Monitor.Log($"Objects ({objNum}): {foundObjects}");
            //Monitor.Log($"Terrain Feature {terNum} {foundTerrain}");
        }


        

        private int CutWeeds()
        {
            if (Game1.random.NextDouble() < 0.5)
                return 771;
            if (Game1.random.NextDouble() < 0.05)
                return 770;

            return 0;
        }

        private int BreakStone(/*int stoneType, */int x, int y)
        {
            var random = new Random((int)Game1.stats.DaysPlayed + (int)Game1.uniqueIDForThisGame / 2 + x * 2000 + y);
            if (random.NextDouble() < 0.035 && Game1.stats.DaysPlayed > 1U)
                return 535 + (Game1.stats.DaysPlayed <= 60U || random.NextDouble() >= 0.2 ? (Game1.stats.DaysPlayed <= 120U || random.NextDouble() >= 0.2 ? 0 : 2) : 1);
            if (random.NextDouble() < 0.035 * (Game1.player.professions.Contains(21) ? 2.0 : 1.0) &&
                Game1.stats.DaysPlayed > 1U)
                return 382;
            if (random.NextDouble() < 0.01 && Game1.stats.DaysPlayed > 1U)
                return 390;

            return 390;
        }

        private int GetSeed(Tree tree)
        {
            var seedId = tree.treeType.Value switch
            {
                "1" => //Oak
                    309,
                "2" => //Maple
                    310,
                "3" => //Pine
                    311,
                "6" => //Palm
                    88,
                "7" => //Mushroom
                    891,
                "8" => //Mahogony
                    292,
                "9" => //Palm2
                    88,
                _ => 309
            };

            return seedId;
        }

        private static Item DoItem(int itemId)
        {
            Item i = new SObject($"{itemId}", 1);
            return i;
        }

        private static Item DoItem(int itemId, int amount)
        {
            Item i = new SObject($"{itemId}", amount);
        return i;
        }
        
    }
}
