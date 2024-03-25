namespace ConfigureMachineOutputs.Framework.Configs
{
    internal class WoodChipperConfig
    {
        public bool CustomWoodChipperEnabled { get; set; } = true;
        //public int WoodChipperInputMultiplier { get; set; } = 1;
        public int WoodChipperMinOutput { get; set; } = 1;
        public int WoodChipperMaxOutput { get; set; } = 2;
    }
}
