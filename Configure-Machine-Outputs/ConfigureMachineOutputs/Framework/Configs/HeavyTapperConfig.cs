namespace ConfigureMachineOutputs.Framework.Configs
{
    internal class HeavyTapperConfig
    {
        public bool CustomHeavyTapperEnabled { get; set; } = true;
        //public int HeavyTapperInputMultiplier { get; set; } = 1;
        public int HeavyTapperMinOutput { get; set; } = 1;
        public int HeavyTapperMaxOutput { get; set; } = 2;
    }
}
