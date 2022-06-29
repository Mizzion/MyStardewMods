﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;

namespace OneSprinklerOneScarecrow.Framework.Overrides
{
    internal class GetBaseRadiusForSprinklerPatch
    {
        private static IMonitor _monitor;
        public GetBaseRadiusForSprinklerPatch(IMonitor monitor)
        {
            _monitor = monitor;
        }

        
        public static bool Prefix(ref Object __instance, ref int __result)
        {
            
            __result = -1;
           /*
            foreach (var obj in __instance.objects.Pairs)
            {
                if (obj.Value.Name == "Haxarecrow")
                {
                    _monitor.Log("No crows ran");
                    return false;
                }

            }

            return true;*/
            
            if (__instance.QualifiedItemID == "(O)599")
            {
                __result = 0;
            }
            if (__instance.QualifiedItemID == "(O)621")
            {
                __result = 1;
            }
            if (__instance.QualifiedItemID == "(O)645")
            {
                __result = 2;
            }

            if (__instance.QualifiedItemID == $"(O){HaxorSprinkler.ItemID}")
            {                
                __result = 999;
            }
            __result = __result >= 0 ? __result : -1;

            return __result <= 0;
            
        }
    }
}
