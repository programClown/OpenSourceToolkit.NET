using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.NET.Localization;
using OpenSourceToolkit.Security;
using System;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public partial class JwtToolViewModel : ToolViewModel
    {
        public override int Id => 10;
        public override string Name => ToolkitLocalization.GetString("Tool_Jwt_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_Jwt_Description");
        public override string IconKey => "JwtIcon";

        private string _encodedToken;
        public string EncodedToken
        {
            get => _encodedToken;
            set => SetProperty(ref _encodedToken, value);
        }

        private string _decodedHeader;
        public string DecodedHeader
        {
            get => _decodedHeader;
            set => SetProperty(ref _decodedHeader, value);
        }

        private string _decodedPayload;
        public string DecodedPayload
        {
            get => _decodedPayload;
            set => SetProperty(ref _decodedPayload, value);
        }

        // Generation properties
        private string _secret = "super_secret_key_1234567890abcde";
        public string Secret
        {
            get => _secret;
            set => SetProperty(ref _secret, value);
        }

        private string _issuer = "OpenSourceToolkit";
        public string Issuer
        {
            get => _issuer;
            set => SetProperty(ref _issuer, value);
        }

        public ICommand DecodeCommand { get; }
        public ICommand GenerateCommand { get; }

        public JwtToolViewModel()
        {
            DecodeCommand = new RelayCommand(Decode);
            GenerateCommand = new RelayCommand(Generate);
        }

        private void Decode()
        {
            if (string.IsNullOrEmpty(EncodedToken)) return;

            try
            {
                var token = JwtHelper.DecodeToken(EncodedToken);
                if (token != null)
                {
                    // Serialize Header and Payload to formatted JSON
                    var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                    DecodedHeader = System.Text.Json.JsonSerializer.Serialize(token.Header, options);
                    DecodedPayload = System.Text.Json.JsonSerializer.Serialize(token.Payload, options);
                }
                else
                {
                    DecodedPayload = "Invalid Token";
                }
            }
            catch (Exception ex)
            {
                DecodedPayload = $"Error: {ex.Message}";
            }
        }

        private void Generate()
        {
            const int MinSecretLength = 32; // HS256 requires at least 256 bits (32 bytes)
            
            if (string.IsNullOrEmpty(Secret) || Secret.Length < MinSecretLength)
            {
                var charsNeeded = MinSecretLength - (Secret?.Length ?? 0);
                EncodedToken = $"Secret key too short. Minimum {MinSecretLength} characters required ({charsNeeded} more needed).";
                return;
            }
            
            try
            {
                EncodedToken = JwtHelper.GenerateToken(Secret, Issuer, "User");
                Decode();
            }
            catch (Exception ex)
            {
                EncodedToken = $"Error generating token: {ex.Message}";
            }
        }
    }
}
