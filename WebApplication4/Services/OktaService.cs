using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication4.Services
{
    public class OktaService
    {
        private readonly string _domain = "https://dev-36728140.okta.com";

        private async Task<dynamic> SendRequest(string url, object body)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri(_domain);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                string json = JsonConvert.SerializeObject(body);
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage response = await client.PostAsync(url, content);
                    string responseBody = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception($"Error calling Okta API: {responseBody}");
                    }

                    return JsonConvert.DeserializeObject(responseBody);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to send request to Okta API: {ex.Message}", ex);
                }
            }
        }

        public Task<dynamic> AuthenticateUser(string username, string password)
        {
            var body = new
            {
                username = username,
                password = password,
                options = new
                {
                    multiOptionalFactorEnroll = false,
                    warnBeforePasswordExpired = false
                }
            };

            return SendRequest("/api/v1/authn", body);
        }

        public Task<dynamic> VerifyFactor(string stateToken, string factorId, string passCode)
        {
            var body = new
            {
                stateToken = stateToken,
                passCode = passCode
            };

            return SendRequest($"/api/v1/authn/factors/{factorId}/verify", body);
        }

        public Task<dynamic> EnrollMfa(string stateToken, string factorId)
        {
            var body = new
            {
                stateToken = stateToken
            };

            return SendRequest($"/api/v1/authn/factors/{factorId}/lifecycle/activate", body);
        }

        public Task<dynamic> SendSmsCode(string stateToken, string factorId)
        {
            var body = new
            {
                stateToken = stateToken
            };

            return SendRequest($"/api/v1/authn/factors/{factorId}/verify", body);
        }
    }
}
