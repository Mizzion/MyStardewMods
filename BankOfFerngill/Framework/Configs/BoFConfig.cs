namespace BankOfFerngill.Framework.Configs
{
    public class BoFConfig
    {
        public int BaseBankingInterest { get; set; } = 1; //1% interest rate.

        public bool EnableRandomEvents { get; set; } = false;//Random events, like money taken or given randomly.

        public bool EnableVaultRoomDeskActivation { get; set; } = false;
        public LoanConfig LoanSettings { get; set; } = new();
    }
}