
using CoffeeApp.Helpers.Composition;
using CoffeeApp.Helpers.Composition.ImageLoader;
using CoffeeApp.Controls;
using CoffeeApp.Data;
using CoffeeApp.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.UI;
using Windows.UI.Xaml.Media.Animation;
using Microsoft.WindowsAzure.MobileServices;
using Windows.UI.Popups;
using Windows.Security.Credentials;
using Windows.UI.ApplicationSettings;
using Windows.Security.Authentication.Web.Core;
using System.Text;
using CoffeApp.Helpers;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CoffeeApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {

        private string _MicrosoftAccountToken = null;
        private string _AppAuthToken = null;

        private const string MicrosoftAccountProviderId = "https://login.microsoft.com";
        private const string ConsumerAuthority = "consumers";
        private const string AccountScopeRequested = "wl.basic";
        private const string AccountClientId = "d4c5056c-ceb9-46b1-b69d-b3d3b5606898";
        private const string _KeyNameForMicrosoftAccount = "msa-used-by-uwp-sample";
        private const string _KeyNameForMicrosoftAccountToken = "msa-used-by-uwp-sample-token";

        private List<DrinkItem> items;

        public List<DrinkItem> Items
        {
            get { return items; }
            set { Set(ref items, value); }
        }

        private DrinkItem hero;

        public DrinkItem Hero
        {
            get { return hero; }
            set { Set(ref hero, value); }
        }

        List<string> Topics;
        bool _firstRun = true;
        public event PropertyChangedEventHandler PropertyChanged;

        public MainPage()
        {
            this.NavigationCacheMode = NavigationCacheMode.Required;


            this.Loaded += MainPage_Loaded;
            this.InitializeComponent();
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            AccountsSettingsPane pane = AccountsSettingsPane.GetForCurrentView();

            pane.AccountCommandsRequested += OnAccountCommandsRequested;
         //   RootPage.Current.UpdateBackground(Items.First().HeroImage, 0);
           // SectionList.SelectedIndex = 0;
        }

        private async void OnAccountCommandsRequested(
            AccountsSettingsPane sender,
            AccountsSettingsPaneCommandsRequestedEventArgs e)
        {
            // In order to make async calls within this callback, the deferral object is needed
            AccountsSettingsPaneEventDeferral deferral = e.GetDeferral();

            // This scenario only lets the user have one account at a time.
            // If there already is an account, we do not include a provider in the list
            // This will prevent the add account button from showing up.
            bool isPresent = ApplicationData.Current.LocalSettings.Values.ContainsKey(_KeyNameForMicrosoftAccount);

            if (isPresent)
            {
                await AddWebAccount(e);
            }
            else
            {
                await AddWebAccountProvider(e);
            }

            AddLinksAndDescription(e);

            deferral.Complete();
        }
        private async Task AddWebAccountProvider(AccountsSettingsPaneCommandsRequestedEventArgs e)
        {
            // FindAccountProviderAsync returns the WebAccountProvider of an installed plugin
            // The Provider and Authority specifies the specific plugin
            // This scenario only supports Microsoft accounts.

            // The Microsoft account provider is always present in Windows 10 devices, as is the Azure AD plugin.
            // If a non-installed plugin or incorect identity is specified, FindAccountProviderAsync will return null   
            WebAccountProvider provider = await WebAuthenticationCoreManager.FindAccountProviderAsync(MicrosoftAccountProviderId, ConsumerAuthority);

            WebAccountProviderCommand providerCommand = new WebAccountProviderCommand(provider, WebAccountProviderCommandInvoked);
            e.WebAccountProviderCommands.Add(providerCommand);
        }

        private async Task AddWebAccount(AccountsSettingsPaneCommandsRequestedEventArgs e)
        {
            WebAccountProvider provider = await WebAuthenticationCoreManager.FindAccountProviderAsync(MicrosoftAccountProviderId, ConsumerAuthority);

            String accountID = (String)ApplicationData.Current.LocalSettings.Values[_KeyNameForMicrosoftAccount];
            WebAccount account = await WebAuthenticationCoreManager.FindAccountAsync(provider, accountID);

            if (account == null)
            {
                // The account has most likely been deleted in Windows settings
                // Unless there would be significant data loss, you should just delete the account
                // If there would be significant data loss, prompt the user to either re-add the account, or to remove it
                ApplicationData.Current.LocalSettings.Values.Remove(_KeyNameForMicrosoftAccount);
            }

            WebAccountCommand command = new WebAccountCommand(account, WebAccountInvoked, SupportedWebAccountActions.Remove);
            e.WebAccountCommands.Add(command);
        }

        private void AddLinksAndDescription(AccountsSettingsPaneCommandsRequestedEventArgs e)
        {
            e.HeaderText = "Describe what adding an account to your application will do for the user";

            // You can add links such as privacy policy, help, general account settings
            e.Commands.Add(new SettingsCommand("privacypolicy", "Privacy policy", PrivacyPolicyInvoked));
            e.Commands.Add(new SettingsCommand("otherlink", "Other link", OtherLinkInvoked));
        }

        private async void WebAccountProviderCommandInvoked(WebAccountProviderCommand command)
        {
            // ClientID is ignored by MSA
            await RequestTokenAndSaveAccount(command.WebAccountProvider, AccountScopeRequested, AccountClientId);
        }

        private async void WebAccountInvoked(WebAccountCommand command, WebAccountInvokedArgs args)
        {
            if (args.Action == WebAccountAction.Remove)
            {
                // SetMessage("Removing account", NotifyType.StatusMessage);
                await LogoffAndRemoveAccount();
            }
        }

        private void PrivacyPolicyInvoked(IUICommand command)
        {
            // SetMessage("Privacy policy clicked by user", NotifyType.StatusMessage);
        }

        private void OtherLinkInvoked(IUICommand command)
        {
            // SetMessage("Other link pressed by user", NotifyType.StatusMessage);
        }

        private async Task RequestTokenAndSaveAccount(WebAccountProvider Provider, String Scope, String ClientID)
        {
            try
            {
                WebTokenRequest webTokenRequest = new WebTokenRequest(Provider, Scope, ClientID);

                WebTokenRequestResult webTokenRequestResult = await WebAuthenticationCoreManager.RequestTokenAsync(webTokenRequest);

                if (webTokenRequestResult.ResponseStatus == WebTokenRequestStatus.Success)
                {
                    ApplicationData.Current.LocalSettings.Values.Remove(_KeyNameForMicrosoftAccount);
                    ApplicationData.Current.LocalSettings.Values.Remove(_KeyNameForMicrosoftAccountToken);

                    ApplicationData.Current.LocalSettings.Values[_KeyNameForMicrosoftAccount] = webTokenRequestResult.ResponseData[0].WebAccount.Id;

                    _MicrosoftAccountToken = webTokenRequestResult.ResponseData[0].Token;

                    SaveMicrosoftAccountToken(_MicrosoftAccountToken);
                }

                OutputTokenResult(webTokenRequestResult);
            }
            catch (Exception ex)
            {
                SetMessage("Web Token request failed: " + ex.ToString());
            }
        }

        public string MicrosoftAccountToken
        {
            get
            {
                if (_MicrosoftAccountToken == null)
                {
                    _MicrosoftAccountToken = GetMicrosoftAccountToken();
                }

                return _MicrosoftAccountToken;
            }
        }


        private void OutputTokenResult(WebTokenRequestResult result)
        {
            if (result.ResponseStatus == WebTokenRequestStatus.Success)
            {
                SetMessage("Web Token request successful for user: " + result.ResponseData[0].WebAccount.UserName);
                ButtonLogin.Content = "Account";
            }
            else
            {
                var builder = new StringBuilder();

                builder.AppendLine("Web Token request error");

                Append(builder, result.ResponseData);
                Append(builder, result.ResponseError);
                Append(builder, result.ResponseStatus);


                // SetMessage("Web Token request error: " + result.ResponseError);
                SetMessage("Web Token request error: " + builder.ToString());
            }
        }

        private void Append(StringBuilder builder, WebTokenRequestStatus responseStatus)
        {
            builder.AppendLine("** WebTokenRequestStatus **");

            builder.AppendLine(responseStatus.ToString());
        }

        private void Append(StringBuilder builder, WebProviderError responseError)
        {
            builder.AppendLine("** WebProviderError **");

            builder.Append("Error Code: ");
            builder.AppendLine(responseError.ErrorCode.ToString());

            builder.Append("Error Message: ");
            builder.AppendLine(responseError.ErrorMessage.ToString());

            builder.Append("Properties Count: ");
            builder.AppendLine(responseError.Properties.Count.ToString());
        }

        private void Append(StringBuilder builder, IReadOnlyList<WebTokenResponse> responseData)
        {
            builder.AppendLine("** START IReadOnlyList<WebTokenResponse> **");

            foreach (var item in responseData)
            {
                builder.AppendLine("-- item --");

                builder.Append("Properties Count: ");
                builder.AppendLine(item.Properties.Count.ToString());

                builder.Append("Provider Error: ");
                builder.AppendLine(item.ProviderError.ToString());

                builder.Append("Token: ");
                builder.AppendLine(item.Token);

                builder.Append("WebAccount.Id: ");
                builder.AppendLine(item.WebAccount.Id.ToString());

                builder.Append("WebAccount.State: ");
                builder.AppendLine(item.WebAccount.State.ToString());
            }

            builder.AppendLine("** END IReadOnlyList<WebTokenResponse> **");
        }

        private void SetMessage(string message)
        {
            _TextboxMessages.Text = message;
        }

        private async Task LogoffAndRemoveAccount()
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(_KeyNameForMicrosoftAccount))
            {
                WebAccountProvider providertoDelete = await WebAuthenticationCoreManager.FindAccountProviderAsync(MicrosoftAccountProviderId, ConsumerAuthority);

                WebAccount accountToDelete = await WebAuthenticationCoreManager.FindAccountAsync(providertoDelete, (string)ApplicationData.Current.LocalSettings.Values[_KeyNameForMicrosoftAccount]);

                if (accountToDelete != null)
                {
                    await accountToDelete.SignOutAsync();
                }

                ApplicationData.Current.LocalSettings.Values.Remove(_KeyNameForMicrosoftAccount);
                ApplicationData.Current.LocalSettings.Values.Remove(_KeyNameForMicrosoftAccountToken);

                ButtonLogin.Content = "Sign in";
            }
        }

        private async void _ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;

            // SetMessage("Resetting...", NotifyType.StatusMessage);

            await LogoffAndRemoveAccount();

            ((Button)sender).IsEnabled = true;
        }

        private async void _ButtonCallServiceUsingClient_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(MicrosoftAccountToken) == true)
            {
                SetMessage("Are you logged in?  Token is null...not making call.");
            }
            else
            {
                var builder = new StringBuilder();

                builder.AppendLine(DateTime.Now.ToString());
                builder.AppendLine();

                 string baseUrl = "https://coffeeapp.azurewebsites.net";
               // string baseUrl = null;

                if (baseUrl == null)
                {
                    throw new InvalidOperationException("Hey! You didn't update the baseUrl variable with the value for *your* Azure App Service!");
                }

                string serviceCallUrl = "/api/Values";

                var client = new AzureAppServiceMicrosoftAccountClient(baseUrl, MicrosoftAccountToken);

                var result = await client.GetStringFromApplicationService(serviceCallUrl);

                builder.AppendLine(result);

                _TextboxResults.Text = builder.ToString();
            }
        }
        private void SaveMicrosoftAccountToken(string microsoftAccountToken)
        {
            new AccountTokenStorage().SaveMicrosoftAccountToken(microsoftAccountToken);
        }

        private string GetMicrosoftAccountToken()
        {
            return new AccountTokenStorage().GetMicrosoftAccountToken();
        }
        // Define a member variable for storing the signed-in user. 
        private MobileServiceUser user;
        
        private async void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            // Login the user and then load data from the mobile app.
            // Login the user and then load data from the mobile app.
            ((Button)sender).IsEnabled = false;

            AccountsSettingsPane.Show();

            ((Button)sender).IsEnabled = true;
        }
       
        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateTimelineItemsSize();
        }

        private void UpdateTimelineItemsSize()
        {
           
          
        }

     

        private void SetupCompositionImage(CompositionImage image, CoffeeApp.Helpers.Composition.CompositionShadow shadow)
        {
            

            var imageVisual = image.SpriteVisual;
            var compositor = imageVisual.Compositor;

            var imageLoader = ImageLoaderFactory.CreateImageLoader(compositor);
            var imageMaskSurface = imageLoader.CreateManagedSurfaceFromUri(new Uri("ms-appx:///Helpers/Composition/CircleMask.png"));

            var mask = compositor.CreateSurfaceBrush();
            mask.Surface = imageMaskSurface.Surface;

            var source = image.SurfaceBrush as CompositionSurfaceBrush;

            var maskBrush = compositor.CreateMaskBrush();
            maskBrush.Mask = mask;
            maskBrush.Source = source;

            image.Brush = maskBrush;
            shadow.Mask = maskBrush.Mask;
        }
        
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var shadow = (sender as FrameworkElement).FindChildren<CoffeeApp.Helpers.Composition.CompositionShadow>().FirstOrDefault();
            var image = (sender as FrameworkElement).FindChildren<CompositionImage>().FirstOrDefault();

            if (shadow == null || image == null)
                return;

            SetupCompositionImage(image, shadow);
        }

       

        ListViewItem previousFocus;

        private void SectionList_LostFocus(object sender, RoutedEventArgs e)
        {
            previousFocus = e.OriginalSource as ListViewItem;
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var index = Items.IndexOf(e.ClickedItem as DrinkItem);
            var children = (sender as ListView).FindChildren<ListViewItem>();

            var listViewItem = children.ElementAt(index);

            var titleLine = listViewItem.FindDescendantByName("TitleLine");
            var summaryLine = listViewItem.FindDescendantByName("SummaryLine");


            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("Title", titleLine);
            ConnectedAnimationService.GetForCurrentView().PrepareToAnimate("Summary", summaryLine);

            var story = e.ClickedItem as DrinkItem;

            RootPage.Current.UpdateBackground(story.HeroImage, 0);
            Frame.Navigate(typeof(DetailsPage), story);

        }

        private void Template_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateTemplateContentVisibility(sender);
        }

        private void Template_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateTemplateContentVisibility(sender);
        }

        private void UpdateTemplateContentVisibility(object sender)
        {
         
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            Splitter.IsPaneOpen = !Splitter.IsPaneOpen;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SectionTitle.Text = e.AddedItems.First() as string;
            Splitter.IsPaneOpen = false;
        }

        public void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (!Equals(storage, value))
            {
                storage = value;
                RaisePropertyChanged(propertyName);
            }
        }
    }
}
