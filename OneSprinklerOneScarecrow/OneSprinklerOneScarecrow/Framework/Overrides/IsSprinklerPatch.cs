using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;
using Object = StardewValley.Object;

namespace OneSprinklerOneScarecrow.Framework.Overrides
{
    internal class IsSprinklerPatch
    {
        public static bool Prefix(ref Object __instance, ref bool __result)
        {
            __result = false;
            /*
            foreach (var obj in __instance.Objects.Pairs)
            {
                if (obj.Value.Name == "Haxarecrow")
                {
                    _monitor.Log("No crows ran");
                    return false;
                }

            }

            return true;*/
            /*
            if (Utility.IsNormalObjectAtParentSheetIndex(__instance, 599))
            {
                __result = 0;
            }
            if (Utility.IsNormalObjectAtParentSheetIndex(__instance, 621))
            {
                __result = 1;
            }
            if (Utility.IsNormalObjectAtParentSheetIndex(__instance, 645))
            {
                __result = 2;
            }

            if (__instance.Name.Contains("Haxor Sprinkler"))
            {
                __result = 99999;
            }
            __result = -1;

            return __result <= 0;
            */
            if (__instance.GetBaseRadiusForSprinkler() >= 0)
                __result = true;
            if (__instance.Name.Contains("Haxor Sprinkler"))
                __result = true;
            return __result;

        }
    }
}
