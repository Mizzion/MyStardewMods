using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OneSprinklerOneScarecrow.Framework;
using OneSprinklerOneScarecrow.Framework.Overrides;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using SObject = StardewValley.Object;

namespace OneSprinklerOneScarecrow
{
    public class ModEntry : Mod
    {
        private Config _config;
        
        private bool isDebugging = false;
        
        public override void Entry(IModHelper helper)
        {
            
            //helper.Events.Player.InventoryChanged += InventoryChanged;
            _config = helper.ReadConfig<Config>();
            


            //Lets activate the asset editor
            helper.Events.Content.AssetRequested += ContentEvent_AssetRequested;

            //Events that happen in the game
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;

            
            //Apply Harmony Patches
            
            var harmony = new Harmony(this.ModManifest.UniqueID);
            

            Monitor.Log("Patching Object.IsSprinkler with IsSprinklerPatch");
            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.IsSprinkler)),
                prefix: new HarmonyMethod(typeof(IsSprinklerPatch), nameof(IsSprinklerPatch.Prefix))
            );

            //Patch GetBaseRadius
            Monitor.Log("Patching Object.GetBaseRadiusForSprinkler");
            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.GetBaseRadiusForSprinkler)),
                prefix: new HarmonyMethod(typeof(GetBaseRadiusForSprinklerPatch), nameof(GetBaseRadiusForSprinklerPatch.Prefix))
                );


        }

        private void ContentEvent_AssetRequested(object sender, AssetRequestedEventArgs e)
        {
            HaxorSprinkler.TranslatedName = Helper.Translation.Get("haxorsprinkler.name");
            HaxorSprinkler.TranslatedDescription = Helper.Translation.Get("haxorsprinkler.description");
            HaxorScarecrow.TranslatedName = Helper.Translation.Get("haxorscarecrow.name");
            HaxorScarecrow.TranslatedDescription = Helper.Translation.Get("haxorscarecrow.description");
            
            //Lets start editing the content files.
            
            if (e.NameWithoutLocale.IsEquivalentTo("Data/ObjectInformation"))
            {
                e.Edit(asset =>
                {
                    var itemTexture = Helper.ModContent.GetInternalAssetName("Assets/HaxorSprinkler.png").ToString()?.Replace("/", "\\");
                    string ass = $"{HaxorSprinkler.Name}/{HaxorSprinkler.Price}/{HaxorSprinkler.Edibility}/{HaxorSprinkler.Type} {HaxorSprinkler.Category}/{HaxorSprinkler.TranslatedName}/{HaxorSprinkler.TranslatedDescription}////{HaxorSprinkler.ParentSheetIndex}/{itemTexture}";
                    asset.AsDictionary<string, string>().Data.Add($"{HaxorSprinkler.ItemID}", ass);
                    Monitor.Log($"Added Name: {HaxorSprinkler.Name}({HaxorSprinkler.TranslatedName}) Id: {HaxorSprinkler.ParentSheetIndex}.\r\n {ass}");

                    //Testing this shit.
                    if (isDebugging)
                    {
                        var s = $"{HaxorSprinkler.Name}/{HaxorSprinkler.Price}/{HaxorSprinkler.Edibility}/{HaxorSprinkler.Type} {HaxorSprinkler.Category}/{HaxorSprinkler.TranslatedName}/{HaxorSprinkler.TranslatedDescription}////{HaxorSprinkler.ParentSheetIndex}/{itemTexture}";
                        var s1 = s.Split('/');
                        var num = 0;
                        foreach (var item in s1)
                        {
                            num++;
                            var n = num - 1;
                            Monitor.Log($"{n} : {s1[n]}: {item}");
                        }
                    }
                });
                

                
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/BigCraftablesInformation"))
            {
                e.Edit(asset =>
                {
                    var itemTexture = Helper.ModContent.GetInternalAssetName("Assets/HaxorScarecrow.png").ToString()?.Replace("/", "\\");
                    asset.AsDictionary<string, string>().Data.Add($"{HaxorScarecrow.ItemID}", $"{HaxorScarecrow.Name}/{HaxorScarecrow.Price}/{HaxorScarecrow.Edibility}/{HaxorScarecrow.Type} {HaxorScarecrow.Category}/{HaxorScarecrow.TranslatedDescription}/true/false/0//{HaxorScarecrow.TranslatedName}/{HaxorScarecrow.ParentSheetIndex}/{itemTexture}");
                    Monitor.Log($"Added Name: {HaxorScarecrow.Name}({HaxorScarecrow.TranslatedName}). Id: {HaxorScarecrow.ItemID}");
                    //Testing this shit.
                    if (isDebugging)
                    {
                        var s = $"{HaxorScarecrow.Name}/{HaxorScarecrow.Price}/{HaxorScarecrow.Edibility}/{HaxorScarecrow.Type} {HaxorScarecrow.Category}/{HaxorScarecrow.TranslatedDescription}/true/false/0//{HaxorScarecrow.TranslatedName}/{HaxorScarecrow.ParentSheetIndex}/{itemTexture}";
                        var s1 = s.Split('/');
                        var num = 0;
                        foreach (var item in s1)
                        {
                            num++;
                            var n = num - 1;
                            Monitor.Log($"{n} : {s1[n]}: {item}");
                        }
                    }
                });
                
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes"))
            {

                /*
                    
                    Sprinkler
                    {390, 100}, //Easy Mode = 100 Stone
                    {386, 10} // Hard Mode = 10 Iridium Ore
        
                    Scarecrow
                    {388, 100}, //Easy Mode = 100 Wood
                    {337, 10} // Hard Mode = 10 Iridium Bars
                 
                 */
                e.Edit(asset =>
                {
                    var curData = asset.AsDictionary<string, string>();
                    //bool isEn = asset.Locale == "en";
                    string isEnSprik = asset.Locale != "en" ? $"/{HaxorSprinkler.TranslatedName}" : "";
                    string isEnScare = asset.Locale != "en" ? $"/{HaxorScarecrow.TranslatedName}" : "";
                    Monitor.Log("Made it to the else");
                    string sprinklerIngredientsOut = !_config.ActivateHarderIngredients ? $"390 100/Home/{HaxorSprinkler.ItemID}/false/null{isEnSprik}" : $"386 10/Home/{HaxorSprinkler.ItemID}/false/null{isEnSprik}";
                    string scarecrowIngredientsOut = !_config.ActivateHarderIngredients ? $"388 100/Home/{HaxorScarecrow.ItemID}/true/null{isEnScare}" : $"337 10/Home/{HaxorScarecrow.ItemID}/true/null{isEnScare}";

                    if (curData.Data.ContainsKey("Haxor Sprinkler"))
                        curData.Data["Haxor Sprinkler"] = sprinklerIngredientsOut;
                    if (curData.Data.ContainsKey("Haxor Scarecrpw"))
                        curData.Data["Haxor Scarecrow"] = scarecrowIngredientsOut;
                    if (!curData.Data.ContainsKey("Haxor Sprinkler") && !curData.Data.ContainsKey("Haxor Scarecrow"))
                    {
                        //Didn't find the recipes, now we add them
                        try
                        {
                            curData.Data.Add("Haxor Sprinkler", sprinklerIngredientsOut);
                            curData.Data.Add("Haxor Scarecrow", scarecrowIngredientsOut);
                            Monitor.Log($"Added Haxor Sprinkler Recipe: {sprinklerIngredientsOut}");
                            Monitor.Log($"Added Haxor Scarecrow: {scarecrowIngredientsOut}");
                        }
                        catch (Exception ex)
                        {
                            Monitor.Log($"There was an error editing crafting recipes. {ex}");
                        }

                    }
                });
                
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/ObjectContextTags"))
            {
                e.Edit(asset =>
                {
                    asset.AsDictionary<string, string>().Data.Add($"{HaxorScarecrow.Name}", "crow_scare, crow_scare_radius_300");
                    Monitor.Log($"Added context tags for HaxorScarecrow.");
                });
            }
        }

        /// <summary>
        /// Event gets ran when a save game is loaded.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The Event Arguments</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            //Lets run the recipe fixer
            AddRecipes();

            //Run FixLegacyItem.
            FixLegacyItems();
        }
        

        /// <summary>
        /// Method that will search the farm and make sure it gets rid of old haxor items.
        /// </summary>
        private void FixLegacyItems()
        {
            Dictionary<Vector2, SObject> obj = new Dictionary<Vector2, SObject>();
            foreach (var o in Game1.getFarm().objects.Pairs)
            {
                if (o.Value.Name.Contains("Haxor Sprinkler") && o.Value.ParentSheetIndex != HaxorSprinkler.ParentSheetIndex)
                    obj.Add(o.Key, o.Value);
                else if (o.Value.ParentSheetIndex == 273)
                    o.Value.ParentSheetIndex = HaxorScarecrow.ParentSheetIndex;
                else if (o.Value.Name.Contains("Haxor Scarecrow") && o.Value.ParentSheetIndex != HaxorScarecrow.ParentSheetIndex)
                    obj.Add(o.Key, o.Value);
            }
            foreach (var i in obj)
            {
                Game1.getFarm().objects.Remove(i.Key);
                var newSprinkler = new SObject(HaxorSprinkler.ItemID, 1);
                var newScare = new SObject(i.Key, HaxorScarecrow.ItemID);
                var replacedWith = i.Value.Name.Contains("Sprinkler") ? $"Replaced with {HaxorSprinkler.ItemID}" : $"Replaced with {HaxorScarecrow.ItemID}";
                if (i.Value.Name.Contains("Sprinkler"))
                    Game1.getFarm().objects.Add(i.Key, newSprinkler);
                if (i.Value.Name.Contains("Scare"))
                {
                    //newScare.bigCraftable.Value = true;
                    Game1.getFarm().objects.Add(i.Key, newScare);
                }
                    

                Monitor.Log($"Removed Legacy Item: Name: {i.Value.Name}, {replacedWith}");
            }

        }

        /// <summary>
        /// Method that adds/removes the recipes for the haxor items.
        /// </summary>
        private void AddRecipes()
        {
            Dictionary<string, int> curRecipes = new Dictionary<string, int>();

            foreach (var r in Game1.player.craftingRecipes.Pairs)
            {
                curRecipes.Add(r.Key, r.Value);
            }

            foreach (var c in curRecipes)
            {
                if (c.Key.Contains("Haxor"))
                {
                    Game1.player.craftingRecipes.Remove(c.Key);
                    Monitor.Log($"Removed: {c.Key} recipe");
                }
            }
            //Now that they have been removed, lets add the new ones.
            Game1.player.craftingRecipes.Add(HaxorSprinkler.Name, 0);
            Game1.player.craftingRecipes.Add(HaxorScarecrow.Name, 0);
            Monitor.Log("Added the Haxor item recipes.");
        }

        /// <summary>Get all in-game locations.</summary>
        private IEnumerable<GameLocation> GetLocations()
        {
            GameLocation[] mainLocations = (Context.IsMainPlayer ? Game1.locations : this.Helper.Multiplayer.GetActiveLocations()).ToArray();

            foreach (GameLocation location in mainLocations.Concat(MineShaft.activeMines).Concat(VolcanoDungeon.activeLevels))
            {
                yield return location;

                foreach (Building building in location.buildings)
                {
                    if (building.indoors.Value != null)
                        yield return building.indoors.Value;
                }
            }
        }
       
    }
}
