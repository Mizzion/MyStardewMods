namespace BankOfFerngill.Framework.Configs
{
    public class LoanConfig
    {
        public int LoanBaseInterest { get; set; } = 3; //Will be 3% interest.
        public bool PayBackLoanDaily { get; set; } = true; //Whether or not the player will payback some of the loan daily.
        public int PercentageOfLoanToPayBackDaily { get; set; } = 10; //Payback 10% of the load daily.
        public bool EnableUnlimitedLoansAtOnce { get; set; } = false; //Allows the player to have unlimited loans at once.
    }
}