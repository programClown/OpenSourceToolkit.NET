using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.Hardware;
using OpenSourceToolkit.NET.Localization;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Threading;
using System.Linq;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public class AudioNoiseReductionToolViewModel : ToolViewModel
    {
        public override int Id => 25;
        public override string Name => ToolkitLocalization.GetString("Tool_AudioNoiseReduction_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_AudioNoiseReduction_Description");
        public override string IconKey => "AudioNoiseIcon";

        public bool IsAudioSupported => PlatformSupport.IsAudioSupported;

        private readonly AudioProcessor _processor;
        private DispatcherTimer _timer;
        private DispatcherTimer _peakDecayTimer;
        private int _recordingSeconds;
        private float _currentPeak;

        private bool _isRecording;
        public bool IsRecording
        {
            get => _isRecording;
            set => SetProperty(ref _isRecording, value);
        }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            set => SetProperty(ref _isPlaying, value);
        }

        private bool _isProcessing;
        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private string _recordingTimeDisplay = "00:00";
        public string RecordingTimeDisplay
        {
            get => _recordingTimeDisplay;
            set => SetProperty(ref _recordingTimeDisplay, value);
        }

        private bool _hasAudioFile;
        public bool HasAudioFile
        {
            get => _hasAudioFile;
            set => SetProperty(ref _hasAudioFile, value);
        }

        private string _fileName;
        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

        private string _fileSize;
        public string FileSize
        {
            get => _fileSize;
            set => SetProperty(ref _fileSize, value);
        }

        // Settings
        private double _noiseReduction = 50;
        public double NoiseReduction
        {
            get => _noiseReduction;
            set => SetProperty(ref _noiseReduction, value);
        }

        private double _gain = 0;
        public double Gain
        {
            get => _gain;
            set => SetProperty(ref _gain, value);
        }

        private double _highPassFilter = 80;
        public double HighPassFilter
        {
            get => _highPassFilter;
            set => SetProperty(ref _highPassFilter, value);
        }

        private double _lowPassFilter = 8000;
        public double LowPassFilter
        {
            get => _lowPassFilter;
            set => SetProperty(ref _lowPassFilter, value);
        }

        // Compressor
        private double _compThreshold = -24;
        public double CompThreshold
        {
            get => _compThreshold;
            set => SetProperty(ref _compThreshold, value);
        }

        private double _compRatio = 4;
        public double CompRatio
        {
            get => _compRatio;
            set => SetProperty(ref _compRatio, value);
        }

        private double _compAttack = 3;
        public double CompAttack
        {
            get => _compAttack;
            set => SetProperty(ref _compAttack, value);
        }

        private double _compRelease = 250;
        public double CompRelease
        {
            get => _compRelease;
            set => SetProperty(ref _compRelease, value);
        }

        // EQ
        private double _eqBass = 0;
        public double EqBass
        {
            get => _eqBass;
            set => SetProperty(ref _eqBass, value);
        }

        private double _eqMid = 0;
        public double EqMid
        {
            get => _eqMid;
            set => SetProperty(ref _eqMid, value);
        }

        private double _eqTreble = 0;
        public double EqTreble
        {
            get => _eqTreble;
            set => SetProperty(ref _eqTreble, value);
        }

        // Audio device selection
        public ObservableCollection<AudioDeviceManager.AudioDevice> InputDevices { get; } = new ObservableCollection<AudioDeviceManager.AudioDevice>();

        private AudioDeviceManager.AudioDevice _selectedInputDevice;
        public AudioDeviceManager.AudioDevice SelectedInputDevice
        {
            get => _selectedInputDevice;
            set
            {
                if (SetProperty(ref _selectedInputDevice, value) && value != null)
                {
                    SetSetting("InputDeviceName", value.Name);
                }
            }
        }

        // Peak meter (0.0 to 1.0)
        private double _peakLevel;
        public double PeakLevel
        {
            get => _peakLevel;
            set => SetProperty(ref _peakLevel, value);
        }

        // Mic monitoring
        private bool _isMonitoring;
        public bool IsMonitoring
        {
            get => _isMonitoring;
            set => SetProperty(ref _isMonitoring, value);
        }

        // Export format options
        public ObservableCollection<string> ExportFormats { get; } = new ObservableCollection<string> { "WAV", "MP3" };

        private string _selectedExportFormat = "WAV";
        public string SelectedExportFormat
        {
            get => _selectedExportFormat;
            set
            {
                if (SetProperty(ref _selectedExportFormat, value))
                {
                    OnPropertyChanged(nameof(IsMp3Selected));
                    SetSetting("ExportFormat", value);
                }
            }
        }

        public bool IsMp3Selected => SelectedExportFormat == "MP3";

        public ObservableCollection<int> Mp3Bitrates { get; } = new ObservableCollection<int> { 128, 192, 256, 320 };

        private int _selectedMp3Bitrate = 192;
        public int SelectedMp3Bitrate
        {
            get => _selectedMp3Bitrate;
            set
            {
                if (SetProperty(ref _selectedMp3Bitrate, value))
                {
                    SetSetting("Mp3Bitrate", value);
                }
            }
        }

        public ObservableCollection<double> VisualizationData { get; } = new ObservableCollection<double>();

        public ICommand RefreshDevicesCommand { get; }
        public ICommand ToggleMonitoringCommand { get; }

        // Commands
        public ICommand ToggleRecordingCommand { get; }
        public ICommand UploadFileAsyncCommand { get; }
        public ICommand ProcessAudioCommand { get; }
        public ICommand PlayOriginalCommand { get; }
        public ICommand PlayProcessedCommand { get; }
        public ICommand StopPlaybackCommand { get; }
        public ICommand ExportAsyncCommand { get; }
        public ICommand ResetCommand { get; }

        public AudioNoiseReductionToolViewModel()
        {
            // Restore saved settings
            _selectedExportFormat = GetSetting("ExportFormat", "WAV");
            _selectedMp3Bitrate = GetSetting("Mp3Bitrate", 192);

            if (IsAudioSupported)
            {
                _processor = new AudioProcessor();
                _processor.OnVisualizationDataAvailable += OnVisualizationData;
                _processor.OnPlaybackStopped += () =>
                {
                    Dispatcher.UIThread.Post(() =>
                    {
                        IsPlaying = false;
                        PeakLevel = 0;
                        _currentPeak = 0;
                        StatusMessage = "Playback stopped";
                        _peakDecayTimer.Start();
                    });
                };
            }

            ToggleRecordingCommand = new RelayCommand(ToggleRecording, () => IsAudioSupported);
            UploadFileAsyncCommand = new AsyncRelayCommand(UploadFileAsync, () => IsAudioSupported);
            ProcessAudioCommand = new RelayCommand(ProcessAudio, () => IsAudioSupported);
            PlayOriginalCommand = new RelayCommand(PlayOriginal, () => IsAudioSupported);
            PlayProcessedCommand = new RelayCommand(PlayProcessed, () => IsAudioSupported);
            StopPlaybackCommand = new RelayCommand(StopPlayback, () => IsAudioSupported);
            ExportAsyncCommand = new AsyncRelayCommand(ExportAsync, () => IsAudioSupported);
            ResetCommand = new RelayCommand(Reset, () => IsAudioSupported);
            RefreshDevicesCommand = new RelayCommand(RefreshDevices, () => IsAudioSupported);
            ToggleMonitoringCommand = new RelayCommand(ToggleMonitoring, () => IsAudioSupported);

            if (IsAudioSupported)
            {
                RefreshDevices();
            }
            else
            {
                StatusMessage = "Audio features are only supported on Windows.";
            }

            // Peak meter decay timer
            _peakDecayTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
            _peakDecayTimer.Tick += (s, e) =>
            {
                if (!IsRecording && !IsMonitoring && !IsPlaying)
                {
                    _currentPeak = (float)Math.Max(0, _currentPeak - 0.05f);
                    PeakLevel = Math.Max(0, PeakLevel - 0.05);
                    if (PeakLevel <= 0)
                        _peakDecayTimer.Stop();
                }
            };
        }

        private void ToggleMonitoring()
        {
            if (IsMonitoring)
            {
                _processor.StopMonitoring();
                IsMonitoring = false;
                _peakDecayTimer.Start();
                StatusMessage = "Monitoring stopped";
            }
            else
            {
                try
                {
                    int deviceNumber = SelectedInputDevice?.Id ?? 0;
                    _processor.StartMonitoring(deviceNumber);
                    IsMonitoring = true;
                    _currentPeak = 0;
                    PeakLevel = 0;
                    StatusMessage = "Monitoring...";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error: {ex.Message}";
                }
            }
        }

        private void RefreshDevices()
        {
            InputDevices.Clear();
            foreach (var device in AudioDeviceManager.GetInputDevices().OrderBy(d => d.Name))
            {
                InputDevices.Add(device);
            }
            if (InputDevices.Count > 0)
            {
                // Try to restore saved device
                var savedName = GetSetting<string>("InputDeviceName");
                var savedDevice = !string.IsNullOrEmpty(savedName)
                    ? InputDevices.FirstOrDefault(d => d.Name == savedName)
                    : null;
                _selectedInputDevice = savedDevice ?? InputDevices[0];
                OnPropertyChanged(nameof(SelectedInputDevice));
            }
        }

        private void OnVisualizationData(float[] data)
        {
            if (data == null || data.Length == 0) return;

            // Find peak sample value (absolute max) for responsive metering
            float peak = 0;
            for (int i = 0; i < data.Length; i++)
            {
                float abs = Math.Abs(data[i]);
                if (abs > peak) peak = abs;
            }

            // Apply a slight boost to compensate for typical mic input levels
            // Most consumer/prosumer setups don't hit 0dBFS even on loud speech
            peak = Math.Min(1.0f, peak * 1.5f);

            // Track with smoothing: fast attack, slower decay
            if (peak > _currentPeak)
                _currentPeak = peak; // instant attack for peaks
            else
                _currentPeak = _currentPeak * 0.95f + peak * 0.05f; // slow decay

            // Update UI on dispatcher thread
            Dispatcher.UIThread.Post(() =>
            {
                float finalPeak = Math.Min(1.0f, _currentPeak);
                if (float.IsNaN(finalPeak) || float.IsInfinity(finalPeak)) finalPeak = 0;
                PeakLevel = finalPeak;
            });
        }

        private void ToggleRecording()
        {
            if (IsRecording)
            {
                _processor.StopRecording();
                IsRecording = false;
                _timer?.Stop();
                _peakDecayTimer.Start();
                StatusMessage = "Recording stopped";
                HasAudioFile = true;
                FileName = "Recorded Audio";
            }
            else
            {
                // Stop monitoring if active
                if (IsMonitoring)
                {
                    _processor.StopMonitoring();
                    IsMonitoring = false;
                }

                try
                {
                    int deviceNumber = SelectedInputDevice?.Id ?? 0;
                    _processor.StartRecording(deviceNumber);
                    IsRecording = true;
                    _recordingSeconds = 0;
                    _currentPeak = 0;
                    PeakLevel = 0;
                    UpdateRecordingTime();
                    _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                    _timer.Tick += (s, e) =>
                    {
                        _recordingSeconds++;
                        UpdateRecordingTime();
                    };
                    _timer.Start();
                    StatusMessage = "Recording...";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error: {ex.Message}";
                }
            }
        }

        private void UpdateRecordingTime()
        {
            var ts = TimeSpan.FromSeconds(_recordingSeconds);
            RecordingTimeDisplay = $"{ts.Minutes:00}:{ts.Seconds:00}";
        }

        private async Task UploadFileAsync()
        {
            var storage = TopLevel?.StorageProvider;
            if (storage == null) return;

            var result = await storage.OpenFilePickerAsync(new global::Avalonia.Platform.Storage.FilePickerOpenOptions
            {
                Title = "Select Audio File",
                AllowMultiple = false,
                FileTypeFilter = new[] { new global::Avalonia.Platform.Storage.FilePickerFileType("Audio Files") { Patterns = new[] { "*.wav", "*.mp3" } } }
            });

            if (result != null && result.Count > 0)
            {
                var file = result[0];
                var path = file.Path.LocalPath;
                try
                {
                    IsPlaying = false;
                    _processor.LoadAudio(path);
                    HasAudioFile = true;
                    FileName = file.Name;
                    StatusMessage = "File loaded";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error loading file: {ex.Message}";
                }
            }
        }

        private void ProcessAudio()
        {
            if (!HasAudioFile) return;
            IsProcessing = true;
            StatusMessage = "Processing...";

            try
            {
                _processor.UpdateSettings(
                    (float)Gain,
                    (float)HighPassFilter,
                    (float)LowPassFilter,
                    (float)EqBass, (float)EqMid, (float)EqTreble,
                    (float)CompThreshold, (float)CompRatio, (float)CompAttack, (float)CompRelease
                );
                StatusMessage = "Settings applied";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private void PlayOriginal()
        {
            if (!HasAudioFile) return;
            _processor.Play(original: true);
            IsPlaying = true;
            StatusMessage = "Playing original...";
        }

        private void PlayProcessed()
        {
            if (!HasAudioFile) return;
            ProcessAudio();
            _processor.Play(original: false);
            IsPlaying = true;
            StatusMessage = "Playing processed...";
        }

        private void StopPlayback()
        {
            _processor.StopPlayback();
            IsPlaying = false;
            PeakLevel = 0;
            _currentPeak = 0;
            StatusMessage = "Stopped";
        }

        private async Task ExportAsync()
        {
            if (!HasAudioFile) return;

            var storage = TopLevel?.StorageProvider;
            if (storage == null) return;

            var isMp3 = SelectedExportFormat == "MP3";
            var extension = isMp3 ? "mp3" : "wav";
            var fileType = isMp3
                ? new global::Avalonia.Platform.Storage.FilePickerFileType("MP3 File") { Patterns = new[] { "*.mp3" } }
                : new global::Avalonia.Platform.Storage.FilePickerFileType("WAV File") { Patterns = new[] { "*.wav" } };

            var result = await storage.SaveFilePickerAsync(new global::Avalonia.Platform.Storage.FilePickerSaveOptions
            {
                Title = "Save Processed Audio",
                DefaultExtension = extension,
                FileTypeChoices = new[] { fileType }
            });

            if (result != null)
            {
                var path = result.Path.LocalPath;
                IsProcessing = true;
                StatusMessage = $"Exporting to {SelectedExportFormat}...";

                var format = isMp3 ? AudioExportFormat.Mp3 : AudioExportFormat.Wav;
                var bitrate = SelectedMp3Bitrate;

                await Task.Run(() =>
                {
                    try
                    {
                        _processor.ExportProcessed(path, format, bitrate);
                        Dispatcher.UIThread.Post(() => StatusMessage = "Export completed");
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.UIThread.Post(() => StatusMessage = $"Export failed: {ex.Message}");
                    }
                    finally
                    {
                        Dispatcher.UIThread.Post(() => IsProcessing = false);
                    }
                });
            }
        }

        private void Reset()
        {
            _processor.StopPlayback();
            _processor.StopRecording();
            IsRecording = false;
            IsPlaying = false;
            HasAudioFile = false;
            FileName = null;
            PeakLevel = 0;
            _currentPeak = 0;
            StatusMessage = "Reset";
        }

        // Helper for file picker
        private global::Avalonia.Controls.TopLevel TopLevel =>
            global::Avalonia.Controls.TopLevel.GetTopLevel(
                global::Avalonia.Application.Current.ApplicationLifetime is global::Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null);
    }
}
