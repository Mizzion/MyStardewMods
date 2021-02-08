namespace StardewTaxes.Framework.Configs
{
    internal class ModConfig
    {
        //Whether or not the mod is enabled.
        private bool EnableMod { get; set; } = true;
        
        //Whether debugging should be enabled. Will be used for logs.
        private bool EnableDebugging { get; set; } = false;

        //Current Tax percentage. Example 5 = 5%
        private int TaxPercentage { get; set; } = 5;
    }
}
