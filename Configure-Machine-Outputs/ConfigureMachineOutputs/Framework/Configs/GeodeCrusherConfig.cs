namespace ConfigureMachineOutputs.Framework.Configs
{
    internal class GeodeCrusherConfig
    {
        public bool CustomGeodeCrusherEnabled { get; set; } = true;
        //public int GeodeCrusherInputMultiplier { get; set; } = 1;
        public int GeodeCrusherMinOutput { get; set; } = 1;
        public int GeodeCrusherMaxOutput { get; set; } = 2;
    }
}
