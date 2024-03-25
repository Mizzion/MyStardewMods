using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewValley;

namespace FerngillCommunityKeepers.Framework
{
    /*
     *
     * Exp Nums{
     * Farming = 0
     * Fishing = 1
     * Foraging = 2
     * Mining = 3
     * Combat = 4
     * }
     *
     */

    internal class FckHelper
    {
        private IMonitor _monitor;
        private IModHelper _helper;
        public FckHelper(IMonitor monitor, IModHelper helper)
        {
            _monitor = monitor;
            _helper = helper;
        }


        //Set Exp
        public void GiveExp(ExpType exp, int amt)
        {
            Game1.player.gainExperience((int)exp, amt);
        }

        internal enum ExpType
        {
            Farming = 0,
            Fishing = 1,
            Foraging = 2,
            Mining = 3,
            Combat = 4
        }
    }
}
