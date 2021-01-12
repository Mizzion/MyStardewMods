namespace ConfigureMachineOutputs.Framework.Configs
{
    internal class SolarPanelConfig
    {
        public bool CustomSolarPanelEnabled { get; set; } = true;
        //public int SolarPanelInputMultiplier { get; set; } = 1;
        public int SolarPanelMinOutput { get; set; } = 1;
        public int SolarPanelMaxOutput { get; set; } = 2;
    }
}
