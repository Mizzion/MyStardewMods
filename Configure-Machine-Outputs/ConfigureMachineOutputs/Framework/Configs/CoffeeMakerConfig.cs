namespace ConfigureMachineOutputs.Framework.Configs
{
    internal class CoffeeMakerConfig
    {
        public bool CustomCoffeeMakerEnabled { get; set; } = true;
        //public int CoffeeMakerInputMultiplier { get; set; } = 1;
        public int CoffeeMakerMinOutput { get; set; } = 1;
        public int CoffeeMakerMaxOutput { get; set; } = 2;
    }
}
