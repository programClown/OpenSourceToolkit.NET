using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenSourceToolkit.ApiTesting;
using OpenSourceToolkit.NET.Localization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OpenSourceToolkit.NET.ViewModels.Tools
{
    public class ApiTesterToolViewModel : ToolViewModel
    {
        public override int Id => 23;
        public override string Name => ToolkitLocalization.GetString("Tool_ApiTester_Name");
        public override string Description => ToolkitLocalization.GetString("Tool_ApiTester_Description");
        public override string IconKey => "ApiTesterIcon";

        private readonly ApiTester _apiTester;

        public ApiTesterToolViewModel()
        {
            _apiTester = new ApiTester();
            Methods = new ObservableCollection<string> { "GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS" };
            AuthTypes = new ObservableCollection<AuthType> { AuthType.None, AuthType.Bearer, AuthType.Basic, AuthType.ApiKey };
            ApiKeyLocations = new ObservableCollection<ApiKeyLocation> { ApiKeyLocation.Header, ApiKeyLocation.Query };
            ExecuteRequestCommand = new AsyncRelayCommand(ExecuteRequestAsync);
        }

        private string _url = "https://jsonplaceholder.typicode.com/posts/1";
        public string Url
        {
            get => _url;
            set => SetProperty(ref _url, value);
        }

        private string _selectedMethod = "GET";
        public string SelectedMethod
        {
            get => _selectedMethod;
            set => SetProperty(ref _selectedMethod, value);
        }

        public ObservableCollection<string> Methods { get; }

        private string _headers = "Content-Type: application/json";
        public string Headers
        {
            get => _headers;
            set => SetProperty(ref _headers, value);
        }

        private string _body;
        public string Body
        {
            get => _body;
            set => SetProperty(ref _body, value);
        }

        // Auth
        private AuthType _selectedAuthType = AuthType.None;
        public AuthType SelectedAuthType
        {
            get => _selectedAuthType;
            set => SetProperty(ref _selectedAuthType, value);
        }

        public ObservableCollection<AuthType> AuthTypes { get; }

        private string _authToken;
        public string AuthToken
        {
            get => _authToken;
            set => SetProperty(ref _authToken, value);
        }

        private string _authUsername;
        public string AuthUsername
        {
            get => _authUsername;
            set => SetProperty(ref _authUsername, value);
        }

        private string _authPassword;
        public string AuthPassword
        {
            get => _authPassword;
            set => SetProperty(ref _authPassword, value);
        }

        private string _apiKey;
        public string ApiKey
        {
            get => _apiKey;
            set => SetProperty(ref _apiKey, value);
        }

        private string _apiKeyName = "X-API-Key";
        public string ApiKeyName
        {
            get => _apiKeyName;
            set => SetProperty(ref _apiKeyName, value);
        }

        private ApiKeyLocation _selectedApiKeyLocation = ApiKeyLocation.Header;
        public ApiKeyLocation SelectedApiKeyLocation
        {
            get => _selectedApiKeyLocation;
            set => SetProperty(ref _selectedApiKeyLocation, value);
        }

        public ObservableCollection<ApiKeyLocation> ApiKeyLocations { get; }

        // Response
        private int _responseStatusCode;
        public int ResponseStatusCode
        {
            get => _responseStatusCode;
            set => SetProperty(ref _responseStatusCode, value);
        }

        private string _responseStatusText;
        public string ResponseStatusText
        {
            get => _responseStatusText;
            set => SetProperty(ref _responseStatusText, value);
        }

        private string _responseBody;
        public string ResponseBody
        {
            get => _responseBody;
            set => SetProperty(ref _responseBody, value);
        }

        private string _responseHeaders;
        public string ResponseHeaders
        {
            get => _responseHeaders;
            set => SetProperty(ref _responseHeaders, value);
        }

        private string _responseTime;
        public string ResponseTime
        {
            get => _responseTime;
            set => SetProperty(ref _responseTime, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private bool _hasResponse;
        public bool HasResponse
        {
            get => _hasResponse;
            set => SetProperty(ref _hasResponse, value);
        }

        public ICommand ExecuteRequestCommand { get; }

        private async Task ExecuteRequestAsync()
        {
            if (string.IsNullOrWhiteSpace(Url)) return;

            IsLoading = true;
            HasResponse = false;
            ResponseBody = string.Empty;
            ResponseHeaders = string.Empty;
            ResponseStatusCode = 0;
            ResponseTime = string.Empty;

            try
            {
                var request = new ApiRequest
                {
                    Url = Url,
                    Method = SelectedMethod,
                    Body = Body,
                    Headers = ParseHeaders(Headers),
                    Auth = new AuthConfig
                    {
                        Type = SelectedAuthType,
                        Token = AuthToken,
                        Username = AuthUsername,
                        Password = AuthPassword,
                        ApiKey = ApiKey,
                        ApiKeyName = ApiKeyName,
                        ApiKeyLocation = SelectedApiKeyLocation
                    }
                };

                var result = await _apiTester.ExecuteAsync(request);

                ResponseStatusCode = result.StatusCode;
                ResponseBody = result.ResponseBody;
                ResponseHeaders = string.Join(Environment.NewLine, result.ResponseHeaders.Select(x => $"{x.Key}: {x.Value}"));
                ResponseTime = $"{result.DurationMs} ms";
                HasResponse = true;

                if (!result.Success)
                {
                     // If there was an exception (Success=false), show the error
                     ResponseBody = $"Error: {result.ErrorMessage}\n\n{result.ResponseBody}";
                }
            }
            catch (Exception ex)
            {
                ResponseBody = $"Error executing request: {ex.Message}";
                HasResponse = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private Dictionary<string, string> ParseHeaders(string headerString)
        {
            var headers = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(headerString)) return headers;

            var lines = headerString.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var parts = line.Split(new[] { ':' }, 2);
                if (parts.Length == 2)
                {
                    headers[parts[0].Trim()] = parts[1].Trim();
                }
            }
            return headers;
        }
    }
}
