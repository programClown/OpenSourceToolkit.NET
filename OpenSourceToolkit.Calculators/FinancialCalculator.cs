using System;
using System.Collections.Generic;

namespace OpenSourceToolkit.Calculators
{
    /// <summary>
    /// Provides methods for financial calculations such as compound interest, loans, and investments.
    /// </summary>
    public static class FinancialCalculator
    {
        /// <summary>
        /// Calculates the future value of a principal amount with compound interest.
        /// </summary>
        /// <param name="principal">The initial amount.</param>
        /// <param name="rate">The annual interest rate (in percent).</param>
        /// <param name="timesPerYear">Number of times interest is compounded per year.</param>
        /// <param name="years">The number of years the money is invested.</param>
        /// <returns>The future value including interest.</returns>
        public static double CalculateCompoundInterest(double principal, double rate, int timesPerYear, int years)
        {
            return principal * Math.Pow(1 + (rate / 100.0 / timesPerYear), timesPerYear * years);
        }

        /// <summary>
        /// Calculates the monthly payment for a fully amortized loan.
        /// </summary>
        /// <param name="principal">The loan amount.</param>
        /// <param name="rate">The annual interest rate (in percent).</param>
        /// <param name="months">The total number of months for the loan term.</param>
        /// <returns>The monthly payment amount.</returns>
        public static double CalculateLoanPayment(double principal, double rate, int months)
        {
            if (rate <= 0) return principal / months;

            var monthlyRate = rate / 100.0 / 12.0;
            return (principal * monthlyRate * Math.Pow(1 + monthlyRate, months)) / (Math.Pow(1 + monthlyRate, months) - 1);
        }

        /// <summary>
        /// Calculates the Return on Investment (ROI).
        /// </summary>
        /// <param name="investment">The initial investment cost.</param>
        /// <param name="returnAmount">The total amount returned.</param>
        /// <returns>The ROI percentage.</returns>
        public static double CalculateRoi(double investment, double returnAmount)
        {
            if (investment == 0) return 0;
            return ((returnAmount - investment) / investment) * 100.0;
        }

        /// <summary>
        /// Calculates the future value of an investment with monthly contributions.
        /// </summary>
        /// <param name="principal">The initial starting amount.</param>
        /// <param name="monthlyContribution">The amount contributed each month.</param>
        /// <param name="rate">The annual return rate (in percent).</param>
        /// <param name="years">The duration of the investment in years.</param>
        /// <returns>The total future value.</returns>
        public static double CalculateInvestmentGrowth(double principal, double monthlyContribution, double rate, int years)
        {
            double r = rate / 100.0 / 12.0;
            double n = years * 12.0;

            if (r == 0)
                return principal + monthlyContribution * n;

            double futureValuePrincipal = principal * Math.Pow(1 + r, n);
            double futureValueSeries = monthlyContribution * ((Math.Pow(1 + r, n) - 1) / r);

            return futureValuePrincipal + futureValueSeries;
        }

        /// <summary>
        /// Calculates Annual Percentage Yield (APY) from Annual Percentage Rate (APR).
        /// </summary>
        /// <param name="nominalRate">The nominal annual rate (APR) in percent.</param>
        /// <param name="compoundingFrequency">The number of compounding periods per year.</param>
        /// <returns>A tuple containing the original APR and the calculated APY in percent.</returns>
        public static (double APR, double APY) CalculateApy(double nominalRate, int compoundingFrequency)
        {
            double r = nominalRate / 100.0;
            double n = compoundingFrequency;

            if (r <= 0 || n <= 0) return (0, 0);

            double apy = Math.Pow(1 + r / n, n) - 1;
            return (nominalRate, apy * 100);
        }

        /// <summary>
        /// Calculates a Certificate of Deposit (CD) ladder strategy.
        /// </summary>
        /// <param name="totalAmount">The total amount to invest.</param>
        /// <param name="years">The number of rungs (years) in the ladder.</param>
        /// <param name="startRate">The interest rate for the shortest term (in percent).</param>
        /// <param name="endRate">The interest rate for the longest term (in percent).</param>
        /// <returns>A list of CD ladder items describing each rung.</returns>
        public static List<CdLadderItem> CalculateCdLadder(double totalAmount, int years, double startRate, double endRate)
        {
            var list = new List<CdLadderItem>();
            if (years <= 0) return list;

            double amountPerCd = totalAmount / years;
            double rateIncrement = years > 1 ? (endRate - startRate) / (years - 1) : 0;

            for (int i = 0; i < years; i++)
            {
                double currentRate = startRate + (rateIncrement * i);
                double r = currentRate / 100.0;

                double maturity = amountPerCd * Math.Pow(1 + r, years);

                list.Add(new CdLadderItem
                {
                    Index = i + 1,
                    Amount = amountPerCd,
                    Rate = currentRate,
                    Maturity = maturity,
                    Term = years
                });
            }
            return list;
        }

        /// <summary>
        /// Calculates German mortgage metrics (Baufinanzierung) including monthly rate and remaining debt.
        /// </summary>
        /// <param name="principal">The loan amount (Darlehensbetrag).</param>
        /// <param name="interestRate">The nominal annual interest rate (Sollzins) in percent.</param>
        /// <param name="tilgungPercent">The initial amortization rate (Anfängliche Tilgung) in percent per year.</param>
        /// <param name="zinsbindungYears">The fixed interest period (Zinsbindungsfrist) in years.</param>
        /// <returns>A tuple containing monthly payment, remaining debt, and total interest paid.</returns>
        public static (double MonthlyPayment, double RemainingDebt, double TotalInterestPaid) CalculateGermanLoan(
            double principal, double interestRate, double tilgungPercent, int zinsbindungYears)
        {
            if (principal <= 0 || interestRate < 0 || tilgungPercent < 0)
                return (0, 0, 0);

            // In Germany: Monthly Payment = (Interest% + Tilgung%) * Principal / 12
            double annualPaymentPercent = interestRate + tilgungPercent;
            double monthlyPayment = principal * (annualPaymentPercent / 100.0) / 12.0;

            double balance = principal;
            double totalInterest = 0;
            double monthlyRate = interestRate / 100.0 / 12.0;

            int totalMonths = zinsbindungYears * 12;

            for (int i = 0; i < totalMonths; i++)
            {
                double interestPart = balance * monthlyRate;
                double tilgungPart = monthlyPayment - interestPart;

                totalInterest += interestPart;
                balance -= tilgungPart;

                // If paid off early
                if (balance <= 0)
                {
                    balance = 0;
                    break;
                }
            }

            return (monthlyPayment, balance, totalInterest);
        }
    }

    /// <summary>
    /// Represents a single item in a CD ladder strategy.
    /// </summary>
    public struct CdLadderItem
    {
        /// <summary>
        /// The index/sequence number of the CD in the ladder.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The principal amount invested in this CD.
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// The interest rate for this specific CD term.
        /// </summary>
        public double Rate { get; set; }

        /// <summary>
        /// The maturity value of this CD.
        /// </summary>
        public double Maturity { get; set; }

        /// <summary>
        /// The term length in years for this CD.
        /// </summary>
        public int Term { get; set; }
    }
}
