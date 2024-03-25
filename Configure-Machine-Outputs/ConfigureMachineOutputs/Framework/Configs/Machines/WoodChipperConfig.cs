namespace ConfigureMachineOutputs.Framework.Configs.Machines
{
    internal class WoodChipperConfig
    {
        public bool Enabled { get; set; } = true;

        private static string Id = "(BC)211";

        internal string QualityId = Id;
        //public int WoodChipperInputMultiplier { get; set; } = 1;
        public int MinOutput { get; set; } = 1;
        public int MaxOutput { get; set; } = 2;
    }
}
