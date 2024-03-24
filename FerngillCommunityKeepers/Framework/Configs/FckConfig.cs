using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FerngillCommunityKeepers.Framework.Configs
{
    internal class FckConfig
    {
        public bool EnableFreeMode { get; set; } = false;

        public int HourlyRateForWorkers { get; set; } = 10;

        //TreeConfig
        public TreeConfig TreeSettings {get;set;} = new();

        //CropConfig
        public CropConfig CropSettings { get; set; } = new();

        //AnimalConfig
        public AnimalConfig AnimalSettings { get; set; } = new();
    }
}
