using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.Converters;
using OpenSourceToolkit.NET.Localization;
using System;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public partial class TimestampToolViewModel : ToolViewModel
    {
        public override int Id => 7;
        public override string Name => ToolkitLocalization.GetString("Tool_Timestamp_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_Timestamp_Description");
        public override string IconKey => "TimestampIcon";

        private DateTimeOffset _date = DateTimeOffset.Now;
        public DateTimeOffset Date
        {
            get => _date;
            set
            {
                if (SetProperty(ref _date, value))
                {
                    UpdateUnixFromDate();
                }
            }
        }

        private long _unixSeconds;
        public long UnixSeconds
        {
            get => _unixSeconds;
            set => SetProperty(ref _unixSeconds, value);
        }

        public ICommand ConvertToDateCommand { get; }

        public TimestampToolViewModel()
        {
            ConvertToDateCommand = new RelayCommand(ConvertFromUnix);
            UpdateUnixFromDate();
        }

        private void UpdateUnixFromDate()
        {
            UnixSeconds = TimestampConverter.ToUnixTimeSeconds(Date.DateTime);
        }

        private void ConvertFromUnix()
        {
            Date = new DateTimeOffset(TimestampConverter.FromUnixTimeSeconds(UnixSeconds));
        }
    }
}
