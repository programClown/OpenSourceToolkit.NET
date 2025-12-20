using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.NET.Localization;
using OpenSourceToolkit.TextData;
using QRCoder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using System.Threading.Tasks;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public enum QrPayloadType { Text, Url, Email, WiFi }
    public enum QrOutputFormat { Png, Svg, Both }

    public partial class QrCodeToolViewModel : ToolViewModel
    {
        public override int Id => 5;
        public override string Name => ToolkitLocalization.GetString("Tool_QrCode_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_QrCode_Description");
        public override string IconKey => "QrCodeIcon";

        // Settings
        private QrEccLevel _eccLevel = QrEccLevel.Q;
        public QrEccLevel EccLevel
        {
            get => _eccLevel;
            set => SetProperty(ref _eccLevel, value);
        }

        public QrEccLevel[] EccLevels { get; } = (QrEccLevel[])Enum.GetValues(typeof(QrEccLevel));

        private QrOutputFormat _outputFormat = QrOutputFormat.Png;
        public QrOutputFormat OutputFormat
        {
            get => _outputFormat;
            set
            {
                if (SetProperty(ref _outputFormat, value))
                {
                    OnPropertyChanged(nameof(IsPngVisible));
                    OnPropertyChanged(nameof(IsSvgVisible));
                }
            }
        }

        public QrOutputFormat[] OutputFormats { get; } = (QrOutputFormat[])Enum.GetValues(typeof(QrOutputFormat));

        private QrPayloadType _payloadType = QrPayloadType.Text;
        public QrPayloadType PayloadType
        {
            get => _payloadType;
            set
            {
                if (SetProperty(ref _payloadType, value))
                {
                    OnPropertyChanged(nameof(IsTextOrUrl));
                    OnPropertyChanged(nameof(IsEmail));
                    OnPropertyChanged(nameof(IsWiFi));
                }
            }
        }

        public QrPayloadType[] PayloadTypes { get; } = (QrPayloadType[])Enum.GetValues(typeof(QrPayloadType));

        // Platform Support
        public bool IsPngSupported => QrCodeGenerator.IsPngSupported;

        // Visibility Helpers
        public bool IsTextOrUrl => PayloadType == QrPayloadType.Text || PayloadType == QrPayloadType.Url;
        public bool IsEmail => PayloadType == QrPayloadType.Email;
        public bool IsWiFi => PayloadType == QrPayloadType.WiFi;

        public bool IsPngVisible => IsPngSupported && (OutputFormat == QrOutputFormat.Png || OutputFormat == QrOutputFormat.Both);
        public bool IsSvgVisible => OutputFormat == QrOutputFormat.Svg || OutputFormat == QrOutputFormat.Both;

        // Input Fields
        private string _inputText;
        public string InputText
        {
            get => _inputText;
            set => SetProperty(ref _inputText, value);
        }

        private string _emailAddress;
        public string EmailAddress
        {
            get => _emailAddress;
            set => SetProperty(ref _emailAddress, value);
        }

        private string _emailSubject;
        public string EmailSubject
        {
            get => _emailSubject;
            set => SetProperty(ref _emailSubject, value);
        }

        private string _emailMessage;
        public string EmailMessage
        {
            get => _emailMessage;
            set => SetProperty(ref _emailMessage, value);
        }

        private string _wifiSsid;
        public string WifiSsid
        {
            get => _wifiSsid;
            set => SetProperty(ref _wifiSsid, value);
        }

        private string _wifiPassword;
        public string WifiPassword
        {
            get => _wifiPassword;
            set => SetProperty(ref _wifiPassword, value);
        }

        private string _wifiType = "WPA";
        public string WifiType
        {
            get => _wifiType;
            set => SetProperty(ref _wifiType, value);
        }
        public string[] WifiTypes { get; } = new[] { "WPA", "WEP", "nopass" };

        // Outputs
        private Bitmap _qrImage;
        public Bitmap QrImage
        {
            get => _qrImage;
            set => SetProperty(ref _qrImage, value);
        }

        private byte[] _lastPngBytes;
        public byte[] LastPngBytes
        {
            get => _lastPngBytes;
            set => SetProperty(ref _lastPngBytes, value);
        }

        private string _svgText;
        public string SvgText
        {
            get => _svgText;
            set => SetProperty(ref _svgText, value);
        }

        private string _copyStatusMessage;
        public string CopyStatusMessage
        {
            get => _copyStatusMessage;
            set => SetProperty(ref _copyStatusMessage, value);
        }

        public ICommand GenerateCommand { get; }

        public QrCodeToolViewModel()
        {
            GenerateCommand = new RelayCommand(Generate);

            // Default to SVG format if PNG not supported on this platform
            if (!QrCodeGenerator.IsPngSupported)
            {
                _outputFormat = QrOutputFormat.Svg;
            }

            var lorem = new LoremIpsumGenerator();
            InputText = lorem.GenerateParagraphs(3);
        }

        public async Task ShowCopyStatusAsync(string message)
        {
            CopyStatusMessage = message;
            await Task.Delay(3000);
            CopyStatusMessage = string.Empty;
        }

        private void Generate()
        {
            string payload = GetPayloadString();
            if (string.IsNullOrEmpty(payload)) return;

            if (IsPngVisible)
            {
                var bytes = QrCodeGenerator.GeneratePng(payload, 20, EccLevel);
                LastPngBytes = bytes;
                using (var stream = new MemoryStream(bytes))
                {
                    QrImage = new Bitmap(stream);
                }
            }
            else
            {
                QrImage = null;
                LastPngBytes = null;
            }

            if (IsSvgVisible)
            {
                SvgText = QrCodeGenerator.GenerateSvg(payload, 20, EccLevel);
            }
            else
            {
                SvgText = null;
            }
        }

        private string GetPayloadString()
        {
            switch (PayloadType)
            {
                case QrPayloadType.Url:
                    return new PayloadGenerator.Url(InputText).ToString();
                case QrPayloadType.Email:
                    return new PayloadGenerator.Mail(EmailAddress, EmailSubject, EmailMessage).ToString();
                case QrPayloadType.WiFi:
                    var authMode = PayloadGenerator.WiFi.Authentication.WPA;
                    if (WifiType == "WEP") authMode = PayloadGenerator.WiFi.Authentication.WEP;
                    if (WifiType == "nopass") authMode = PayloadGenerator.WiFi.Authentication.nopass;
                    return new PayloadGenerator.WiFi(WifiSsid, WifiPassword, authMode).ToString();
                case QrPayloadType.Text:
                default:
                    return InputText;
            }
        }
    }
}
