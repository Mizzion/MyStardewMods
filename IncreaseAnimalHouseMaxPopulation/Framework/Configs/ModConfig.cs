﻿using System.Configuration;
using IncreaseAnimalHouseMaxPopulation.Framework.Configs;
using StardewModdingAPI;

namespace IncreaseAnimalHouseMaxPopulation.Framework.Configs
{
    public class ModConfig
    {
        public bool EnableDebugMode { get; set; } = false;


        public bool EnableHoverTip { get; set; } = false;


        public SButton RefreshConfigButton { get; set; } = SButton.F5;


        public int MaxBarnPopulation { get; set; } = 20;


        public int MaxCoopPopulation { get; set; } = 20;


        public int CostPerPopulationIncrease { get; set; } = 10000;


        public bool AutoFeedExtraAnimals { get; set; } = false;


        public Cheats Cheats { get; set; } = new Cheats();

    }
}