namespace ConfigureMachineOutputs.Framework.Configs
{
    internal class OstrichIncubatorConfig
    {
        public bool CustomOstrichIncubatorEnabled { get; set; } = true;
        //public int OstrichIncubatorInputMultiplier { get; set; } = 1;
        public int OstrichIncubatorMinOutput { get; set; } = 1;
        public int OstrichIncubatorMaxOutput { get; set; } = 2;
    }
}
