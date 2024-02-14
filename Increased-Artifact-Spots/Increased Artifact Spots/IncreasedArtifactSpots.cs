using System;
using System.Collections.Generic;
using System.Linq;
using GenericModConfigMenu;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using Object = StardewValley.Object;

namespace Increased_Artifact_Spots
{
    public class IncreasedArtifactSpots : Mod
    {
        //Number of actual spawned artifact spots
        private int _spawnedSpots;

        //Debug setting
        private bool _debugging;

        //The Mods config
        private ModConfig _config;

        private ITranslationHelper _i18N;

        private IGenericModConfigMenuApi _cfgMenu;

        //Populate location names
        private Dictionary<int, GameLocation> _locations;
        private List<GameLocation> _validLocations;

        


        public override void Entry(IModHelper helper)
        {
            //Initiate the config file
            _config = helper.ReadConfig<ModConfig>();
            _i18N = Helper.Translation;

            //Set whether or not debugging is enabled
            _debugging = false;
            //Set up new Console Command
            helper.ConsoleCommands.Add("artifacts", "Shows how many Artifact Spots were spawned per location..\n\nUsage: artifacts <value>\n- value: can be all, or a location name.", this.ShowSpots);
            //Events

            helper.Events.GameLoop.GameLaunched += GameLaunched;
            helper.Events.GameLoop.DayStarted += DayStarted;
            helper.Events.Input.ButtonPressed += ButtonPressed;
        }

        public void ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;


            if (e.IsDown(SButton.F6) && _debugging)
                SpawnSpots();

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
                SpawnSpots();
            }
                
        }

        public void GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            _cfgMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
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


        public void DayStarted(object sender, DayStartedEventArgs e)
        {
            _validLocations = new List<GameLocation>();
            GetValidLocations();
            SpawnSpots();
        }

        private void SpawnSpots()
        {
            _spawnedSpots = 0;
            _locations = new Dictionary<int, GameLocation>();

            if (_config.ShowSpawnedNumArtifactSpots)
                Game1.showGlobalMessage(_i18N.Get("artifact_start"));

            if (_config.ForceAverageArtifacts)
            {
                while (_spawnedSpots < _config.AverageArtifactSpots)
                {
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

                        _spawnedSpots++;

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
                    var loc = _validLocations[rnd.Next(0, _validLocations.Count - 1)];


                    var randomWidth = Game1.random.Next(loc.Map.DisplayWidth / Game1.tileSize);
                    var randomHeight = Game1.random.Next(loc.Map.DisplayHeight / Game1.tileSize);
                    var newLoc = new Vector2(randomWidth, randomHeight);

                    if (!IsValidArtifactSpot(loc, newLoc))
                    {
                        if(_debugging)
                            Monitor.Log($"({i}) IsValidArtifactSpot failed for {loc.DisplayName} X:{newLoc.X} Y:{newLoc.Y}");
                    }

                    _spawnedSpots++;

                    if(_debugging)
                        Monitor.Log($"({i}) IsValidArtifactSpot passed for {loc.DisplayName} X:{newLoc.X} Y:{newLoc.Y}");
                    
                    loc.objects.TryAdd(newLoc, ItemRegistry.Create<Object>("(O)590"));
                    _locations.TryAdd(_spawnedSpots, loc);


                }
            }
           
            if (_config.ShowSpawnedNumArtifactSpots)
                Game1.showGlobalMessage(_i18N.Get("artifact_spawned", new { artifact_spawns = _spawnedSpots }));
        }

        private void ShowSpots(string command, string[] args)
        {
            if(args.Length < 1) return;
            var arg = args[0];
            var spawns = new Dictionary<string, int>();

            switch (arg)
            {
                case "all":
                    foreach (var loc in Game1.locations)
                    {
                        var artifactsFound = loc.objects.Pairs.Count(obj => obj.Value.QualifiedItemId == "(O)590");

                        if(artifactsFound != 0)
                            spawns.TryAdd(loc.Name, artifactsFound);
                    }

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
                default:
                    Monitor.Log("command must include all, modded or debug.");
                    break;
            }
        }



        private bool IsValidArtifactSpot(GameLocation location, Vector2 coords)
        {
            var xCoord = Convert.ToInt32(coords.X); //Game1.random.Next(location.Map.DisplayWidth / Game1.tileSize);
            var yCoord = Convert.ToInt32(coords.Y); //Game1.random.Next(location.Map.DisplayHeight / Game1.tileSize);
            var loc = new Vector2(xCoord, yCoord);
            
            if (location.Name.Equals("Forest") && coords.X >= 93 && coords.Y <= 22)
                return false;


            return location.CanItemBePlacedHere(loc) &&
                   !location.IsTileOccupiedBy(loc) &&
                   location.getTileIndexAt(xCoord, yCoord, "AlwaysFront") == -1 &&
                   location.getTileIndexAt(xCoord, yCoord, "Front") == -1 &&
                   !location.isBehindBush(loc) &&
                   (location.doesTileHaveProperty(xCoord, yCoord, "Diggable", "Back") != null ||
                    (location.GetSeason() == Season.Winter &&
                     location.doesTileHaveProperty(xCoord, yCoord, "Type", "Back") != null &&
                     location.doesTileHaveProperty(xCoord, yCoord, "Type", "Back").Equals("Grass")));
        }


        private void GetValidLocations()
        {
            foreach (var loc in Game1.locations)
            {
                var locationData = loc.GetData(); 

                if (loc.IsFarm ||
                !loc.IsOutdoors ||
                locationData is null ||
                    locationData.ArtifactSpots.Count < 1 ||
                    (loc.Name.Contains("Desert") && !Game1.MasterPlayer.mailReceived.Contains("ccVault")) ||
                    (loc.Name.Contains("Island") && !Game1.MasterPlayer.hasCompletedCommunityCenter()) ||
                    ((loc.Name.Contains("Mountain") || loc.Name.Contains("Railroad")) && Game1.stats.DaysPlayed < 31) ||
                    (loc.Name.Contains("Secret")))
                    continue;

                if(!_validLocations.Contains(loc))
                    _validLocations.Add(loc);
            }
        }
    }
}
