using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.Calculators;
using OpenSourceToolkit.NET.Localization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public class FinancialCalculatorToolViewModel : ToolViewModel
    {
        public override int Id => 21;
        public override string Name => ToolkitLocalization.GetString("Tool_FinancialCalculator_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_FinancialCalculator_Description");
        public override string IconKey => "FinancialCalculatorIcon";

        // Compound Interest
        private double _principal = 1000;
        public double Principal
        {
            get => _principal;
            set => SetProperty(ref _principal, value);
        }

        private double _rate = 5;
        public double Rate
        {
            get => _rate;
            set => SetProperty(ref _rate, value);
        }

        private double _years = 10;
        public double Years
        {
            get => _years;
            set => SetProperty(ref _years, value);
        }

        private string _interestResult;
        public string InterestResult
        {
            get => _interestResult;
            set => SetProperty(ref _interestResult, value);
        }

        // Loan Payment
        private double _loanAmount = 250000;
        public double LoanAmount
        {
            get => _loanAmount;
            set => SetProperty(ref _loanAmount, value);
        }

        private double _loanRate = 4.5;
        public double LoanRate
        {
            get => _loanRate;
            set => SetProperty(ref _loanRate, value);
        }

        private double _loanMonths = 360;
        public double LoanMonths
        {
            get => _loanMonths;
            set => SetProperty(ref _loanMonths, value);
        }

        private string _loanResult;
        public string LoanResult
        {
            get => _loanResult;
            set => SetProperty(ref _loanResult, value);
        }

        // ROI
        private double _investment = 1000;
        public double Investment
        {
            get => _investment;
            set => SetProperty(ref _investment, value);
        }

        private double _returnAmount = 1500;
        public double ReturnAmount
        {
            get => _returnAmount;
            set => SetProperty(ref _returnAmount, value);
        }

        private string _roiResult;
        public string RoiResult
        {
            get => _roiResult;
            set => SetProperty(ref _roiResult, value);
        }

        // Investment Growth
        private double _invInitial = 5000;
        public double InvInitial { get => _invInitial; set => SetProperty(ref _invInitial, value); }

        private double _invMonthly = 500;
        public double InvMonthly { get => _invMonthly; set => SetProperty(ref _invMonthly, value); }

        private double _invRate = 7;
        public double InvRate { get => _invRate; set => SetProperty(ref _invRate, value); }

        private double _invYears = 20;
        public double InvYears { get => _invYears; set => SetProperty(ref _invYears, value); }

        private string _invResult;
        public string InvResult { get => _invResult; set => SetProperty(ref _invResult, value); }

        // Fixed Deposit
        private double _fdAmount = 50000;
        public double FdAmount { get => _fdAmount; set => SetProperty(ref _fdAmount, value); }

        private double _fdRate = 6.5;
        public double FdRate { get => _fdRate; set => SetProperty(ref _fdRate, value); }

        private double _fdTenure = 12;
        public double FdTenure { get => _fdTenure; set => SetProperty(ref _fdTenure, value); }

        // 0: Months, 1: Years
        private int _fdTenureType = 0;
        public int FdTenureType { get => _fdTenureType; set => SetProperty(ref _fdTenureType, value); }

        // 1, 2, 4, 12, 365
        private int _fdCompounding = 4; // Quarterly
        public int FdCompounding { get => _fdCompounding; set => SetProperty(ref _fdCompounding, value); }

        private int _fdCompoundingIndex = 2;
        public int FdCompoundingIndex
        {
            get => _fdCompoundingIndex;
            set
            {
                if (SetProperty(ref _fdCompoundingIndex, value))
                {
                    switch (value)
                    {
                        case 0: FdCompounding = 1; break;
                        case 1: FdCompounding = 2; break;
                        case 2: FdCompounding = 4; break;
                        case 3: FdCompounding = 12; break;
                        case 4: FdCompounding = 365; break;
                    }
                }
            }
        }

        private string _fdResult;
        public string FdResult { get => _fdResult; set => SetProperty(ref _fdResult, value); }

        // CD Ladder
        private double _cdTotal = 100000;
        public double CdTotal { get => _cdTotal; set => SetProperty(ref _cdTotal, value); }

        private double _cdYears = 5;
        public double CdYears { get => _cdYears; set => SetProperty(ref _cdYears, value); }

        private double _cdStart = 4;
        public double CdStart { get => _cdStart; set => SetProperty(ref _cdStart, value); }

        private double _cdEnd = 6;
        public double CdEnd { get => _cdEnd; set => SetProperty(ref _cdEnd, value); }

        private List<CdLadderItem> _cdResultList;
        public List<CdLadderItem> CdResultList { get => _cdResultList; set => SetProperty(ref _cdResultList, value); }

        private string _cdSummary;
        public string CdSummary { get => _cdSummary; set => SetProperty(ref _cdSummary, value); }

        // APY
        private double _apyNominal = 5;
        public double ApyNominal { get => _apyNominal; set => SetProperty(ref _apyNominal, value); }

        private int _apyFreq = 12;
        public int ApyFreq { get => _apyFreq; set => SetProperty(ref _apyFreq, value); }

        private int _apyFreqIndex = 3;
        public int ApyFreqIndex
        {
            get => _apyFreqIndex;
            set
            {
                if (SetProperty(ref _apyFreqIndex, value))
                {
                    switch (value)
                    {
                        case 0: ApyFreq = 1; break;
                        case 1: ApyFreq = 2; break;
                        case 2: ApyFreq = 4; break;
                        case 3: ApyFreq = 12; break;
                        case 4: ApyFreq = 365; break;
                    }
                }
            }
        }

        private string _apyResult;
        public string ApyResult { get => _apyResult; set => SetProperty(ref _apyResult, value); }

        // German Baufinanzierung (DE)
        private double _deLoanAmount = 300000;
        public double DeLoanAmount { get => _deLoanAmount; set => SetProperty(ref _deLoanAmount, value); }

        private double _deInterestRate = 3.5;
        public double DeInterestRate { get => _deInterestRate; set => SetProperty(ref _deInterestRate, value); }

        private double _deTilgung = 2.0;
        public double DeTilgung { get => _deTilgung; set => SetProperty(ref _deTilgung, value); }

        private double _deZinsbindung = 10;
        public double DeZinsbindung { get => _deZinsbindung; set => SetProperty(ref _deZinsbindung, value); }

        private string _deLoanResult;
        public string DeLoanResult { get => _deLoanResult; set => SetProperty(ref _deLoanResult, value); }

        // German Baufinanzierung Schedule
        private double _deSondertilgung;
        public double DeSondertilgung { get => _deSondertilgung; set => SetProperty(ref _deSondertilgung, value); }

        private double _deMonthlyPayment;
        public double DeMonthlyPayment { get => _deMonthlyPayment; private set => SetProperty(ref _deMonthlyPayment, value); }

        private double _deRestschuld;
        public double DeRestschuld { get => _deRestschuld; private set => SetProperty(ref _deRestschuld, value); }

        private double _deOrigRestschuld;
        public double DeOrigRestschuld { get => _deOrigRestschuld; private set => SetProperty(ref _deOrigRestschuld, value); }

        private double _deTotalInterest;
        public double DeTotalInterest { get => _deTotalInterest; private set => SetProperty(ref _deTotalInterest, value); }

        private double _deInterestSaved;
        public double DeInterestSaved { get => _deInterestSaved; private set => SetProperty(ref _deInterestSaved, value); }

        private double _deSondertilgungPaid;
        public double DeSondertilgungPaid { get => _deSondertilgungPaid; private set => SetProperty(ref _deSondertilgungPaid, value); }

        private ObservableCollection<ScheduleItem> _deLoanSchedule = new ObservableCollection<ScheduleItem>();
        public ObservableCollection<ScheduleItem> DeLoanSchedule
        {
            get => _deLoanSchedule;
            private set => SetProperty(ref _deLoanSchedule, value);
        }

        // German Festgeld (DE)
        private double _deFdAmount = 25000;
        public double DeFdAmount { get => _deFdAmount; set => SetProperty(ref _deFdAmount, value); }

        private double _deFdRate = 3.0;
        public double DeFdRate { get => _deFdRate; set => SetProperty(ref _deFdRate, value); }

        private double _deFdMonths = 12;
        public double DeFdMonths { get => _deFdMonths; set => SetProperty(ref _deFdMonths, value); }

        private string _deFdResult;
        public string DeFdResult { get => _deFdResult; set => SetProperty(ref _deFdResult, value); }

        // German Festgeld Schedule
        private double _deFdFinalBalance;
        public double DeFdFinalBalance { get => _deFdFinalBalance; private set => SetProperty(ref _deFdFinalBalance, value); }

        private double _deFdTotalInterest;
        public double DeFdTotalInterest { get => _deFdTotalInterest; private set => SetProperty(ref _deFdTotalInterest, value); }

        private DateTime _deFdMaturityDate;
        public DateTime DeFdMaturityDate { get => _deFdMaturityDate; private set => SetProperty(ref _deFdMaturityDate, value); }

        private ObservableCollection<ScheduleItem> _deFdSchedule = new ObservableCollection<ScheduleItem>();
        public ObservableCollection<ScheduleItem> DeFdSchedule
        {
            get => _deFdSchedule;
            private set => SetProperty(ref _deFdSchedule, value);
        }

        // Mortgage Amortization
        private double _mortHomePrice = 400000;
        public double MortHomePrice { get => _mortHomePrice; set { if (SetProperty(ref _mortHomePrice, value)) RecalculateMortgage(); } }

        private double _mortDownPayment = 80000;
        public double MortDownPayment { get => _mortDownPayment; set { if (SetProperty(ref _mortDownPayment, value)) RecalculateMortgage(); } }

        private double _mortInterestRate = 6.5;
        public double MortInterestRate { get => _mortInterestRate; set { if (SetProperty(ref _mortInterestRate, value)) RecalculateMortgage(); } }

        private int _mortTermIndex = 2; // 0=15yr, 1=20yr, 2=30yr
        public int MortTermIndex
        {
            get => _mortTermIndex;
            set
            {
                if (SetProperty(ref _mortTermIndex, value))
                {
                    switch (value)
                    {
                        case 0: MortTermYears = 15; break;
                        case 1: MortTermYears = 20; break;
                        default: MortTermYears = 30; break;
                    }
                }
            }
        }

        private int _mortTermYears = 30;
        public int MortTermYears { get => _mortTermYears; set { if (SetProperty(ref _mortTermYears, value)) RecalculateMortgage(); } }

        private double _mortAnnualTax = 4800;
        public double MortAnnualTax { get => _mortAnnualTax; set { if (SetProperty(ref _mortAnnualTax, value)) RecalculateMortgage(); } }

        private double _mortAnnualInsurance = 1800;
        public double MortAnnualInsurance { get => _mortAnnualInsurance; set { if (SetProperty(ref _mortAnnualInsurance, value)) RecalculateMortgage(); } }

        // Computed summaries
        public double MortLoanAmount => MortHomePrice - MortDownPayment;
        public double MortMonthlyPI { get => _mortMonthlyPI; private set => SetProperty(ref _mortMonthlyPI, value); }
        private double _mortMonthlyPI;

        public double MortMonthlyEscrow => (MortAnnualTax + MortAnnualInsurance) / 12.0;
        public double MortMonthlyPITI => MortMonthlyPI + MortMonthlyEscrow;

        private double _mortTotalInterest;
        public double MortTotalInterest { get => _mortTotalInterest; private set => SetProperty(ref _mortTotalInterest, value); }

        private DateTime _mortPayoffDate;
        public DateTime MortPayoffDate { get => _mortPayoffDate; private set => SetProperty(ref _mortPayoffDate, value); }

        // Extra payment
        private double _mortMonthlyExtra;
        public double MortMonthlyExtra { get => _mortMonthlyExtra; set => SetProperty(ref _mortMonthlyExtra, value); }

        // Savings summary
        private double _mortInterestSaved;
        public double MortInterestSaved { get => _mortInterestSaved; private set => SetProperty(ref _mortInterestSaved, value); }

        private string _mortTimeSaved;
        public string MortTimeSaved { get => _mortTimeSaved; private set => SetProperty(ref _mortTimeSaved, value); }

        private DateTime _mortNewPayoffDate;
        public DateTime MortNewPayoffDate { get => _mortNewPayoffDate; private set => SetProperty(ref _mortNewPayoffDate, value); }

        private double _mortExtraPaid;
        public double MortExtraPaid { get => _mortExtraPaid; private set => SetProperty(ref _mortExtraPaid, value); }

        private ObservableCollection<AmortizationItem> _amortizationSchedule = new ObservableCollection<AmortizationItem>();
        public ObservableCollection<AmortizationItem> AmortizationSchedule
        {
            get => _amortizationSchedule;
            private set => SetProperty(ref _amortizationSchedule, value);
        }


        public ICommand CalculateInterestCommand { get; }
        public ICommand CalculateLoanCommand { get; }
        public ICommand CalculateRoiCommand { get; }
        public ICommand CalculateInvestmentCommand { get; }
        public ICommand CalculateFdCommand { get; }
        public ICommand CalculateCdCommand { get; }
        public ICommand CalculateApyCommand { get; }
        public ICommand CalculateDeLoanCommand { get; }
        public ICommand DeApplyAllSondertilgungCommand { get; }
        public ICommand DeClearSondertilgungCommand { get; }
        public ICommand CalculateDeFdCommand { get; }
        public ICommand CalculateMortgageCommand { get; }
        public ICommand MortApplyAllExtraCommand { get; }
        public ICommand MortClearExtraCommand { get; }
        public ICommand MortResetCommand { get; }

        public FinancialCalculatorToolViewModel()
        {
            CalculateInterestCommand = new RelayCommand(CalculateInterest);
            CalculateLoanCommand = new RelayCommand(CalculateLoan);
            CalculateRoiCommand = new RelayCommand(CalculateRoi);
            CalculateInvestmentCommand = new RelayCommand(CalculateInvestment);
            CalculateFdCommand = new RelayCommand(CalculateFd);
            CalculateCdCommand = new RelayCommand(CalculateCd);
            CalculateApyCommand = new RelayCommand(CalculateApy);
            CalculateDeLoanCommand = new RelayCommand(CalculateDeLoan);
            DeApplyAllSondertilgungCommand = new RelayCommand(ApplyAllSondertilgung);
            DeClearSondertilgungCommand = new RelayCommand(ClearAllSondertilgung);
            CalculateDeFdCommand = new RelayCommand(CalculateDeFd);
            CalculateMortgageCommand = new RelayCommand(RecalculateMortgage);
            MortApplyAllExtraCommand = new RelayCommand(ApplyAllExtra);
            MortClearExtraCommand = new RelayCommand(ClearAllExtra);
            MortResetCommand = new RelayCommand(ResetMortgage);

            RecalculateMortgage();
            CalculateDeLoan();
            CalculateDeFd();
        }

        private void CalculateInterest()
        {
            double result = FinancialCalculator.CalculateCompoundInterest(Principal, Rate, 12, (int)Years);
            InterestResult = $"Future Value: {result:C2}";
        }

        private void CalculateLoan()
        {
            double payment = FinancialCalculator.CalculateLoanPayment(LoanAmount, LoanRate, (int)LoanMonths);
            LoanResult = $"Monthly Payment: {payment:C2}";
        }

        private void CalculateRoi()
        {
            double roi = FinancialCalculator.CalculateRoi(Investment, ReturnAmount);
            RoiResult = $"ROI: {roi:F2}%";
        }

        private void CalculateInvestment()
        {
            double result = FinancialCalculator.CalculateInvestmentGrowth(InvInitial, InvMonthly, InvRate, (int)InvYears);
            double totalInvested = InvInitial + (InvMonthly * InvYears * 12);
            double interest = result - totalInvested;
            InvResult = $"Future Value: {result:C2}\nInterest Earned: {interest:C2}";
        }

        private void CalculateFd()
        {
            // Handle tenure type: if months, convert to years for the formula?
            // Wait, formula takes 'years'.
            double t = FdTenureType == 0 ? FdTenure / 12.0 : FdTenure;

            // Reuse CompoundInterest logic but with specific compounding
            // A = P(1 + r/n)^(nt)
            // CalculateCompoundInterest: principal * Pow(1 + (rate/100/times), times * years)

            double maturity = FinancialCalculator.CalculateCompoundInterest(FdAmount, FdRate, FdCompounding, (int)t);
            // Note: existing CalculateCompoundInterest takes int years. We might need double years.
            // Let's check FinancialCalculator.cs again.
            // public static double CalculateCompoundInterest(double principal, double rate, int timesPerYear, int years)
            // It takes int years. Fixed deposits can be 6 months (0.5 years).
            // I should update FinancialCalculator.cs to take double years.
            // Or implement the math here.
            // Let's update FinancialCalculator.cs to take double years. It's a quick fix.

            // Since I can't easily jump back and edit the previous file in the middle of writing this one without finishing this one...
            // I will invoke the math directly here or assume I fix it.
            // Actually, I can fix it in the next step. For now I'll cast to int which is BUGGY for months.
            // But wait, I can use the general formula:
            double r = FdRate / 100.0;
            double n = FdCompounding;
            double finalAmount = FdAmount * System.Math.Pow(1 + r / n, n * t);

            FdResult = $"Maturity Amount: {finalAmount:C2}\nInterest: {(finalAmount - FdAmount):C2}";
        }

        private void CalculateCd()
        {
            var list = FinancialCalculator.CalculateCdLadder(CdTotal, (int)CdYears, CdStart, CdEnd);
            CdResultList = list;
            double totalMaturity = list.Sum(x => x.Maturity);
            CdSummary = $"Total Maturity: {totalMaturity:C2}\nTotal Interest: {(totalMaturity - CdTotal):C2}";
        }

        private void CalculateApy()
        {
            var (apr, apy) = FinancialCalculator.CalculateApy(ApyNominal, ApyFreq);
            ApyResult = $"APR: {apr:F2}%\nAPY: {apy:F2}%";
        }

        private void CalculateDeLoan()
        {
            if (DeLoanAmount <= 0 || DeInterestRate < 0 || DeTilgung <= 0 || DeZinsbindung <= 0)
                return;

            // Get existing Sondertilgungen from current schedule
            var sondertilgungen = new List<double>();
            if (_deLoanSchedule != null)
            {
                foreach (var item in _deLoanSchedule)
                    sondertilgungen.Add(item.Extra);
            }

            var result = LoanScheduleCalculator.CalculateGermanLoan(
                DeLoanAmount, DeInterestRate, DeTilgung, (int)DeZinsbindung,
                sondertilgungen.Count > 0 ? sondertilgungen : null);

            DeMonthlyPayment = result.MonthlyPayment;
            DeRestschuld = result.RemainingBalance;
            DeOrigRestschuld = result.OriginalRemainingBalance;
            DeTotalInterest = result.TotalInterest;
            DeInterestSaved = result.InterestSaved;
            DeSondertilgungPaid = result.ExtraPaid;

            DeLoanResult = $"Monatliche Rate: {result.MonthlyPayment:C2}\n" +
                           $"Restschuld nach {DeZinsbindung} Jahren: {result.RemainingBalance:C2}\n" +
                           $"Gezahlte Zinsen: {result.TotalInterest - result.InterestSaved:C2}";

            // Update schedule
            var schedule = new ObservableCollection<ScheduleItem>();
            foreach (var item in result.Schedule)
            {
                var scheduleItem = new ScheduleItem
                {
                    Number = item.Number,
                    Date = item.Date,
                    Principal = item.Principal,
                    Interest = item.Interest,
                    Extra = item.Extra,
                    Ahead = item.Ahead,
                    OrigBalance = item.OrigBalance,
                    Balance = item.Balance
                };
                scheduleItem.ExtraChanged += (s, e) => RecalculateDeLoanFromSchedule();
                schedule.Add(scheduleItem);
            }
            DeLoanSchedule = schedule;
        }

        private void RecalculateDeLoanFromSchedule()
        {
            if (_deLoanSchedule == null || _deLoanSchedule.Count == 0)
                return;

            var sondertilgungen = new List<double>();
            foreach (var item in _deLoanSchedule)
                sondertilgungen.Add(item.Extra);

            var result = LoanScheduleCalculator.CalculateGermanLoan(
                DeLoanAmount, DeInterestRate, DeTilgung, (int)DeZinsbindung, sondertilgungen);

            DeRestschuld = result.RemainingBalance;
            DeInterestSaved = result.InterestSaved;
            DeSondertilgungPaid = result.ExtraPaid;

            // Update existing items in place
            for (int i = 0; i < result.Schedule.Count && i < _deLoanSchedule.Count; i++)
            {
                var src = result.Schedule[i];
                var dest = _deLoanSchedule[i];
                dest.Principal = src.Principal;
                dest.Interest = src.Interest;
                dest.Ahead = src.Ahead;
                dest.Balance = src.Balance;
                dest.NotifyAllChanged();
            }
        }

        private void ApplyAllSondertilgung()
        {
            if (_deLoanSchedule == null || DeSondertilgung <= 0)
                return;

            foreach (var item in _deLoanSchedule)
                item.Extra = DeSondertilgung;

            RecalculateDeLoanFromSchedule();
        }

        private void ClearAllSondertilgung()
        {
            if (_deLoanSchedule == null)
                return;

            foreach (var item in _deLoanSchedule)
                item.Extra = 0;

            RecalculateDeLoanFromSchedule();
        }

        private void CalculateDeFd()
        {
            if (DeFdAmount <= 0 || DeFdRate < 0 || DeFdMonths <= 0)
                return;

            var result = LoanScheduleCalculator.CalculateFestgeld(DeFdAmount, DeFdRate, (int)DeFdMonths);

            DeFdFinalBalance = result.FinalBalance;
            DeFdTotalInterest = result.TotalInterest;
            DeFdMaturityDate = result.MaturityDate;

            DeFdResult = $"Endkapital: {result.FinalBalance:C2}\nZinsertrag: {result.TotalInterest:C2}";

            // Update schedule
            var schedule = new ObservableCollection<ScheduleItem>();
            foreach (var item in result.Schedule)
            {
                schedule.Add(new ScheduleItem
                {
                    Number = item.Number,
                    Date = item.Date,
                    Interest = item.Interest,
                    Balance = item.Balance,
                    Ahead = item.Ahead
                });
            }
            DeFdSchedule = schedule;
        }

        private void RecalculateMortgage()
        {
            double loanAmount = MortLoanAmount;
            if (loanAmount <= 0 || MortInterestRate < 0 || MortTermYears <= 0)
                return;

            int totalMonths = MortTermYears * 12;
            double monthlyRate = MortInterestRate / 100.0 / 12.0;
            double monthlyEscrow = MortMonthlyEscrow;

            double monthlyPI = monthlyRate > 0
                ? (loanAmount * monthlyRate * Math.Pow(1 + monthlyRate, totalMonths)) / (Math.Pow(1 + monthlyRate, totalMonths) - 1)
                : loanAmount / totalMonths;

            MortMonthlyPI = monthlyPI;
            OnPropertyChanged(nameof(MortLoanAmount));
            OnPropertyChanged(nameof(MortMonthlyEscrow));
            OnPropertyChanged(nameof(MortMonthlyPITI));

            // Build original schedule (no extra payments) to compare
            double origBalance = loanAmount;
            double origTotalInterest = 0;
            for (int i = 0; i < totalMonths && origBalance > 0; i++)
            {
                double interestPmt = origBalance * monthlyRate;
                origTotalInterest += interestPmt;
                double principalPmt = monthlyPI - interestPmt;
                origBalance -= principalPmt;
            }

            MortTotalInterest = origTotalInterest;
            MortPayoffDate = DateTime.Today.AddMonths(totalMonths);

            // Build schedule with extra payments
            var schedule = new ObservableCollection<AmortizationItem>();
            double balance = loanAmount;
            double cumExtraPaid = 0;
            double cumInterestWithExtra = 0;
            DateTime paymentDate = DateTime.Today;
            int paymentNum = 0;

            // Track original cumulative interest and balance for "Ahead" calculation
            double origBalanceTrack = loanAmount;
            double origCumInterest = 0;

            while (balance > 0.01 && paymentNum < totalMonths + 1200)
            {
                paymentNum++;
                paymentDate = paymentDate.AddMonths(1);

                double interestPmt = balance * monthlyRate;
                double scheduledPrincipal = Math.Min(monthlyPI - interestPmt, balance);

                // Get extra from existing schedule item if available
                double extra = 0;
                if (_amortizationSchedule != null && paymentNum - 1 < _amortizationSchedule.Count)
                {
                    extra = _amortizationSchedule[paymentNum - 1].Extra;
                }

                double totalPrincipal = scheduledPrincipal + extra;
                if (totalPrincipal > balance)
                    totalPrincipal = balance;

                double actualExtra = totalPrincipal - scheduledPrincipal;
                if (actualExtra < 0) actualExtra = 0;

                cumExtraPaid += actualExtra;
                cumInterestWithExtra += interestPmt;

                double newBalance = balance - totalPrincipal;

                // Calculate "Ahead" - how much interest saved vs original schedule
                if (paymentNum <= totalMonths)
                {
                    double origInterestAtMonth = origBalanceTrack * monthlyRate;
                    double origPrincipalAtMonth = monthlyPI - origInterestAtMonth;
                    origCumInterest += origInterestAtMonth;
                    origBalanceTrack = Math.Max(0, origBalanceTrack - origPrincipalAtMonth);
                }

                double ahead = (origCumInterest - cumInterestWithExtra) + (origBalanceTrack - newBalance);
                if (paymentNum > totalMonths)
                    ahead = origTotalInterest - cumInterestWithExtra + cumExtraPaid;

                var item = new AmortizationItem
                {
                    Number = paymentNum,
                    Date = paymentDate,
                    Principal = scheduledPrincipal,
                    Interest = interestPmt,
                    Escrow = monthlyEscrow,
                    Extra = actualExtra,
                    Ahead = ahead,
                    OrigBalance = paymentNum <= totalMonths ? origBalanceTrack + origPrincipalAtThisPoint(paymentNum, loanAmount, monthlyPI, monthlyRate) : 0,
                    Balance = Math.Max(0, newBalance)
                };

                item.ExtraChanged += (s, e) => RecalculateFromSchedule();
                schedule.Add(item);

                balance = newBalance;
            }

            AmortizationSchedule = schedule;

            // Calculate savings
            MortInterestSaved = origTotalInterest - cumInterestWithExtra;
            MortExtraPaid = cumExtraPaid;
            MortNewPayoffDate = schedule.Count > 0 ? schedule[schedule.Count - 1].Date : MortPayoffDate;

            int monthsSaved = totalMonths - schedule.Count;
            if (monthsSaved > 0)
            {
                int yearsSaved = monthsSaved / 12;
                int remainingMonths = monthsSaved % 12;
                MortTimeSaved = yearsSaved > 0 ? $"{yearsSaved}y {remainingMonths}m" : $"{remainingMonths}m";
            }
            else
            {
                MortTimeSaved = "0m";
            }
        }

        private double origPrincipalAtThisPoint(int month, double principal, double pmt, double rate)
        {
            double balance = principal;
            for (int i = 0; i < month - 1 && balance > 0; i++)
            {
                double interest = balance * rate;
                balance -= (pmt - interest);
            }
            return Math.Max(0, balance);
        }

        private void RecalculateFromSchedule()
        {
            if (_amortizationSchedule == null || _amortizationSchedule.Count == 0)
                return;

            double loanAmount = MortLoanAmount;
            double monthlyRate = MortInterestRate / 100.0 / 12.0;
            double monthlyPI = MortMonthlyPI;
            double monthlyEscrow = MortMonthlyEscrow;
            int totalMonths = MortTermYears * 12;

            double balance = loanAmount;
            double cumExtraPaid = 0;
            double cumInterestWithExtra = 0;

            double origBalanceTrack = loanAmount;
            double origCumInterest = 0;
            double origTotalInterest = MortTotalInterest;

            var schedule = _amortizationSchedule;
            int lastValidIndex = 0;

            for (int i = 0; i < schedule.Count && balance > 0.01; i++)
            {
                var item = schedule[i];
                double interestPmt = balance * monthlyRate;
                double scheduledPrincipal = Math.Min(monthlyPI - interestPmt, balance);
                double extra = item.Extra;

                double totalPrincipal = scheduledPrincipal + extra;
                if (totalPrincipal > balance)
                    totalPrincipal = balance;

                double actualExtra = totalPrincipal - scheduledPrincipal;
                if (actualExtra < 0) actualExtra = 0;

                cumExtraPaid += actualExtra;
                cumInterestWithExtra += interestPmt;

                double newBalance = balance - totalPrincipal;

                if (i + 1 <= totalMonths)
                {
                    double origInterestAtMonth = origBalanceTrack * monthlyRate;
                    double origPrincipalAtMonth = monthlyPI - origInterestAtMonth;
                    origCumInterest += origInterestAtMonth;
                    origBalanceTrack = Math.Max(0, origBalanceTrack - origPrincipalAtMonth);
                }

                double ahead = (origCumInterest - cumInterestWithExtra) + (origBalanceTrack - newBalance);
                if (i + 1 > totalMonths)
                    ahead = origTotalInterest - cumInterestWithExtra + cumExtraPaid;

                item.Principal = scheduledPrincipal;
                item.Interest = interestPmt;
                item.Ahead = ahead;
                item.Balance = Math.Max(0, newBalance);
                item.NotifyAllChanged();

                balance = newBalance;
                lastValidIndex = i;
            }

            // Remove any extra rows beyond payoff
            while (schedule.Count > lastValidIndex + 1)
            {
                schedule.RemoveAt(schedule.Count - 1);
            }

            MortInterestSaved = origTotalInterest - cumInterestWithExtra;
            MortExtraPaid = cumExtraPaid;
            MortNewPayoffDate = schedule.Count > 0 ? schedule[schedule.Count - 1].Date : MortPayoffDate;

            int monthsSaved = totalMonths - schedule.Count;
            if (monthsSaved > 0)
            {
                int yearsSaved = monthsSaved / 12;
                int remainingMonths = monthsSaved % 12;
                MortTimeSaved = yearsSaved > 0 ? $"{yearsSaved}y {remainingMonths}m" : $"{remainingMonths}m";
            }
            else
            {
                MortTimeSaved = "0m";
            }
        }

        private void ApplyAllExtra()
        {
            if (_amortizationSchedule == null || MortMonthlyExtra <= 0)
                return;

            foreach (var item in _amortizationSchedule)
            {
                item.Extra = MortMonthlyExtra;
            }
            RecalculateFromSchedule();
        }

        private void ClearAllExtra()
        {
            if (_amortizationSchedule == null)
                return;

            foreach (var item in _amortizationSchedule)
            {
                item.Extra = 0;
            }
            RecalculateFromSchedule();
        }

        private void ResetMortgage()
        {
            MortHomePrice = 400000;
            MortDownPayment = 80000;
            MortInterestRate = 6.5;
            MortTermIndex = 2;
            MortAnnualTax = 4800;
            MortAnnualInsurance = 1800;
            MortMonthlyExtra = 0;
            RecalculateMortgage();
        }
    }
}
