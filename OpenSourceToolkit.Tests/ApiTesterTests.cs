using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenSourceToolkit.ApiTesting;

namespace OpenSourceToolkit.Tests
{
    [TestClass]
    public class ApiTesterTests
    {
        [TestMethod]
        public void ApplyEnvironmentVariables_ReplacesVariables_Correctly()
        {
            var variables = new Dictionary<string, string>
            {
                { "BASE_URL", "https://api.example.com" },
                { "USER_ID", "123" },
                { "TOKEN", "xyz-token" }
            };

            var request = new ApiRequest
            {
                Url = "{{ BASE_URL }}/users/{{ USER_ID }}",
                Headers = new Dictionary<string, string>
                {
                    { "Authorization", "Bearer {{ TOKEN }}" }
                },
                Body = "{ \"id\": \"{{ USER_ID }}\" }"
            };

            var processed = ApiTester.ApplyEnvironmentVariables(request, variables);

            Assert.AreEqual("https://api.example.com/users/123", processed.Url);
            Assert.AreEqual("Bearer xyz-token", processed.Headers["Authorization"]);
            Assert.AreEqual("{ \"id\": \"123\" }", processed.Body);
        }

        [TestMethod]
        public void ApplyEnvironmentVariables_HandlesMissingVariables_Gracefully()
        {
            var variables = new Dictionary<string, string>();

            var request = new ApiRequest
            {
                Url = "{{ BASE_URL }}/users",
                Body = "test"
            };

            var processed = ApiTester.ApplyEnvironmentVariables(request, variables);

            // Should remain unchanged if variable not found
            Assert.AreEqual("{{ BASE_URL }}/users", processed.Url);
        }

        // Since we cannot easily mock HttpClient without a wrapper interface or 3rd party library in this setup,
        // we will test the logic that builds the request headers/auth by inspecting the ApiTester private logic
        // or by refactoring ApiTester to be more testable.
        // However, for this exercise, we will verify the auth logic by using a MockHttpMessageHandler
        // if we want to go full integration, but let's stick to unit testing what we can.
        // Actually, we can create a fake HttpClient with a custom handler to intercept the request.

        private class MockHttpMessageHandler : HttpMessageHandler
        {
            public HttpRequestMessage LastRequest { get; private set; }
            public string ResponseContent { get; set; } = "{}";
            public System.Net.HttpStatusCode ResponseStatusCode { get; set; } = System.Net.HttpStatusCode.OK;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                LastRequest = request;
                var response = new HttpResponseMessage(ResponseStatusCode)
                {
                    Content = new StringContent(ResponseContent)
                };
                return Task.FromResult(response);
            }
        }

        [TestMethod]
        public async Task ExecuteAsync_BearerAuth_AddsHeader()
        {
            var handler = new MockHttpMessageHandler();
            using (var client = new HttpClient(handler))
            {
                var tester = new ApiTester(client);
                var request = new ApiRequest
                {
                    Url = "https://example.com",
                    Auth = new AuthConfig
                    {
                        Type = AuthType.Bearer,
                        Token = "my-token"
                    }
                };

                await tester.ExecuteAsync(request);

                Assert.IsNotNull(handler.LastRequest);
                Assert.IsNotNull(handler.LastRequest.Headers.Authorization);
                Assert.AreEqual("Bearer", handler.LastRequest.Headers.Authorization.Scheme);
                Assert.AreEqual("my-token", handler.LastRequest.Headers.Authorization.Parameter);
            }
        }

        [TestMethod]
        public async Task ExecuteAsync_BasicAuth_AddsHeader()
        {
            var handler = new MockHttpMessageHandler();
            using (var client = new HttpClient(handler))
            {
                var tester = new ApiTester(client);
                var request = new ApiRequest
                {
                    Url = "https://example.com",
                    Auth = new AuthConfig
                    {
                        Type = AuthType.Basic,
                        Username = "user",
                        Password = "pass"
                    }
                };

                await tester.ExecuteAsync(request);

                Assert.IsNotNull(handler.LastRequest);
                Assert.IsNotNull(handler.LastRequest.Headers.Authorization);
                Assert.AreEqual("Basic", handler.LastRequest.Headers.Authorization.Scheme);

                var expected = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes("user:pass"));
                Assert.AreEqual(expected, handler.LastRequest.Headers.Authorization.Parameter);
            }
        }

        [TestMethod]
        public async Task ExecuteAsync_ApiKeyHeader_AddsHeader()
        {
            var handler = new MockHttpMessageHandler();
            using (var client = new HttpClient(handler))
            {
                var tester = new ApiTester(client);
                var request = new ApiRequest
                {
                    Url = "https://example.com",
                    Auth = new AuthConfig
                    {
                        Type = AuthType.ApiKey,
                        ApiKey = "12345",
                        ApiKeyName = "X-API-Key",
                        ApiKeyLocation = ApiKeyLocation.Header
                    }
                };

                await tester.ExecuteAsync(request);

                Assert.IsNotNull(handler.LastRequest);
                Assert.IsTrue(handler.LastRequest.Headers.Contains("X-API-Key"));
                Assert.AreEqual("12345", handler.LastRequest.Headers.GetValues("X-API-Key").First());
            }
        }

        [TestMethod]
        public async Task ExecuteAsync_ApiKeyQuery_AddsQueryParam()
        {
            var handler = new MockHttpMessageHandler();
            using (var client = new HttpClient(handler))
            {
                var tester = new ApiTester(client);
                var request = new ApiRequest
                {
                    Url = "https://example.com/api",
                    Auth = new AuthConfig
                    {
                        Type = AuthType.ApiKey,
                        ApiKey = "12345",
                        ApiKeyName = "key",
                        ApiKeyLocation = ApiKeyLocation.Query
                    }
                };

                await tester.ExecuteAsync(request);

                Assert.IsNotNull(handler.LastRequest);
                StringAssert.Contains(handler.LastRequest.RequestUri.Query, "key=12345");
            }
        }
    }
}
