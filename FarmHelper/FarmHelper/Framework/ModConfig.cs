using System.Dynamic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace FarmHelper.Framework
{
    internal class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public bool ShowAffectedArea { get; set; } = true;
        public int AffectedArea { get; set; } = 1;
        public bool UsePlayerOrMouse { get; set; } = true;
        public bool DisableExhaustion { get; set; } = false;
        
        public KeyBinds Keys { get; set; } = new();
        public WateringCanSettings WateringCan { get; set; } = new();
        public HoeSettings Hoe { get; set; } = new();
        public PickAxeSettings PickAxe { get; set; } = new();
        public AxeSettings Axe { get; set; } = new();
        public ScytheSettings Scythe { get; set; } = new();
        public WeaponSettings Weapon { get; set; } = new();
        public AnimalSettings Animals { get; set; } = new();
        
        //public bool AutomaticMode { get; set; } = true;

        //public bool EnableNotification { get; set; } = true;
        //public bool EnableCost { get; set; } = true;
        //public int HelperCost { get; set; } = 50;
        //public bool AddItemsToInventory { get; set; } = true;
        //public Vector2 ChestLocation { get; set; } = new Vector2(68, 12);
        public CheatSettings Cheats { get; set; } = new();
    }
}
