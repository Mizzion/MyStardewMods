using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FerngillCommunityKeepers.Framework.Configs
{
    internal class TreeConfig
    {
        public bool CutDownTrees { get; set; } = false;

        public bool CutDownSaplings { get; set; } = false;

        public bool DigUpTreeSeeds { get; set; } = false;
    }
}
