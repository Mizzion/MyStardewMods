using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace FarmCleaner.Framework.Configs
{
    internal class ModConfig
    {
       // public List<string> LocationsToClear { get; set; } = new List<string>(){"Farm", "Mountain"};
        public GeneralConfig GeneralConfigs { get; set; } = new GeneralConfig();

        public ResourceConfig ResourceConfigs { get; set; } = new ResourceConfig();

        public ResourceClumpConfig ResourceClumpConfigs { get; set; } = new ResourceClumpConfig();

        public ForageConfig ForageConfigs { get; set; } = new ForageConfig();


        public TreeConfig TreeConfigs { get; set; } = new TreeConfig();

        public MiscConfig MiscConfigs { get; set; } = new MiscConfig();

        public KeybindConfig KeybindConfigs { get; set; } = new KeybindConfig();

    }
}
