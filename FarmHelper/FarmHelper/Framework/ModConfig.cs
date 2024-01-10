﻿using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace FarmHelper.Framework
{
    internal class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public SButton ActivationKey { get; set; } = SButton.Q;
        public SButton UseToolKey { get; set; } = SButton.Z;
        public SButton ClearLocationKey { get; set; } = SButton.R;
        public SButton GatherForageKey { get; set; } = SButton.X;
        public SButton SingleUseKey { get; set; } = SButton.V;
        public bool AutomaticMode { get; set; } = true;
        public bool EnablePetting { get; set; } = true;
        public bool EnableNotification { get; set; } = true;
        public bool EnableCost { get; set; } = true;
        public int HelperCost { get; set; } = 50;
        public bool AddItemsToInventory { get; set; } = true;
        public bool HarvestAnimalProducts { get; set; } = true;
        public Vector2 ChestLocation { get; set; } = new Vector2(68, 12);
    }
}
