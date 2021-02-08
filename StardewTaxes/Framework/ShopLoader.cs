using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;

namespace StardewTaxes.Framework
{
    internal class ShopLoader : Mod
    {
        /*
         *
         * Public Methods
         *
         */
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.DayStarted += DayStarted;
        }

        /*
         *
         *Private Methods
         *
         */

        private void DayStarted(object sender, DayStartedEventArgs e)
        {
            IDictionary<ISalable, int[]> PierresShop = (Game1.getLocationFromName("SeedShop") as SeedShop)?.shopStock();

            foreach (var s in PierresShop.ToDictionary())
            {

            }
        }
    }
}
