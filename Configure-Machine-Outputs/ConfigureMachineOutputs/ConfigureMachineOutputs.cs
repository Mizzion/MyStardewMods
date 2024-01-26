using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ConfigureMachineOutputs.Framework;
using ConfigureMachineOutputs.Framework.Patches;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using SObject = StardewValley.Object;

namespace ConfigureMachineOutputs
{
    public class ConfigureMachineOutputs : Mod
    {
        private CmoConfig _config;
        //public static readonly Type[] PatchedTypes = { typeof(Furniture), typeof(Wallpaper) };
        private bool _debugging = true;


        private PerformObjectDropInActionPatch podia;
        private CheckForActionPatch cfa;
        
        public override void Entry(IModHelper helper)
        {
            _config = helper.ReadConfig<CmoConfig>();

            //Events
            helper.Events.Input.ButtonPressed += ButtonPressed;

            //Make sure Customized Crystalarium mod isn't installed.
            if (helper.ModRegistry.IsLoaded("DIGUS.CustomCrystalariumMod"))
            {
                Monitor.Log("Due to incompatability issues with Customizable Crystalarium, the Crystalarium has been turned off for this mod. This way you can use both at the same time.", LogLevel.Info);
                _config.Machines.Crystalarium.CustomCrystalariumEnabled = false;
            }
            //Harmony Original Code credit goes to Cat from the SDV Modding Discord, I modified his Harmony code.
            try
            {
                var Harmony = new Harmony(this.ModManifest.UniqueID);

                //Now we set up the patches, will use a dictionary, just in case I need to expand later. Idea of using Harmony this way came from Cat#2506's mod  from the SDV discord
                IDictionary<string, Type> replacements = new Dictionary<string, Type>
                {
                    [nameof(SObject.performObjectDropInAction)] = typeof(PerformObjectDropInActionPatch),
                    [nameof(SObject.checkForAction)] = typeof(CheckForActionPatch)
                };

                IList<Type> typesToPatch = new List<Type>();
                typesToPatch.Add(typeof(SObject));
                //Let's try to get CFR Machines working. 
                //Still not sure how I want to do the input/Outputs......
                
                if (helper.ModRegistry.IsLoaded("Platonymous.CustomFarming"))
                {
                        try
                        {
                            //typesToPatch.Add(Type.GetType("CustomFarmingRedux.CustomMachine, CustomFarmingRedux"));
                            Monitor.Log("CFR Support should be active(Soon).", LogLevel.Trace);
                        }
                        catch (Exception e)
                        {
                            this.Monitor.Log("Failed to add support for CFR Machines.", LogLevel.Trace);
                            this.Monitor.Log(e.ToString(), LogLevel.Debug);
                        }
                }

                //Go through and set up the patching
                foreach (var t in typesToPatch)
                foreach (var replacement in replacements)
                {
                    var original = t.GetMethods(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(m => m.Name == replacement.Key);

                    var prefix = replacement.Value
                        .GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(item => item.Name == "Prefix");
                    var postfix = replacement.Value
                        .GetMethods(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(item => item.Name == "Postfix");

                    //this.Monitor.Log($"Patching {original} with {prefix} {postfix}", LogLevel.Trace);
                    this.Monitor.Log($"Patching {original} with {prefix} {postfix}", LogLevel.Trace);

                    Harmony.Patch(original, prefix == null ? null : new HarmonyMethod(prefix),
                        postfix == null ? null : new HarmonyMethod(postfix));
                }

            }
            catch (Exception ex)
            {
                Monitor.Log($"There was an error setting up harmony.\n {ex}", LogLevel.Trace);
            }

            //Initialize so we can get the configs.
            podia = new PerformObjectDropInActionPatch(Monitor, _config);
            cfa = new CheckForActionPatch(Monitor, _config);
            //pfp = new PerformLightningPatch(Monitor, _config);
        }
        

        private void ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;
            if (e.IsDown(SButton.NumPad8) && _debugging)
            {
                var locations = getLocations();

                foreach (var loc in locations)
                {
                    foreach (var obj in loc.objects.Values)
                    {

                        if (obj.heldObject.Value != null)
                        {
                            obj.MinutesUntilReady = 10;
                            Monitor.Log("Set all objects to 10 minutes.");
                        }
                    }
                }
                locations.Clear();
            }
            if (e.IsDown(SButton.F5))
            {
                _config = Helper.ReadConfig<CmoConfig>();
                podia = new PerformObjectDropInActionPatch(Monitor, _config);
                cfa = new CheckForActionPatch(Monitor, _config);
                Monitor.Log("The Config file was reloaded.", LogLevel.Info);
            }
        }
        

        private List<GameLocation> getLocations()
        {
            var location = new List<GameLocation>();
            foreach (var loc in Game1.locations)
            {
                if (loc.Name.Contains("Farm") || loc.Name.Contains("Green") || loc.Name.Contains("Coop") ||
                    loc.Name.Contains("Barn") || loc.Name.Contains("Cellar") || loc.Name.Contains("Shed"))
                {
                    location.Add(loc);
                    if (loc is BuildableGameLocation building)
                    {
                        foreach (Building build in building.buildings)
                        {
                            if(build.indoors.Value != null)
                                location.Add(build.indoors.Value);
                        }
                    }
                }
            }
            return location;
        }

        
    }
}
