﻿namespace ConfigureMachineOutputs.Framework.Configs.Machines
{
    internal class OilMakerConfig
    {
        public bool Enabled { get; set; } = true;

        private static string Id = "(BC)19";

        public int InputMultiplier { get; set; } = 1;

        internal string QualityId { get; } = Id;
        public int MinOutput { get; set; } = 1;
        public int MaxOutput { get; set; } = 2;
    }
}
