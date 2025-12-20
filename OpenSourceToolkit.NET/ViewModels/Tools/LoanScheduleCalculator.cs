using System;
using System.Collections.Generic;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    /// <summary>
    /// Reusable calculator for generating loan amortization and deposit growth schedules.
    /// </summary>
    public static class LoanScheduleCalculator
    {
        /// <summary>
        /// Calculates standard amortization schedule (US-style mortgage).
        /// </summary>
        public static LoanScheduleResult CalculateAmortization(
            double principal,
            double annualRate,
            int termMonths,
            double monthlyEscrow = 0,
            IReadOnlyList<double> extraPayments = null)
        {
            var schedule = new List<ScheduleItem>();
            double monthlyRate = annualRate / 100.0 / 12.0;

            double monthlyPI = monthlyRate > 0
                ? (principal * monthlyRate * Math.Pow(1 + monthlyRate, termMonths)) / (Math.Pow(1 + monthlyRate, termMonths) - 1)
                : principal / termMonths;

            // Calculate original totals (without extra payments)
            double origBalance = principal;
            double origTotalInterest = 0;
            for (int i = 0; i < termMonths && origBalance > 0.01; i++)
            {
                double interestPmt = origBalance * monthlyRate;
                origTotalInterest += interestPmt;
                origBalance -= (monthlyPI - interestPmt);
            }

            // Build schedule with extra payments
            double balance = principal;
            double cumExtraPaid = 0;
            double cumInterest = 0;
            double origBalanceTrack = principal;
            double origCumInterest = 0;
            DateTime paymentDate = DateTime.Today;
            int paymentNum = 0;

            while (balance > 0.01 && paymentNum < termMonths + 1200)
            {
                paymentNum++;
                paymentDate = paymentDate.AddMonths(1);

                double interestPmt = balance * monthlyRate;
                double scheduledPrincipal = Math.Min(monthlyPI - interestPmt, balance);

                double extra = 0;
                if (extraPayments != null && paymentNum - 1 < extraPayments.Count)
                    extra = extraPayments[paymentNum - 1];

                double totalPrincipal = Math.Min(scheduledPrincipal + extra, balance);
                double actualExtra = Math.Max(0, totalPrincipal - scheduledPrincipal);

                cumExtraPaid += actualExtra;
                cumInterest += interestPmt;

                double newBalance = balance - totalPrincipal;

                // Track original schedule for "Ahead" calculation
                double origInterestAtMonth = 0;
                double origPrincipalAtMonth = 0;
                if (paymentNum <= termMonths)
                {
                    origInterestAtMonth = origBalanceTrack * monthlyRate;
                    origPrincipalAtMonth = monthlyPI - origInterestAtMonth;
                    origCumInterest += origInterestAtMonth;
                    origBalanceTrack = Math.Max(0, origBalanceTrack - origPrincipalAtMonth);
                }

                double ahead = paymentNum <= termMonths
                    ? (origCumInterest - cumInterest) + (origBalanceTrack - newBalance)
                    : origTotalInterest - cumInterest + cumExtraPaid;

                schedule.Add(new ScheduleItem
                {
                    Number = paymentNum,
                    Date = paymentDate,
                    Principal = scheduledPrincipal,
                    Interest = interestPmt,
                    Escrow = monthlyEscrow,
                    Extra = actualExtra,
                    Ahead = ahead,
                    OrigBalance = paymentNum <= termMonths ? origBalanceTrack : 0,
                    Balance = Math.Max(0, newBalance)
                });

                balance = newBalance;
            }

            int monthsSaved = termMonths - schedule.Count;
            string timeSaved = monthsSaved > 0
                ? (monthsSaved / 12 > 0 ? $"{monthsSaved / 12}y {monthsSaved % 12}m" : $"{monthsSaved}m")
                : "0m";

            return new LoanScheduleResult
            {
                Schedule = schedule,
                MonthlyPayment = monthlyPI,
                TotalInterest = origTotalInterest,
                InterestSaved = origTotalInterest - cumInterest,
                ExtraPaid = cumExtraPaid,
                TimeSaved = timeSaved,
                OriginalPayoffDate = DateTime.Today.AddMonths(termMonths),
                NewPayoffDate = schedule.Count > 0 ? schedule[schedule.Count - 1].Date : DateTime.Today.AddMonths(termMonths)
            };
        }

        /// <summary>
        /// Calculates German Baufinanzierung schedule (annuity loan with Zinsbindung).
        /// </summary>
        public static LoanScheduleResult CalculateGermanLoan(
            double principal,
            double annualRate,
            double initialTilgungPercent,
            int zinsbindungYears,
            IReadOnlyList<double> sondertilgungen = null)
        {
            var schedule = new List<ScheduleItem>();
            double monthlyRate = annualRate / 100.0 / 12.0;

            // German: Monthly = (Zins% + Tilgung%) * Principal / 12
            double annualPaymentPercent = annualRate + initialTilgungPercent;
            double monthlyPayment = principal * (annualPaymentPercent / 100.0) / 12.0;

            int totalMonths = zinsbindungYears * 12;

            // Calculate original totals
            double origBalance = principal;
            double origTotalInterest = 0;
            for (int i = 0; i < totalMonths && origBalance > 0.01; i++)
            {
                double interestPmt = origBalance * monthlyRate;
                origTotalInterest += interestPmt;
                origBalance -= (monthlyPayment - interestPmt);
            }
            double origRestschuld = Math.Max(0, origBalance);

            // Build schedule with Sondertilgungen
            double balance = principal;
            double cumSondertilgung = 0;
            double cumInterest = 0;
            double origBalanceTrack = principal;
            double origCumInterest = 0;
            DateTime paymentDate = DateTime.Today;

            for (int i = 0; i < totalMonths && balance > 0.01; i++)
            {
                paymentDate = paymentDate.AddMonths(1);

                double interestPmt = balance * monthlyRate;
                double tilgungPmt = monthlyPayment - interestPmt;
                if (tilgungPmt > balance) tilgungPmt = balance;

                double sondertilgung = 0;
                if (sondertilgungen != null && i < sondertilgungen.Count)
                    sondertilgung = sondertilgungen[i];

                double totalTilgung = Math.Min(tilgungPmt + sondertilgung, balance);
                double actualSonder = Math.Max(0, totalTilgung - tilgungPmt);

                cumSondertilgung += actualSonder;
                cumInterest += interestPmt;

                double newBalance = balance - totalTilgung;

                // Track original for comparison
                double origInterestAtMonth = origBalanceTrack * monthlyRate;
                double origTilgungAtMonth = monthlyPayment - origInterestAtMonth;
                origCumInterest += origInterestAtMonth;
                origBalanceTrack = Math.Max(0, origBalanceTrack - origTilgungAtMonth);

                double ahead = (origCumInterest - cumInterest) + (origBalanceTrack - newBalance);

                schedule.Add(new ScheduleItem
                {
                    Number = i + 1,
                    Date = paymentDate,
                    Principal = tilgungPmt,
                    Interest = interestPmt,
                    Extra = actualSonder,
                    Ahead = ahead,
                    OrigBalance = origBalanceTrack,
                    Balance = Math.Max(0, newBalance)
                });

                balance = newBalance;
            }

            double restschuld = schedule.Count > 0 ? schedule[schedule.Count - 1].Balance : principal;

            return new LoanScheduleResult
            {
                Schedule = schedule,
                MonthlyPayment = monthlyPayment,
                TotalInterest = origTotalInterest,
                InterestSaved = origTotalInterest - cumInterest,
                ExtraPaid = cumSondertilgung,
                RemainingBalance = restschuld,
                OriginalRemainingBalance = origRestschuld,
                TimeSaved = restschuld < origRestschuld ? "Early payoff possible" : "",
                OriginalPayoffDate = DateTime.Today.AddMonths(totalMonths),
                NewPayoffDate = schedule.Count > 0 ? schedule[schedule.Count - 1].Date : DateTime.Today.AddMonths(totalMonths)
            };
        }

        /// <summary>
        /// Calculates German Festgeld (fixed deposit) growth schedule.
        /// </summary>
        public static DepositScheduleResult CalculateFestgeld(
            double principal,
            double annualRate,
            int termMonths)
        {
            var schedule = new List<ScheduleItem>();
            double monthlyRate = annualRate / 100.0 / 12.0;

            double balance = principal;
            double totalInterest = 0;
            DateTime paymentDate = DateTime.Today;

            for (int i = 0; i < termMonths; i++)
            {
                paymentDate = paymentDate.AddMonths(1);

                double interestEarned = balance * monthlyRate;
                totalInterest += interestEarned;
                balance += interestEarned;

                schedule.Add(new ScheduleItem
                {
                    Number = i + 1,
                    Date = paymentDate,
                    Interest = interestEarned,
                    Balance = balance,
                    Ahead = totalInterest
                });
            }

            return new DepositScheduleResult
            {
                Schedule = schedule,
                FinalBalance = balance,
                TotalInterest = totalInterest,
                MaturityDate = paymentDate
            };
        }
    }

    /// <summary>
    /// Result of loan amortization calculation.
    /// </summary>
    public class LoanScheduleResult
    {
        public List<ScheduleItem> Schedule { get; set; }
        public double MonthlyPayment { get; set; }
        public double TotalInterest { get; set; }
        public double InterestSaved { get; set; }
        public double ExtraPaid { get; set; }
        public double RemainingBalance { get; set; }
        public double OriginalRemainingBalance { get; set; }
        public string TimeSaved { get; set; }
        public DateTime OriginalPayoffDate { get; set; }
        public DateTime NewPayoffDate { get; set; }
    }

    /// <summary>
    /// Result of deposit growth calculation.
    /// </summary>
    public class DepositScheduleResult
    {
        public List<ScheduleItem> Schedule { get; set; }
        public double FinalBalance { get; set; }
        public double TotalInterest { get; set; }
        public DateTime MaturityDate { get; set; }
    }
}
