
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
using Newtonsoft.Json;
using Windows.System;
using System.Net.Http;
using Windows.Data.Json;

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


            Topics = DrinkItem.GetListOfTopics();
            this.Loaded += MainPage_Loaded;
            this.InitializeComponent();
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {

            RootPage.Current.UpdateBackground(DrinkItem.GetData().First().HeroImage.ToString(), 0);
            SectionList.SelectedIndex = 0;
        }

       
        // Define a member variable for storing the signed-in user. 
        private MobileServiceUser user;

        public static WebAccount account;
        // Define a method that performs the authentication process
        // using a Facebook sign-in. 
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested += BuildPaneAsync;
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested -= BuildPaneAsync;
        }
       
      
        private async void GetMsaTokenAsync(WebAccountProviderCommand command)
        {
            WebTokenRequest request = new WebTokenRequest(command.WebAccountProvider, "wl.basic");
            WebTokenRequestResult result = await WebAuthenticationCoreManager.RequestTokenAsync(request);

            if (result.ResponseStatus == WebTokenRequestStatus.Success)
            {
                account = result.ResponseData[0].WebAccount;
                StoreWebAccount(account);
                await getUserData(result);
                LoadPageData();

            }
        }
        private async Task getUserData(WebTokenRequestResult result)
        {
            string token = result.ResponseData[0].Token;

           
            var restApi = new Uri(@"https://apis.live.net/v5.0/me?access_token=" + token);

            using (var client = new HttpClient())
            {
                var infoResult = await client.GetAsync(restApi);
                string content = await infoResult.Content.ReadAsStringAsync();

                var jsonObject = JsonObject.Parse(content);
                string id = jsonObject["id"].GetString();
                App.User.Id = id;
                string name = jsonObject["name"].GetString();

                App.User.Name = name;

                GetUserProfileImageAsync(token);
            }

            App.InitNotificationsAsync();
            LoadPageData();
        }
        private async void GetUserProfileImageAsync(String token)
        {
            var photoApi = new Uri(@"https://apis.live.net/v5.0/me/picture?access_token=" + token);
            using (var client = new HttpClient())
            {
                var photoResult = await client.GetAsync(photoApi);
                using (var imageStream = await photoResult.Content.ReadAsStreamAsync())
                {
                    App.User.PhotoUrl = new BitmapImage();
                    using (var randomStream = imageStream.AsRandomAccessStream())
                    {
                        await App.User.PhotoUrl.SetSourceAsync(randomStream);
                        myProfile.Source = App.User.PhotoUrl;
                        placeHolderImage.Visibility = Visibility.Collapsed;
                    }

                }
            }
        }
        private async void StoreWebAccount(WebAccount account)
        {
            ApplicationData.Current.LocalSettings.Values["CurrentUserProviderId"] = account.WebAccountProvider.Id;
            ApplicationData.Current.LocalSettings.Values["CurrentUserId"] = account.Id;
        }
        private async Task<string> GetTokenSilentlyAsync()
        {
            string providerId = ApplicationData.Current.LocalSettings.Values["CurrentUserProviderId"]?.ToString();
            string accountId = ApplicationData.Current.LocalSettings.Values["CurrentUserId"]?.ToString();

            if (null == providerId || null == accountId)
            {
                AccountsSettingsPane.Show();
                return null;
            }

            WebAccountProvider provider = await WebAuthenticationCoreManager.FindAccountProviderAsync(providerId);
            account = await WebAuthenticationCoreManager.FindAccountAsync(provider, accountId);

            WebTokenRequest request = new WebTokenRequest(provider, "wl.basic");

            WebTokenRequestResult result = await WebAuthenticationCoreManager.GetTokenSilentlyAsync(request, account);
            if (result.ResponseStatus == WebTokenRequestStatus.UserInteractionRequired)
            {
                // Unable to get a token silently - you'll need to show the UI
                AccountsSettingsPane.Show();
                return null;
            }
            else if (result.ResponseStatus == WebTokenRequestStatus.Success)
            {
                await getUserData(result);
                return result.ResponseData[0].Token;
            }
            else
            {
                // Other error 
                return null;
            }
        }
        private async Task SignOutAccountAsync()
        {
            ApplicationData.Current.LocalSettings.Values.Remove("CurrentUserProviderId");
            ApplicationData.Current.LocalSettings.Values.Remove("CurrentUserId");
            account.SignOutAsync();
        }
        private async void BuildPaneAsync(AccountsSettingsPane s,
    AccountsSettingsPaneCommandsRequestedEventArgs e)
        {
            var deferral = e.GetDeferral();

            var msaProvider = await WebAuthenticationCoreManager.FindAccountProviderAsync(
                "https://login.microsoft.com", "consumers");
            e.HeaderText = "Coffee UX will only work if you're signed in.";
            var command = new WebAccountProviderCommand(msaProvider, GetMsaTokenAsync);
            var settingsCmd = new SettingsCommand(
       "settings_privacy",
       "Privacy policy",
       async (x) => await Launcher.LaunchUriAsync(new Uri(@"https://privacy.microsoft.com/en-US/")));

            e.Commands.Add(settingsCmd);
            e.WebAccountProviderCommands.Add(command);

            deferral.Complete();
        }
       
        private async void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
           
            await GetTokenSilentlyAsync();
            
        }
        private async void ButtonLogout_Click(object sender, RoutedEventArgs e)
        {

            await SignOutAccountAsync();
            ResetPageData();

        }
        private void ResetPageData()
        {
            ButtonLogin.Visibility = Visibility.Visible;
             ButtonLogout.Visibility = Visibility.Collapsed;
            this.NavigationCacheMode = NavigationCacheMode.Required;
            myProfile.Source = new BitmapImage();
            placeHolderImage.Visibility = Visibility.Visible;
            Items = new List<DrinkItem>();
            this.Loaded += MainPage_Loaded;
            RootPage.Current.ImageCount = 0;
        }


        private void LoadPageData()
        {
            ButtonLogin.Visibility = Visibility.Collapsed;
            ButtonLogout.Visibility = Visibility.Visible;
            this.NavigationCacheMode = NavigationCacheMode.Required;
            var items = DrinkItem.GetData();
            Items = items;
            this.Loaded += MainPage_Loaded;
            RootPage.Current.ImageCount = Items.Count;
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

        async
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

             await RootPage.Current.UpdateBackground(story.HeroImage.ToString(), 0);
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
