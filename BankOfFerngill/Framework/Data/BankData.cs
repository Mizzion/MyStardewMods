namespace BankOfFerngill.Framework.Data
{
    public class BankData
    {
        public bool BankActive { get; set; } = false;
        public int MoneyInBank { get; set; } = 0;

        public int LoanedMoney { get; set; } = 0;

        public int MoneyPaidBack { get; set; } = 0;

        public int BankInterest { get; set; } = 1;

        public int LoanInterest { get; set; } = 3;

        public int NumberOfLoansPaidBack { get; set; } = 0;

        public int TotalNumberOfLoans { get; set; } = 0;
    }
}