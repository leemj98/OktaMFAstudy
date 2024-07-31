using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

[assembly: OwinStartup(typeof(WebApplication4.Startup))]

namespace WebApplication4
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }

        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies"
            });

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                ClientId = ConfigurationManager.AppSettings["okta:ClientId"],
                Authority = $"{ConfigurationManager.AppSettings["okta:OktaDomain"]}/oauth2/default",
                RedirectUri = ConfigurationManager.AppSettings["okta:RedirectUri"],
                PostLogoutRedirectUri = ConfigurationManager.AppSettings["okta:PostLogoutRedirectUri"],
                ResponseType = "code",
                Scope = "openid profile email",
                UseTokenLifetime = false,
                SignInAsAuthenticationType = "Cookies",
                ClientSecret = ConfigurationManager.AppSettings["okta:ClientSecret"],
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthorizationCodeReceived = async n =>
                    {
                        var tokenClient = new HttpClient();
                        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, $"{ConfigurationManager.AppSettings["okta:OktaDomain"]}/oauth2/default/v1/token");
                        tokenRequest.Content = new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("grant_type", "authorization_code"),
                            new KeyValuePair<string, string>("code", n.Code),
                            new KeyValuePair<string, string>("redirect_uri", ConfigurationManager.AppSettings["okta:RedirectUri"])
                        });
                        tokenRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ConfigurationManager.AppSettings["okta:ClientId"]}:{ConfigurationManager.AppSettings["okta:ClientSecret"]}")));

                        var tokenResponse = await tokenClient.SendAsync(tokenRequest);
                        var tokenContent = JsonConvert.DeserializeObject<dynamic>(await tokenResponse.Content.ReadAsStringAsync());

                        // 사용자 정보 가져오기
                        var userInfoClient = new HttpClient();
                        var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, $"{ConfigurationManager.AppSettings["okta:OktaDomain"]}/oauth2/default/v1/userinfo");
                        userInfoRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenContent.access_token.ToString());
                        var userInfoResponse = await userInfoClient.SendAsync(userInfoRequest);
                        var userInfo = JsonConvert.DeserializeObject<dynamic>(await userInfoResponse.Content.ReadAsStringAsync());

                        // 사용자 정보를 기반으로 추가 로직 수행 가능
                    },
                    AuthenticationFailed = n =>
                    {
                        n.HandleResponse();
                        n.Response.Redirect("/Error?message=" + n.Exception.Message);
                        return Task.FromResult(0);
                    }
                }
            });
        }
    }
}
