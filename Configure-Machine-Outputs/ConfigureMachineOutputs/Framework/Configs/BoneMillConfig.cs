namespace ConfigureMachineOutputs.Framework.Configs
{
    internal class BoneMillConfig
    {
        public bool CustomBoneMillEnabled { get; set; } = true;
        //public int GeodeCrusherInputMultiplier { get; set; } = 1;
        public int BoneMillMinOutput { get; set; } = 1;
        public int BoneMillMaxOutput { get; set; } = 2;
    }
}
