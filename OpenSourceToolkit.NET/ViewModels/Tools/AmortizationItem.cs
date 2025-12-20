using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    /// <summary>
    /// Represents a single row in a loan amortization or deposit growth schedule.
    /// Supports real-time binding updates for editable extra payment fields.
    /// </summary>
    public class ScheduleItem : ObservableObject
    {
        public int Number { get; set; }
        public DateTime Date { get; set; }

        private double _principal;
        public double Principal
        {
            get => _principal;
            set => SetProperty(ref _principal, value);
        }

        private double _interest;
        public double Interest
        {
            get => _interest;
            set => SetProperty(ref _interest, value);
        }

        public double Escrow { get; set; }

        private double _extra;
        public double Extra
        {
            get => _extra;
            set
            {
                if (SetProperty(ref _extra, value))
                {
                    OnPropertyChanged(nameof(Total));
                    ExtraChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public double Total => Principal + Interest + Escrow + Extra;

        private double _ahead;
        public double Ahead
        {
            get => _ahead;
            set => SetProperty(ref _ahead, value);
        }

        public double OrigBalance { get; set; }

        private double _balance;
        public double Balance
        {
            get => _balance;
            set => SetProperty(ref _balance, value);
        }

        public event EventHandler ExtraChanged;

        public void NotifyAllChanged()
        {
            OnPropertyChanged(nameof(Principal));
            OnPropertyChanged(nameof(Interest));
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(Ahead));
            OnPropertyChanged(nameof(Balance));
        }
    }

    /// <summary>
    /// Backward compatibility alias for AmortizationItem.
    /// </summary>
    public class AmortizationItem : ScheduleItem { }
}
