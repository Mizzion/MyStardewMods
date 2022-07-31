namespace BankOfFerngill.Framework.Configs
{
    public class HarderModeConfig
    {
        public bool EnableHarderMode { get; set; } = false;

        public int HowFarInDebtAtStart { get; set; } = 15000;

        public bool BypassHavingToRepayDebtFirst { get; set; } = false;
    }
}