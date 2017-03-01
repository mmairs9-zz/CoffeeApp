using CoffeApp.Data;
using CoffeeApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.System;
using Windows.UI.ApplicationSettings;

namespace CoffeApp.Helpers
{
    /// <summary>
    /// Handles authentication. 
    /// </summary>
    public static class AuthenticationHelper
    {
        /// <summary>
        /// The dictonary key for the current username in roaming storage.
        /// </summary>
        private static readonly string RoamingUserKey = "current_user";

        /// <summary>
        /// The dictonary key for the current authentication provider in roaming storage.
        /// </summary>
        private static readonly string RoamingProviderKey = "current_provider";

        /// <summary>
        /// The dictonary key for the app in roaming storage. 
        /// </summary>
        private static readonly string VaultKey = "CoffeeApp";
       

        /// <summary>
        /// Additional text to display on the AccountsSettingsPane. Used in event of error or other unexpected behavior. 
        /// </summary>
        private static string _paneMessage = "";

        /// <summary>
        /// Initializes authentication by instructing the AccountsSettingsPane to show the custom
        /// panel when requested. 
        /// </summary>
        static AuthenticationHelper()
        {
            AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested += BuildSettingsPaneAsync;
        }

        /// <summary>
        /// Logs out the current user and removes their credentials from storage. 
        /// </summary>
        public static void Logout()
        {
            var vault = new PasswordVault();
            ApplicationData.Current.RoamingSettings.Values.Remove(RoamingUserKey);
            ApplicationData.Current.RoamingSettings.Values.Remove(RoamingProviderKey);
            foreach (var credential in vault.FindAllByResource(VaultKey))
            {
                vault.Remove(credential);
            }
            App.User = null;
        }

        /// <summary>
        /// Creates an AccountsSettingsPane with options for MSA or Facebook login. 
        /// </summary>
        private static async void BuildSettingsPaneAsync(AccountsSettingsPane s,
            AccountsSettingsPaneCommandsRequestedEventArgs e)
        {
            var deferral = e.GetDeferral();
            e.HeaderText = _paneMessage + "Get signed in to coffee ux.  " +
                "You can use an existing Microsoftaccount.";

            // Microsoft account
            var msa = new WebAccountProviderCommand(
                await WebAuthenticationCoreManager.FindAccountProviderAsync("https://login.microsoft.com", "consumers"),
                MicrosoftAccountAuth);
            e.WebAccountProviderCommands.Add(msa);

        

            // Help commands
            e.Commands.Add(new SettingsCommand("help", "Get help signing in",
                async x => await Launcher.LaunchUriAsync(new Uri("http://bing.com"))));
            e.Commands.Add(new SettingsCommand("privacy", "Privacy policy",
                async x => await Launcher.LaunchUriAsync(new Uri("http://bing.com"))));

            deferral.Complete();
        }

        /// <summary>
        /// Obtains a MSA OAuth token using WebAccountManager and tries to log the user in with it.
        /// </summary>
        private static async void MicrosoftAccountAuth(WebAccountProviderCommand command)
        {
     
            WebTokenRequest request = new WebTokenRequest(command.WebAccountProvider, "wl.basic,wl.emails");
            WebTokenRequestResult result = await WebAuthenticationCoreManager.GetTokenSilentlyAsync(request);
            if (result.ResponseStatus == WebTokenRequestStatus.UserInteractionRequired)
            {
                result = await WebAuthenticationCoreManager.RequestTokenAsync(request);
            }
            if (result.ResponseStatus == WebTokenRequestStatus.ProviderError)
            {
                ReportFail();
                return;
            }
            await LoginOrRegisterAsync(new ProviderInfo
            {
       
                Token = result.ResponseData.FirstOrDefault()?.Token
            });
        }
        
       

        /// <summary>
        /// Attempts to log in or register the user using the given provider info.
        /// If successful, saves their credentials for future use.
        /// </summary>
        private static async Task LoginOrRegisterAsync(ProviderInfo info)
        {
            var exists = await App.Api.GetAsync<Data.User>("Users", info);
            string action = null != exists ? "Login" : "Register";

            var user = await App.Api.PostAsync<ProviderInfo, Data.User>(action, info);
            if (null != user && !String.IsNullOrWhiteSpace(user.Email))
            {
                UserLoggedIn?.Invoke(DateTime.Now, user);
                StoreUserInfo(user.Auth);
            }
        }

        /// <summary>
        /// Relaunches the AccountsSettingsPane with an error message that the last login attempt failed. 
        /// </summary>
        private static void ReportFail()
        {
            AuthError?.Invoke(null, null);
            if (String.IsNullOrEmpty(_paneMessage))
            {
                _paneMessage = "Whoops, something went wrong. Let's try again.\n\n";
            }
            AccountsSettingsPane.Show();
        }

        /// <summary>
        /// Stores the user's login info in roamingsettings and the password vault for auto-login on
        /// future app launches. 
        /// </summary>
        private static void StoreUserInfo(ProviderInfo request)
        {
            ApplicationData.Current.RoamingSettings.Values[RoamingUserKey] = request.Email;
            var cred = new PasswordCredential
            {
                UserName = request.Email,
                Password = request.Token,
                Resource = VaultKey
            };
            var vault = new PasswordVault();
            vault.Add(cred);
        }

        /// <summary>
        /// Checks if the user has credentials stored and tries to automatically log in with them. 
        /// </summary>
        public static async Task TryLogInSilently()
        {
            if (ApplicationData.Current.RoamingSettings.Values.ContainsKey(RoamingUserKey) &&
                ApplicationData.Current.RoamingSettings.Values.ContainsKey(RoamingProviderKey))
            {
                string username = ApplicationData.Current.RoamingSettings.Values[RoamingUserKey].ToString();
                var vault = new PasswordVault();
                var credential = vault.RetrieveAll().FirstOrDefault(x =>
                    x.Resource == VaultKey && x.UserName == username);
                if (null == credential)
                {
                    return;
                }
                // MSA tokens are short-lived, so the existing cached token is likely invalid
                // Silently request a new token
                if (ApplicationData.Current.RoamingSettings.Values[RoamingProviderKey].ToString() == "msa")
                {
                    var msaProvider = await WebAuthenticationCoreManager.FindAccountProviderAsync("https://login.microsoft.com", "consumers");
                    WebTokenRequest request = new WebTokenRequest(msaProvider, "wl.basic, wl.emails");
                    WebTokenRequestResult result = await WebAuthenticationCoreManager.GetTokenSilentlyAsync(request);
                    if (result.ResponseStatus == WebTokenRequestStatus.Success)
                    {
                        await LoginOrRegisterAsync(new ProviderInfo
                        {
                   
                            Token = result.ResponseData[0].Token
                        });
                    }
                }
                // Facebook and google tokens last 60 days, so we can probably re-log in with one
                // If not, it will fail and show the OOBE expereince
                else
                {

                    credential.RetrievePassword();
                    await LoginOrRegisterAsync(new ProviderInfo
                    {
                        Email = username,
                        Token = credential.Password
                    });
                }
            }
        }

        /// <summary>
        /// Raised when the user successfully logs in. 
        /// </summary>
        public static event EventHandler<Data.User> UserLoggedIn;

        /// <summary>
        /// Raised when an authentication error occurs. 
        /// </summary>
        public static event EventHandler AuthError;
    }
}
