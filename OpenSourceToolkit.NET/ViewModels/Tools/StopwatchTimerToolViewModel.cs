using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.NET.Localization;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Threading;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public class LapTime
    {
        public int Id { get; set; }
        public TimeSpan TotalTime { get; set; }
        public TimeSpan LapDuration { get; set; }
    }

    public class StopwatchTimerToolViewModel : ToolViewModel
    {
        public override int Id => 36;
        public override string Name => ToolkitLocalization.GetString("Tool_StopwatchTimer_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_StopwatchTimer_Description");
        public override string IconKey => "StopwatchIcon";

        private DispatcherTimer _stopwatchTimer;
        private DispatcherTimer _countdownTimer;
        private DateTime _stopwatchStartTime;
        private TimeSpan _stopwatchElapsed;
        private TimeSpan _countdownRemaining;
        private TimeSpan _countdownInitial;

        // Stopwatch state
        private bool _stopwatchRunning;
        public bool StopwatchRunning
        {
            get => _stopwatchRunning;
            set
            {
                if (SetProperty(ref _stopwatchRunning, value))
                {
                    (StartStopwatchCommand as RelayCommand)?.NotifyCanExecuteChanged();
                    (PauseStopwatchCommand as RelayCommand)?.NotifyCanExecuteChanged();
                    (LapCommand as RelayCommand)?.NotifyCanExecuteChanged();
                }
            }
        }

        private string _stopwatchDisplay = "00:00.00";
        public string StopwatchDisplay
        {
            get => _stopwatchDisplay;
            set => SetProperty(ref _stopwatchDisplay, value);
        }

        public ObservableCollection<LapTime> LapTimes { get; } = new ObservableCollection<LapTime>();

        private int _lapCounter;

        // Timer state
        private bool _timerRunning;
        public bool TimerRunning
        {
            get => _timerRunning;
            set
            {
                if (SetProperty(ref _timerRunning, value))
                {
                    (StartTimerCommand as RelayCommand)?.NotifyCanExecuteChanged();
                    (PauseTimerCommand as RelayCommand)?.NotifyCanExecuteChanged();
                }
            }
        }

        private bool _timerFinished;
        public bool TimerFinished
        {
            get => _timerFinished;
            set => SetProperty(ref _timerFinished, value);
        }

        private string _timerDisplay = "00:00";
        public string TimerDisplay
        {
            get => _timerDisplay;
            set => SetProperty(ref _timerDisplay, value);
        }

        private int _timerHours;
        public int TimerHours
        {
            get => _timerHours;
            set
            {
                if (SetProperty(ref _timerHours, Math.Max(0, Math.Min(23, value))))
                    UpdateTimerDisplay();
            }
        }

        private int _timerMinutes = 5;
        public int TimerMinutes
        {
            get => _timerMinutes;
            set
            {
                if (SetProperty(ref _timerMinutes, Math.Max(0, Math.Min(59, value))))
                    UpdateTimerDisplay();
            }
        }

        private int _timerSeconds;
        public int TimerSeconds
        {
            get => _timerSeconds;
            set
            {
                if (SetProperty(ref _timerSeconds, Math.Max(0, Math.Min(59, value))))
                    UpdateTimerDisplay();
            }
        }

        private double _timerProgress;
        public double TimerProgress
        {
            get => _timerProgress;
            set => SetProperty(ref _timerProgress, value);
        }

        private bool _showTimerSetup = true;
        public bool ShowTimerSetup
        {
            get => _showTimerSetup;
            set => SetProperty(ref _showTimerSetup, value);
        }

        // Settings
        private bool _soundEnabled = true;
        public bool SoundEnabled
        {
            get => _soundEnabled;
            set => SetProperty(ref _soundEnabled, value);
        }

        private bool _autoRestart;
        public bool AutoRestart
        {
            get => _autoRestart;
            set => SetProperty(ref _autoRestart, value);
        }

        // Active tab
        private int _selectedTab;
        public int SelectedTab
        {
            get => _selectedTab;
            set
            {
                if (SetProperty(ref _selectedTab, value))
                {
                    OnPropertyChanged(nameof(IsStopwatchTabSelected));
                    OnPropertyChanged(nameof(IsTimerTabSelected));
                }
            }
        }

        public bool IsStopwatchTabSelected
        {
            get => _selectedTab == 0;
            set { if (value) SelectedTab = 0; }
        }

        public bool IsTimerTabSelected
        {
            get => _selectedTab == 1;
            set { if (value) SelectedTab = 1; }
        }

        // Commands
        public ICommand StartStopwatchCommand { get; }
        public ICommand PauseStopwatchCommand { get; }
        public ICommand ResetStopwatchCommand { get; }
        public ICommand LapCommand { get; }
        public ICommand ClearLapsCommand { get; }

        public ICommand StartTimerCommand { get; }
        public ICommand PauseTimerCommand { get; }
        public ICommand ResetTimerCommand { get; }
        public ICommand ApplyPresetCommand { get; }

        public StopwatchTimerToolViewModel()
        {
            _stopwatchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(10) };
            _stopwatchTimer.Tick += OnStopwatchTick;

            _countdownTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _countdownTimer.Tick += OnCountdownTick;

            StartStopwatchCommand = new RelayCommand(StartStopwatch, () => !StopwatchRunning);
            PauseStopwatchCommand = new RelayCommand(PauseStopwatch, () => StopwatchRunning);
            ResetStopwatchCommand = new RelayCommand(ResetStopwatch);
            LapCommand = new RelayCommand(RecordLap, () => StopwatchRunning);
            ClearLapsCommand = new RelayCommand(() => LapTimes.Clear());

            StartTimerCommand = new RelayCommand(StartTimer, () => !TimerRunning && (TimerHours > 0 || TimerMinutes > 0 || TimerSeconds > 0 || _countdownRemaining > TimeSpan.Zero));
            PauseTimerCommand = new RelayCommand(PauseTimer, () => TimerRunning);
            ResetTimerCommand = new RelayCommand(ResetTimer);
            ApplyPresetCommand = new RelayCommand<string>(ApplyPreset);

            UpdateTimerDisplay();
        }

        // Stopwatch methods
        private void StartStopwatch()
        {
            _stopwatchStartTime = DateTime.Now - _stopwatchElapsed;
            StopwatchRunning = true;
            _stopwatchTimer.Start();
        }

        private void PauseStopwatch()
        {
            _stopwatchTimer.Stop();
            _stopwatchElapsed = DateTime.Now - _stopwatchStartTime;
            StopwatchRunning = false;
        }

        private void ResetStopwatch()
        {
            _stopwatchTimer.Stop();
            StopwatchRunning = false;
            _stopwatchElapsed = TimeSpan.Zero;
            StopwatchDisplay = "00:00.00";
            LapTimes.Clear();
            _lapCounter = 0;
        }

        private void RecordLap()
        {
            if (!StopwatchRunning) return;

            var currentTime = DateTime.Now - _stopwatchStartTime;
            var lastLapTime = LapTimes.Count > 0 ? LapTimes[0].TotalTime : TimeSpan.Zero;
            var lapDuration = currentTime - lastLapTime;

            _lapCounter++;
            LapTimes.Insert(0, new LapTime
            {
                Id = _lapCounter,
                TotalTime = currentTime,
                LapDuration = lapDuration
            });
        }

        private void OnStopwatchTick(object sender, EventArgs e)
        {
            var elapsed = DateTime.Now - _stopwatchStartTime;
            StopwatchDisplay = FormatStopwatchTime(elapsed);
        }

        private string FormatStopwatchTime(TimeSpan ts)
        {
            if (ts.Hours > 0)
                return $"{ts.Hours}:{ts.Minutes:D2}:{ts.Seconds:D2}.{ts.Milliseconds / 10:D2}";
            return $"{ts.Minutes:D2}:{ts.Seconds:D2}.{ts.Milliseconds / 10:D2}";
        }

        // Timer methods
        private void StartTimer()
        {
            if (_countdownRemaining <= TimeSpan.Zero)
            {
                _countdownInitial = new TimeSpan(TimerHours, TimerMinutes, TimerSeconds);
                _countdownRemaining = _countdownInitial;
            }

            ShowTimerSetup = false;
            TimerFinished = false;
            TimerRunning = true;
            _countdownTimer.Start();
        }

        private void PauseTimer()
        {
            _countdownTimer.Stop();
            TimerRunning = false;
        }

        private void ResetTimer()
        {
            _countdownTimer.Stop();
            TimerRunning = false;
            TimerFinished = false;
            _countdownRemaining = TimeSpan.Zero;
            TimerProgress = 0;
            ShowTimerSetup = true;
            UpdateTimerDisplay();
        }

        private void OnCountdownTick(object sender, EventArgs e)
        {
            _countdownRemaining = _countdownRemaining.Subtract(TimeSpan.FromMilliseconds(100));

            if (_countdownRemaining <= TimeSpan.Zero)
            {
                _countdownRemaining = TimeSpan.Zero;
                _countdownTimer.Stop();
                TimerRunning = false;
                TimerFinished = true;
                TimerProgress = 100;
                TimerDisplay = "00:00";

                if (AutoRestart)
                {
                    _countdownRemaining = _countdownInitial;
                    TimerFinished = false;
                    TimerRunning = true;
                    _countdownTimer.Start();
                }
                return;
            }

            TimerDisplay = FormatTimerTime(_countdownRemaining);
            if (_countdownInitial.TotalSeconds > 0)
            {
                TimerProgress = 100 - (_countdownRemaining.TotalSeconds / _countdownInitial.TotalSeconds * 100);
            }
        }

        private string FormatTimerTime(TimeSpan ts)
        {
            if (ts.Hours > 0)
                return $"{ts.Hours}:{ts.Minutes:D2}:{ts.Seconds:D2}";
            return $"{ts.Minutes:D2}:{ts.Seconds:D2}";
        }

        private void UpdateTimerDisplay()
        {
            if (!TimerRunning && _countdownRemaining <= TimeSpan.Zero)
            {
                var ts = new TimeSpan(TimerHours, TimerMinutes, TimerSeconds);
                TimerDisplay = FormatTimerTime(ts);
            }
        }

        private void ApplyPreset(string preset)
        {
            if (TimerRunning) return;

            switch (preset)
            {
                case "1min":
                    TimerHours = 0; TimerMinutes = 1; TimerSeconds = 0;
                    break;
                case "5min":
                    TimerHours = 0; TimerMinutes = 5; TimerSeconds = 0;
                    break;
                case "10min":
                    TimerHours = 0; TimerMinutes = 10; TimerSeconds = 0;
                    break;
                case "15min":
                    TimerHours = 0; TimerMinutes = 15; TimerSeconds = 0;
                    break;
                case "25min":
                    TimerHours = 0; TimerMinutes = 25; TimerSeconds = 0;
                    break;
                case "30min":
                    TimerHours = 0; TimerMinutes = 30; TimerSeconds = 0;
                    break;
                case "1hour":
                    TimerHours = 1; TimerMinutes = 0; TimerSeconds = 0;
                    break;
            }
            ResetTimer();
        }

        public string FormatLapTime(TimeSpan ts)
        {
            return FormatStopwatchTime(ts);
        }

        public override void Cleanup()
        {
            _stopwatchTimer?.Stop();
            _countdownTimer?.Stop();
        }
    }
}
