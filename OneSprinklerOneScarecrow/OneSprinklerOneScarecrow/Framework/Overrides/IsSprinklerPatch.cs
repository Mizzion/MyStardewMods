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
            __result = __instance.GetBaseRadiusForSprinkler() >= 0 ||
                       __instance.QualifiedItemID == $"(O){HaxorSprinkler.ItemID}";

        return __result;
        }
    }
}
