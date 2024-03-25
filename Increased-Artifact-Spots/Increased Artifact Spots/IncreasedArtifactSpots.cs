using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Locations;
using Object = StardewValley.Object;

namespace Increased_Artifact_Spots
{
    public class IncreasedArtifactSpots : Mod
    {
        //Number of actual spawned artifact spots
        private int SpawnedSpots;
        //Debug setting
        private bool debugging;
        //The Mods config
        private ModConfig Config;
        //Populate location names
<<<<<<< Updated upstream
        private List<Tuple<string,Vector2>> locations;
=======
        private Dictionary<int, GameLocation> _locations;
        private Dictionary<GameLocation, List<Vector2>> _validSpots;
        private List<GameLocation> _validLocations;

        


>>>>>>> Stashed changes
        public override void Entry(IModHelper helper)
        {
            //Initiate the config file
            Config = helper.ReadConfig<ModConfig>();
            //Set whether or not debugging is enabled
            debugging = false;
            //Set up new Console Command
            helper.ConsoleCommands.Add("artifacts", "Shows how many Artifact Spots were spawned per location..\n\nUsage: artifacts <value>\n- value: can be all, or modded.", this.ShowSpots);
            helper.ConsoleCommands.Add("destroy_artifacts", "Will destroy all Artifact Spots.\n\nUsage: destroy_artifacts <value>\n- value: can be all, or modded.", this.DestroySpots);
            //Events
            helper.Events.GameLoop.DayStarted += DayStarted;
            helper.Events.Input.ButtonPressed += ButtonPressed;
        }

        private void ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || !debugging)
                return;
            if (e.Button == SButton.F6)
                SpawnSpots();
<<<<<<< Updated upstream
            if (e.Button == SButton.F5)
                this.Config = this.Helper.ReadConfig<ModConfig>();
        }

        public void DayStarted(object sender, DayStartedEventArgs e)
        {
=======

            if (e.IsDown(SButton.F5))
            {
                this._config = this.Helper.ReadConfig<ModConfig>();
                Monitor.Log($"Config was reloaded.");
            }

            if (e.IsDown(SButton.RightControl))
            {
                _debugging = !_debugging;
                if(_debugging)
                    Monitor.Log($"Debugging activated");
            }


            if (e.IsDown(SButton.NumPad3) && _debugging)
            {
                foreach (var loc in _validLocations)
                {
                    Random r = Utility.CreateDaySaveRandom();
                    var season = loc.GetSeason();
                    LocationData data = loc.GetData();
                
                    List<SpawnForageData> possibleForage = new List<SpawnForageData>();
                    foreach (SpawnForageData spawn in GameLocation.GetData("Default").Forage.Concat(data.Forage))
                    {
                        if ((spawn.Condition == null || GameStateQuery.CheckConditions(spawn.Condition, loc, null, null, null, r)) && (!spawn.Season.HasValue || spawn.Season == season))
                        {
                            possibleForage.Add(spawn);
                        }
                    }
                }
            }
                
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
                text: () => _i18N.Get("config_mod_name"),
                tooltip: null
            );

            _cfgMenu.AddNumberOption(
                mod: ModManifest,
                getValue: () => _config.AverageArtifactSpots,
                setValue: value => _config.AverageArtifactSpots = value,
                name: () => _i18N.Get("config_artifact_average_artifact_spots_text"),
                tooltip: () => _i18N.Get("config_artifact_average_artifact_spots_description")
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => _config.ForceAverageArtifacts,
                setValue: value => _config.ForceAverageArtifacts = value,
                name: () => _i18N.Get("config_artifact_force_average_spawn_spots_text"),
                tooltip: () => _i18N.Get("config_artifact_force_average_spawn_spots_description")
            );
            _cfgMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => _config.ShowSpawnedNumArtifactSpots,
                setValue: value => _config.ShowSpawnedNumArtifactSpots = value,
                name: () => _i18N.Get("config_artifact_show_spawned_spots_text"),
                tooltip: () => _i18N.Get("config_artifact_show_spawned_spots_description")
            );
        }


        private void DayStarted(object sender, DayStartedEventArgs e)
        {
            _validLocations = new List<GameLocation>();
            
            GetValidLocations();
            GetValidSpawnSpots();
            
>>>>>>> Stashed changes
            SpawnSpots();
        }

        private void SpawnSpots()
        {
            SpawnedSpots = 0;
            locations = new List<Tuple<string, Vector2>>();
            var i18n = Helper.Translation;
            if (Config.ShowSpawnedNumArtifactSpots)
                Game1.showGlobalMessage(i18n.Get("artifact_start"));
            foreach (GameLocation loc in Game1.locations)
            {
                
                if (loc.IsFarm || !loc.IsOutdoors)
                    continue;

                for (int i = 0; i < Config.AverageArtifactSpots; i++)
                {
<<<<<<< Updated upstream
                    int randomWidth = Game1.random.Next(loc.Map.DisplayWidth / Game1.tileSize);
                    int randomHeight = Game1.random.Next(loc.Map.DisplayHeight / Game1.tileSize);
                    Vector2 newLoc = new Vector2(randomWidth, randomHeight);
                    if (!loc.isTileLocationTotallyClearAndPlaceable(newLoc) ||
                        loc.getTileIndexAt(randomWidth, randomHeight, "AlwaysFront") != -1 ||
                        (loc.getTileIndexAt(randomWidth, randomHeight, "Front") != -1 ||
                         loc.isBehindBush(newLoc)) ||
                        (loc.doesTileHaveProperty(randomWidth, randomHeight, "Diggable", "Back") == null &&
                         (!Game1.currentSeason.Equals("winter") ||
                          loc.doesTileHaveProperty(randomWidth, randomHeight, "Type", "Back") == null ||
                          !loc.doesTileHaveProperty(randomWidth, randomHeight, "Type", "Back").Equals("Grass"))) ||
                        (loc.Name.Equals("Forest") && randomWidth >= 93 && randomHeight <= 22)) continue;
                    loc.objects.Add(newLoc, new Object(newLoc, 590, 1));
                    locations.Add(new Tuple<string, Vector2>(loc.Name, newLoc));
                    //locDictionary.Add(loc.Name, newLoc);
                    SpawnedSpots++;
                }
                if (debugging)
                    this.Monitor.Log($"Location Name: {loc.Name}, IsFarm: {loc.IsFarm}, IsOutDoors: {loc.IsOutdoors}.", LogLevel.Alert);
            }
            if (Config.ShowSpawnedNumArtifactSpots)
                Game1.showGlobalMessage(i18n.Get("artifact_spawned", new { artifact_spawns = SpawnedSpots }));
=======
                    
                    var rnd = new Random();
                    var loc = _validLocations[rnd.Next(0, _validLocations.Count - 1)];


                    var randomWidth = Game1.random.Next(loc.Map.DisplayWidth / Game1.tileSize);
                    var randomHeight = Game1.random.Next(loc.Map.DisplayHeight / Game1.tileSize);
                    var newLoc = new Vector2(randomWidth, randomHeight);



                    if (!IsValidArtifactSpot(loc, newLoc))
                    {
                        if(_debugging)
                            Monitor.Log(
                                $"({_spawnedSpots}) IsValidArtifactSpot failed for {loc.DisplayName} X:{newLoc.X} Y:{newLoc.Y}");
                        continue;
                    }

                    if (!IsValidArtifactSpot(loc, newLoc))
                        _spawnedSpots = _spawnedSpots > 1 ? _spawnedSpots-- : 0;
                    else
                        _spawnedSpots++;
                    Monitor.Log($"_spawnedSpots is now {_spawnedSpots}");
                    if(_debugging)
                        Monitor.Log(
                            $"({_spawnedSpots}) IsValidArtifactSpot passed for {loc.DisplayName} X:{newLoc.X} Y:{newLoc.Y}");

                    loc.objects.TryAdd(newLoc, ItemRegistry.Create<Object>("(O)590"));
                    _locations.TryAdd(_spawnedSpots, loc);
                    
                }
            }
            else
            {
                for (var i = 0; i < _config.AverageArtifactSpots; i++)
                {
                    var rnd = new Random();
                    tryagain:
                    var loc = _validLocations[rnd.Next(0, _validLocations.Count - 1)];

                    var randomSpot = rnd.Next(0, _validSpots[loc].Count - 1);
                    var pickedVector = _validSpots[loc][randomSpot];
                    
                    Monitor.Log($"Picked Spot was Map: {loc.DisplayName} Coords: X: {pickedVector.X}, Y: {pickedVector.Y} \r\n");


                    var randomWidth = Game1.random.Next(loc.Map.DisplayWidth / Game1.tileSize);
                    var randomHeight = Game1.random.Next(loc.Map.DisplayHeight / Game1.tileSize);
                    var newLoc = new Vector2(randomWidth, randomHeight);
                    
                    
                    if (!IsValidArtifactSpot(loc, newLoc))
                    {
                        if(_debugging)
                            Monitor.Log($"({i}) IsValidArtifactSpot failed for {loc.DisplayName} X:{newLoc.X} Y:{newLoc.Y}");
                        //continue;
                        Monitor.Log($"Location Chose was: {loc.DisplayName}, but failed", LogLevel.Error);
                        goto tryagain;
                    }

                    
                    //Monitor.Log($"Location Chose was: {loc.DisplayName}", LogLevel.Warn);
                    _spawnedSpots++;

                    if(_debugging)
                        Monitor.Log($"({i}) IsValidArtifactSpot passed for {loc.DisplayName} X:{newLoc.X} Y:{newLoc.Y}");
                    
                    loc.objects.TryAdd(pickedVector, ItemRegistry.Create<Object>("(O)590"));
                    _locations.TryAdd(_spawnedSpots, loc);


                }
            }
           
            if (_config.ShowSpawnedNumArtifactSpots)
                Game1.showGlobalMessage(_i18N.Get("artifact_spawned", new { artifact_spawns = _spawnedSpots }));
>>>>>>> Stashed changes
        }

        private void DestroySpots(string command, string[] args)
        {
            if(args.Length < 1) return;
            var arg = args[0];

            switch (arg)
            {
                case "all":
                    foreach (var loc in Game1.locations)
                    {
                        var numRemoved = 0;
                        
                        var artifactsFound = loc.objects.Pairs.Where(obj => obj.Value.QualifiedItemId == "(O)590");
                        
                        foreach (var art in artifactsFound)
                        {
                            loc.objects.Remove(art.Key);
                            numRemoved++;
                        }
                        if(numRemoved > 0)
                            Monitor.Log($"Removed {numRemoved} artifact spots from {loc.DisplayName}.", LogLevel.Info);

                    }
                    _locations.Clear();
                    break;
                case "modded":
                    foreach (var loc in Game1.locations)
                    {
                        var numRemoved = 0;
                        
                        var found = _locations.Where(obj => obj.Value.Equals(loc));
                        
                        foreach (var art in found)
                        {
                            var artFound = art.Value.objects.Pairs.Where(obj => obj.Value.QualifiedItemId == "(O)590");
                            //var newLoc = new Vector2()
                            foreach (var artt in artFound)
                            {
                                loc.objects.Remove(artt.Key);
                                numRemoved++;
                            }
                        }
                        if(numRemoved > 0)
                            Monitor.Log($"Removed {numRemoved} artifact spots from {loc.DisplayName}", LogLevel.Info);
                        
                    }
                    _locations.Clear();
                    break;
                default:
                    Monitor.Log("command must include all or modded.");
                    break;
            }
        }
        
        private void ShowSpots(string command, string[] args)
        {
            string arg = args[0];
            Dictionary<string, int> spawns = new Dictionary<string, int>();
            if (arg.ToLower() == "all")
            {
                foreach (var i in locations)
                {
                    if(!spawns.ContainsKey(i.Item1))
                        spawns.Add(i.Item1, locations.Count(x => x.Item1 == i.Item1));
                }
                if (spawns.Count != 0)
                {
                    foreach (var spawn in spawns)
                    {
                        this.Monitor.Log($"{spawn.Key}: {spawn.Value}", LogLevel.Info);
                    }
<<<<<<< Updated upstream
                }
                else
                {
                    this.Monitor.Log("The location was empty. Something may have gone wrong.", LogLevel.Info);
                }
            }
            else if (arg.ToLower() == "debug")
=======

                    if (spawns.Count != 0)
                    {
                        foreach (var spawn in spawns)
                        {
                            Monitor.Log($"{spawn.Key}: {spawn.Value}", LogLevel.Info);
                        }
                    }
                    break;
                case "modded":
                    foreach (var loc in Game1.locations)
                    {
                        /*
                        foreach (var item in locations)
                        {
                            var i = loc.objects.Pairs.Count(obj => obj.Key == item.Item2);
                            if (item.Item1 == loc.Name && i > 0)
                            {
                                found++;
                            }
                        }*/
                        var found = _locations.Count(obj => obj.Value.Equals(loc));

                        if (found != 0)
                            spawns.TryAdd(loc.Name, found);
                    }
                    if (spawns.Count != 0)
                    {
                        foreach (var spawn in spawns)
                        {
                            Monitor.Log($"{spawn.Key}: {spawn.Value}", LogLevel.Info);
                        }
                    }
                    break;
                case "debug":
                    foreach (var validSpots in _validSpots)
                    {
                        var i = validSpots.Value;
                        var spotLoc = "";
                        foreach (var vSpot in i)
                        {
                            spotLoc += $", (X: {vSpot.X}, Y: {vSpot.Y})";
                        }
                        Monitor.Log($"Map: {validSpots.Key.DisplayName} Spot: {spotLoc}");
                    }
                    break;
                default:
                    Monitor.Log("command must include all, modded or debug.");
                    break;
            }
        }



        private bool IsValidArtifactSpot(GameLocation location, Vector2 coords)
        {
            Random r = Utility.CreateDaySaveRandom();
            var Debugging = true;
            var isValid = false;
            
            var xCoord = Game1.random.Next(location.Map.DisplayWidth / Game1.tileSize);//Convert.ToInt32(coords.X); //
            var yCoord = Game1.random.Next(location.Map.DisplayHeight / Game1.tileSize);//Convert.ToInt32(coords.Y); //
            var loc = new Vector2(xCoord, yCoord);
            
            if (location.Name.Equals("Forest") && coords.X >= 93 && coords.Y <= 22)
                return false;

            

/*
            var valid1 = location.CanItemBePlacedHere(loc) &&
                   !location.IsTileOccupiedBy(loc) &&
                   location.getTileIndexAt(xCoord, yCoord, "AlwaysFront") == -1 &&
                   location.getTileIndexAt(xCoord, yCoord, "Front") == -1 &&
                   !location.isBehindBush(loc) &&
                   (location.doesTileHaveProperty(xCoord, yCoord, "Diggable", "Back") != null ||
                    (location.GetSeason() == Season.Winter &&
                     location.doesTileHaveProperty(xCoord, yCoord, "Type", "Back") != null &&
                     location.doesTileHaveProperty(xCoord, yCoord, "Type", "Back").Equals("Grass")));

           */

            return !location.IsNoSpawnTile(loc) &&
                   location.doesTileHaveProperty(xCoord, yCoord, "Spawnable", "Back") != null &&
                   location.doesTileHaveProperty(xCoord, yCoord, "Diggable", "Back") != null && 
                   location.doesTileHaveProperty(xCoord, yCoord, "Diggable", "Back") == "T" &&
                   location.doesTileHavePropertyNoNull(xCoord, yCoord, "Type", "Back").Equals("Dirt") &&
                   !location.doesEitherTileOrTileIndexPropertyEqual(xCoord, yCoord, "Spawnable", "Back", "F") &&
                   //location.CanItemBePlacedHere(loc) &&
                   location.CanItemBePlacedHere(loc, itemIsPassable: false, CollisionMask.All, CollisionMask.None) &&
                   location.getTileIndexAt(xCoord, yCoord, "AlwaysFront") == -1 &&
                   location.getTileIndexAt(xCoord, yCoord, "AlwaysFront2") == -1 &&
                   location.getTileIndexAt(xCoord, yCoord, "AlwaysFront3") == -1 &&
                   location.getTileIndexAt(xCoord, yCoord, "Front") == -1 &&
                   !location.isBehindBush(loc) &&
                   !r.NextBool(0.1) && !location.isBehindTree(loc);
        }


        private void GetValidSpawnSpots()
        {
            _validSpots = new Dictionary<GameLocation, List<Vector2>>();
            var _validCoords = new List<Vector2>();
            Random r = Utility.CreateDaySaveRandom();
            

            foreach (var loc in _validLocations)
            {
                for (var xTile = 0; xTile < loc.map.DisplayWidth / 64/*loc.Map.Layers[0].LayerWidth*/; xTile++)
                {
                    for (var yTile = 0; yTile < loc.map.DisplayHeight / 64/*loc.Map.Layers[0].LayerHeight*/; yTile++)
                    {
                        /*
                        if (loc.doesTileHavePropertyNoNull(xTile, yTile, "Type", "Back").Equals("Dirt") && IsValidArtifactSpot(loc, new Vector2(xTile, yTile)) && loc.isTileOnMap(new Vector2(xTile, yTile)))
                        {
                            _validCoords.Add(new Vector2(xTile, yTile));
                        }*/
                        

                        if (!loc.IsNoSpawnTile(new Vector2(xTile, yTile)) &&
                            loc.doesTileHaveProperty(xTile, yTile, "Spawnable", "Back") != null &&
                            loc.doesTileHaveProperty(xTile, yTile, "Diggable", "Back") != null &&
                            loc.doesTileHaveProperty(xTile, yTile, "Diggable", "Back") == "T" &&
                            loc.doesTileHavePropertyNoNull(xTile, yTile, "Type", "Back").Equals("Dirt") &&
                            !loc.doesEitherTileOrTileIndexPropertyEqual(xTile, yTile, "Spawnable", "Back",
                                "F") &&
                            loc.isTileOnMap(new Vector2(xTile, yTile)) &&
                            loc.CanItemBePlacedHere(new Vector2(xTile, yTile), itemIsPassable: false, CollisionMask.All,
                                CollisionMask.None) &&
                            loc.getTileIndexAt(xTile, yTile, "AlwaysFront") == -1 &&
                            loc.getTileIndexAt(xTile, yTile, "AlwaysFront2") == -1 &&
                            loc.getTileIndexAt(xTile, yTile, "AlwaysFront3") == -1 &&
                            loc.getTileIndexAt(xTile, yTile, "Front") == -1 &&
                            !loc.isBehindBush(new Vector2(xTile, yTile)) &&
                            !r.NextBool(0.1) && !loc.isBehindTree(new Vector2(xTile, yTile)))
                        {
                            _validCoords.Add(new Vector2(xTile, yTile));
                        }
                    }
                }

                _validSpots.TryAdd(loc, _validCoords);
            }
        }
        private void GetValidLocations()
        {
            foreach (var loc in Game1.locations)
>>>>>>> Stashed changes
            {
                foreach (var i  in locations)
                {
                    this.Monitor.Log($"{i.Item1}: {i.Item2}", LogLevel.Info);
                }
            }
        }
    }
}
