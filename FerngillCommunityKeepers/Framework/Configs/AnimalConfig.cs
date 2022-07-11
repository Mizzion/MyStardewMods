using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FerngillCommunityKeepers.Framework.Configs
{
    internal class AnimalConfig
    {
        public bool PetAnimals { get; set; } = false;
        
        public bool FeedAnimals { get; set; } = false;

        public bool HarvestAnimalProducts { get; set; } = false;

    }
}
