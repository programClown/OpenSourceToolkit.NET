using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace OpenSourceToolkit.ApiTesting
{
    public class ApiTester
    {
        private readonly HttpClient _httpClient;

        public ApiTester(HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<ApiTestResult> ExecuteAsync(ApiRequest request)
        {
            var result = new ApiTestResult { Request = request };

            // Handle URL query parameters for API Key
            string requestUrl = request.Url;
            if (request.Auth != null &&
                request.Auth.Type == AuthType.ApiKey &&
                request.Auth.ApiKeyLocation == ApiKeyLocation.Query &&
                !string.IsNullOrEmpty(request.Auth.ApiKeyName) &&
                !string.IsNullOrEmpty(request.Auth.ApiKey))
            {
                var uriBuilder = new UriBuilder(requestUrl);
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                query[request.Auth.ApiKeyName] = request.Auth.ApiKey;
                uriBuilder.Query = query.ToString();
                requestUrl = uriBuilder.ToString();
            }

            var httpRequest = new HttpRequestMessage(new HttpMethod(request.Method), requestUrl);

            // Apply Headers
            if (request.Headers != null)
            {
                foreach (var header in request.Headers)
                {
                    httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            // Apply Auth Headers
            ApplyAuth(httpRequest, request.Auth);

            // Apply Body
            if (!string.IsNullOrEmpty(request.Body) &&
                (request.Method == "POST" || request.Method == "PUT" || request.Method == "PATCH" || request.Method == "DELETE"))
            {
                httpRequest.Content = new StringContent(request.Body, Encoding.UTF8, request.ContentType ?? "application/json");
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var response = await _httpClient.SendAsync(httpRequest);
                stopwatch.Stop();

                result.StatusCode = (int)response.StatusCode;
                result.ResponseHeaders = new Dictionary<string, string>();

                foreach (var header in response.Headers)
                {
                    result.ResponseHeaders[header.Key] = string.Join(",", header.Value);
                }
                if (response.Content != null)
                {
                    foreach (var header in response.Content.Headers)
                    {
                        result.ResponseHeaders[header.Key] = string.Join(",", header.Value);
                    }
                    result.ResponseBody = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    result.ResponseBody = string.Empty;
                }

                result.DurationMs = stopwatch.ElapsedMilliseconds;
                result.Success = true;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.DurationMs = stopwatch.ElapsedMilliseconds;
            }

            return result;
        }

        private void ApplyAuth(HttpRequestMessage message, AuthConfig auth)
        {
            if (auth == null) return;

            switch (auth.Type)
            {
                case AuthType.Bearer:
                    if (!string.IsNullOrEmpty(auth.Token))
                    {
                        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.Token);
                    }
                    break;
                case AuthType.Basic:
                    if (!string.IsNullOrEmpty(auth.Username) && !string.IsNullOrEmpty(auth.Password))
                    {
                        var credential = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{auth.Username}:{auth.Password}"));
                        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credential);
                    }
                    break;
                case AuthType.ApiKey:
                    if (auth.ApiKeyLocation == ApiKeyLocation.Header &&
                        !string.IsNullOrEmpty(auth.ApiKeyName) &&
                        !string.IsNullOrEmpty(auth.ApiKey))
                    {
                        message.Headers.TryAddWithoutValidation(auth.ApiKeyName, auth.ApiKey);
                    }
                    break;
                case AuthType.None:
                default:
                    break;
            }
        }

        public static ApiRequest ApplyEnvironmentVariables(ApiRequest request, Dictionary<string, string> variables)
        {
            if (variables == null || variables.Count == 0) return request;

            var newRequest = new ApiRequest
            {
                Url = ReplaceVariables(request.Url, variables),
                Method = request.Method,
                Body = ReplaceVariables(request.Body, variables),
                ContentType = request.ContentType,
                Auth = request.Auth, // Shallow copy, auth usually doesn't have variables but could
                Headers = new Dictionary<string, string>()
            };

            if (request.Headers != null)
            {
                foreach (var header in request.Headers)
                {
                    newRequest.Headers[header.Key] = ReplaceVariables(header.Value, variables);
                }
            }

            // Also apply to Auth fields if necessary
            if (newRequest.Auth != null)
            {
                newRequest.Auth = new AuthConfig
                {
                    Type = newRequest.Auth.Type,
                    Token = ReplaceVariables(newRequest.Auth.Token, variables),
                    Username = ReplaceVariables(newRequest.Auth.Username, variables),
                    Password = ReplaceVariables(newRequest.Auth.Password, variables),
                    ApiKey = ReplaceVariables(newRequest.Auth.ApiKey, variables),
                    ApiKeyName = ReplaceVariables(newRequest.Auth.ApiKeyName, variables),
                    ApiKeyLocation = newRequest.Auth.ApiKeyLocation
                };
            }

            return newRequest;
        }

        private static string ReplaceVariables(string input, Dictionary<string, string> variables)
        {
            if (string.IsNullOrEmpty(input)) return input;

            var result = input;
            foreach (var variable in variables)
            {
                // Regex to match {{ variableKey }} with optional whitespace
                string pattern = $@"\{{\{{\s*{Regex.Escape(variable.Key)}\s*\}}\}}";
                result = Regex.Replace(result, pattern, variable.Value ?? "");
            }
            return result;
        }
    }

    public class ApiRequest
    {
        public string Url { get; set; }
        public string Method { get; set; } = "GET";
        public Dictionary<string, string> Headers { get; set; }
        public string Body { get; set; }
        public string ContentType { get; set; } = "application/json";
        public AuthConfig Auth { get; set; } = new AuthConfig();
    }

    public class AuthConfig
    {
        public AuthType Type { get; set; } = AuthType.None;
        public string Token { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ApiKey { get; set; }
        public string ApiKeyName { get; set; }
        public ApiKeyLocation ApiKeyLocation { get; set; } = ApiKeyLocation.Header;
    }

    public enum AuthType
    {
        None,
        Bearer,
        Basic,
        ApiKey
    }

    public enum ApiKeyLocation
    {
        Header,
        Query
    }

    public class ApiTestResult
    {
        public ApiRequest Request { get; set; }
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public Dictionary<string, string> ResponseHeaders { get; set; }
        public string ResponseBody { get; set; }
        public long DurationMs { get; set; }
        public string ErrorMessage { get; set; }
    }
}
