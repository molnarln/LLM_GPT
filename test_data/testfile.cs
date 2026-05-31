using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample_Exam1_BankDeposit
{
    internal class MutualFundShare : BankDeposit
    {
        private static Random rnd = new Random();
        private string equityFund;

        public string EquityFund
        {
            get { return equityFund; }
            private set
            {
                if (String.IsNullOrWhiteSpace(value))
                {
                    throw new Exception("EquityFund cannot be null or empty!");
                }
                equityFund = value;
            }
        }

        private double minimumYield;

        public double MinimumYield
        {
            get { return minimumYield; }
            set
            {
                if (value < 0.01 || value > 0.5)
                {
                    throw new Exception("Invalid minimum yield value!");
                }
                minimumYield = value;
            }
        }

        private double maximumYield;

        public double MaximumYield
        {
            get { return maximumYield; }
            set
            {
                if (value < MinimumYield || value > 0.99)
                {
                    throw new Exception("Invalid maximum yield value!");
                }
                maximumYield = value;
            }
        }

        public Risk Risk { get; set; }

        public MutualFundShare(string id, int initialCapital, double interestRate, int maturityYears, string equityFund, double minimumYield, double maximumYield, Risk risk) : base(id, initialCapital, 0.01, maturityYears)
        {
            EquityFund = equityFund;
            MinimumYield = minimumYield;
            MaximumYield = maximumYield;
            Risk = risk;
        }

        public MutualFundShare(string id, int initialCapital, double interestRate, int maturityYears, string equityFund, double minimumYield, double maximumYield) : this(id, initialCapital, interestRate, maturityYears, equityFund, minimumYield, maximumYield, Risk.Low)
        {

        }

        public override int Yield(int elapsedYears)
        {
            int accumulatedValue = Convert.ToInt32(InitialCapital * Math.Pow(1 + (InterestRate / 100), elapsedYears));

            for (int i = 0; i < elapsedYears; i++)
            {
                accumulatedValue = Convert.ToInt32(InitialCapital * (1 + (0.01 + rnd.NextDouble() * (0.5 - 0.01))));
            }

            return accumulatedValue;
        }

        public override string ToString()
        {
            return base.ToString() + $" equity fund: {EquityFund}, minimum yield: {MinimumYield}, maximum yield: {MaximumYield}, risk: {Risk}";
        }
    }
}