using System.Collections.Generic;
using System.Dynamic;
using StardewValley;
using StardewValley.GameData.Machines;

namespace ConfigureMachineOutputs.Framework.Configs
{
    internal class CmoConfig
    {
        public bool ModEnabled { get; set; } = true;

        //public MachineConfig Machines { get; set; } = new MachineConfig();
        public MachineConfig MachineData { get; set; } = new MachineConfig();
    }
}
