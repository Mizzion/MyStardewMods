namespace ConfigureMachineOutputs.Framework.Configs
{
    internal class DeconstructorConfig
    {
        public bool CustomDeconstructorEnabled { get; set; } = true;
        //public int DeconstructorInputMultiplier { get; set; } = 1;
        public int DeconstructorMinOutput { get; set; } = 1;
        public int DeconstructorMaxOutput { get; set; } = 2;
    }
}
