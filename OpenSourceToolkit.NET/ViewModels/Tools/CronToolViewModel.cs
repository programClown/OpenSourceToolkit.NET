using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenSourceToolkit.NET.Localization;
using OpenSourceToolkit.Scheduling;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public partial class CronToolViewModel : ToolViewModel
    {
        public override int Id => 16;
        public override string Name => ToolkitLocalization.GetString("Tool_Cron_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_Cron_Description");
        public override string IconKey => "CronIcon";

        private bool _isUpdating;

        private string _expression = "* * * * *";
        public string Expression
        {
            get => _expression;
            set
            {
                if (SetProperty(ref _expression, value))
                {
                    if (!_isUpdating)
                    {
                        UpdateManualFromCron(value);
                    }
                    UpdateDescription();
                    Parse();
                }
            }
        }

        private string _cronDescription;
        public string CronDescription
        {
            get => _cronDescription;
            set => SetProperty(ref _cronDescription, value);
        }

        private string _error;
        public string Error
        {
            get => _error;
            set => SetProperty(ref _error, value);
        }

        public ObservableCollection<string> NextOccurrences { get; } = new ObservableCollection<string>();

        // Manual Builder Properties
        private string _selectedMinute = "*";
        public string SelectedMinute
        {
            get => _selectedMinute;
            set
            {
                if (SetProperty(ref _selectedMinute, value) && !_isUpdating)
                    UpdateCronFromManual();
            }
        }

        private string _selectedHour = "*";
        public string SelectedHour
        {
            get => _selectedHour;
            set
            {
                if (SetProperty(ref _selectedHour, value) && !_isUpdating)
                    UpdateCronFromManual();
            }
        }

        private string _selectedDay = "*";
        public string SelectedDay
        {
            get => _selectedDay;
            set
            {
                if (SetProperty(ref _selectedDay, value) && !_isUpdating)
                    UpdateCronFromManual();
            }
        }

        private string _selectedMonth = "*";
        public string SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                if (SetProperty(ref _selectedMonth, value) && !_isUpdating)
                    UpdateCronFromManual();
            }
        }

        private string _selectedWeekday = "*";
        public string SelectedWeekday
        {
            get => _selectedWeekday;
            set
            {
                if (SetProperty(ref _selectedWeekday, value) && !_isUpdating)
                    UpdateCronFromManual();
            }
        }

        // Options Collections
        public ObservableCollection<KeyValuePair<string, string>> MinuteOptions { get; }
        public ObservableCollection<KeyValuePair<string, string>> HourOptions { get; }
        public ObservableCollection<KeyValuePair<string, string>> DayOptions { get; }
        public ObservableCollection<KeyValuePair<string, string>> MonthOptions { get; }
        public ObservableCollection<KeyValuePair<string, string>> WeekdayOptions { get; }

        public ObservableCollection<KeyValuePair<string, string>> Presets { get; }

        private int _occurrencesCount = 10;
        public int OccurrencesCount
        {
            get => _occurrencesCount;
            set
            {
                if (SetProperty(ref _occurrencesCount, value))
                {
                    Parse();
                }
            }
        }

        public ICommand ApplyPresetCommand { get; }
        public ICommand IncreaseOccurrencesCommand { get; }
        public ICommand DecreaseOccurrencesCommand { get; }

        public CronToolViewModel()
        {
            MinuteOptions = new ObservableCollection<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Every minute", "*"),
                new KeyValuePair<string, string>("0", "0"),
                new KeyValuePair<string, string>("15", "15"),
                new KeyValuePair<string, string>("30", "30"),
                new KeyValuePair<string, string>("45", "45"),
                new KeyValuePair<string, string>("Every 5 min", "*/5"),
                new KeyValuePair<string, string>("Every 10 min", "*/10"),
                new KeyValuePair<string, string>("Every 15 min", "*/15"),
                new KeyValuePair<string, string>("Every 30 min", "*/30")
            };

            HourOptions = new ObservableCollection<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Every hour", "*"),
                new KeyValuePair<string, string>("Midnight (0)", "0"),
                new KeyValuePair<string, string>("6 AM", "6"),
                new KeyValuePair<string, string>("9 AM", "9"),
                new KeyValuePair<string, string>("Noon (12)", "12"),
                new KeyValuePair<string, string>("6 PM", "18"),
                new KeyValuePair<string, string>("Every 6 hours", "*/6"),
                new KeyValuePair<string, string>("Every 12 hours", "*/12")
            };

            DayOptions = new ObservableCollection<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Every day", "*"),
                new KeyValuePair<string, string>("1st", "1"),
                new KeyValuePair<string, string>("15th", "15"),
                new KeyValuePair<string, string>("Every 7 days", "*/7"),
                new KeyValuePair<string, string>("Every 14 days", "*/14")
            };

            MonthOptions = new ObservableCollection<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Every month", "*"),
                new KeyValuePair<string, string>("January", "1"),
                new KeyValuePair<string, string>("February", "2"),
                new KeyValuePair<string, string>("March", "3"),
                new KeyValuePair<string, string>("April", "4"),
                new KeyValuePair<string, string>("May", "5"),
                new KeyValuePair<string, string>("June", "6"),
                new KeyValuePair<string, string>("July", "7"),
                new KeyValuePair<string, string>("August", "8"),
                new KeyValuePair<string, string>("September", "9"),
                new KeyValuePair<string, string>("October", "10"),
                new KeyValuePair<string, string>("November", "11"),
                new KeyValuePair<string, string>("December", "12")
            };

            WeekdayOptions = new ObservableCollection<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Every day", "*"),
                new KeyValuePair<string, string>("Sunday", "0"),
                new KeyValuePair<string, string>("Monday", "1"),
                new KeyValuePair<string, string>("Tuesday", "2"),
                new KeyValuePair<string, string>("Wednesday", "3"),
                new KeyValuePair<string, string>("Thursday", "4"),
                new KeyValuePair<string, string>("Friday", "5"),
                new KeyValuePair<string, string>("Saturday", "6"),
                new KeyValuePair<string, string>("Weekdays", "1-5"),
                new KeyValuePair<string, string>("Weekends", "0,6")
            };

            Presets = new ObservableCollection<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Every minute", "* * * * *"),
                new KeyValuePair<string, string>("Every 5 minutes", "*/5 * * * *"),
                new KeyValuePair<string, string>("Every 15 minutes", "*/15 * * * *"),
                new KeyValuePair<string, string>("Every 30 minutes", "*/30 * * * *"),
                new KeyValuePair<string, string>("Every hour", "0 * * * *"),
                new KeyValuePair<string, string>("Every 6 hours", "0 */6 * * *"),
                new KeyValuePair<string, string>("Every day at midnight", "0 0 * * *"),
                new KeyValuePair<string, string>("Every Monday at 9am", "0 9 * * 1")
            };

            ApplyPresetCommand = new RelayCommand<string>(ApplyPreset);
            IncreaseOccurrencesCommand = new RelayCommand(IncreaseOccurrences);
            DecreaseOccurrencesCommand = new RelayCommand(DecreaseOccurrences);

            UpdateDescription();
            Parse(); // Initial parse
        }

        private void IncreaseOccurrences()
        {
            OccurrencesCount += 5;
        }

        private void DecreaseOccurrences()
        {
            int newCount = OccurrencesCount - 5;
            if (newCount < 10) newCount = 10;
            OccurrencesCount = newCount;
        }

        private void ApplyPreset(string expression)
        {
            if (!string.IsNullOrEmpty(expression))
            {
                Expression = expression;
            }
        }

        private void UpdateManualFromCron(string expression)
        {
            _isUpdating = true;
            try
            {
                var parts = expression.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 5)
                {
                    SelectedMinute = parts[0];
                    SelectedHour = parts[1];
                    SelectedDay = parts[2];
                    SelectedMonth = parts[3];
                    SelectedWeekday = parts[4];
                }
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private void UpdateCronFromManual()
        {
            _isUpdating = true;
            try
            {
                Expression = $"{SelectedMinute} {SelectedHour} {SelectedDay} {SelectedMonth} {SelectedWeekday}";
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private void UpdateDescription()
        {
            CronDescription = CronScheduler.GetDescription(Expression);
        }

        private void Parse()
        {
            Error = null;
            NextOccurrences.Clear();
            try
            {
                var dates = CronScheduler.GetNextOccurrences(Expression, OccurrencesCount);
                foreach (var date in dates)
                {
                    NextOccurrences.Add(date.ToString("yyyy-MM-dd HH:mm:ss"));
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
            }
        }
    }
}
