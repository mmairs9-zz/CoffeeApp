using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CoffeApp.Helpers
{
    public class AzureAppServiceMicrosoftAccountClient
    {
        private Uri _BaseUri;
        private string _MicrosoftAccountToken;
        private string _ApplicationToken;

        public AzureAppServiceMicrosoftAccountClient(string baseUrl, string msaToken)
        {
            if (string.IsNullOrEmpty(baseUrl))
                throw new ArgumentException($"{nameof(baseUrl)} is null or empty.", nameof(baseUrl));
            if (string.IsNullOrEmpty(msaToken))
                throw new ArgumentException($"{nameof(msaToken)} is null or empty.", nameof(msaToken));

            _BaseUri = new Uri(baseUrl);
            _MicrosoftAccountToken = msaToken;
        }

        public async Task<string> GetStringFromApplicationService(string url)
        {
            using (var client = await GetClientForApplicationServiceCall())
            {
                return await client.GetStringAsync(url);
            }
        }

        public async Task<HttpClient> GetClientForApplicationServiceCall()
        {
            if (_ApplicationToken == null)
            {
                await GetApplicationToken();
            }

            var client = new HttpClient();

            client.BaseAddress = _BaseUri;

            // http://stackoverflow.com/questions/37243395/azure-app-services-exchange-authenticationtoken-for-session
            client.DefaultRequestHeaders.Add("x-zumo-auth", _ApplicationToken);

            return client;
        }

        public async Task GetApplicationToken()
        {
            // https://azure.microsoft.com/en-us/blog/announcing-app-service-authentication-authorization/

            using (var client = new HttpClient())
            {
                client.BaseAddress = _BaseUri;

                var jsonToPost = new JObject(
                    new JProperty("access_token", _MicrosoftAccountToken));

                var contentToPost = new StringContent(
                        JsonConvert.SerializeObject(jsonToPost),
                        Encoding.UTF8, "application/json");


                var asyncResult = await client.PostAsync(
                    "/.auth/login/microsoftaccount",
                    contentToPost);

                if (asyncResult.Content == null)
                {
                    throw new InvalidOperationException("Result from call was null.");
                }
                else
                {
                    var resultContentAsString = asyncResult.Content.ToString();

                    var converter = new ExpandoObjectConverter();
                    dynamic responseContentAsObject = JsonConvert.DeserializeObject<ExpandoObject>(
                        resultContentAsString, converter);

                    _ApplicationToken = responseContentAsObject.authenticationToken;
                }
            }
        }
    }
}
