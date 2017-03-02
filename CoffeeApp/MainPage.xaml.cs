
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


            Topics = DrinkItem.GetListOfTopics();
           // this.Loaded += MainPage_Loaded;
            this.InitializeComponent();
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            RootPage.Current.UpdateBackground(Items.First().HeroImage, 0);
            SectionList.SelectedIndex = 0;
        }

       
        // Define a member variable for storing the signed-in user. 
        private MobileServiceUser user;
        // Define a method that performs the authentication process
        // using a Facebook sign-in. 
        private async System.Threading.Tasks.Task<bool> AuthenticateAsync()
        {
            string message;
            bool success = false;
            try
            {
                // Change 'MobileService' to the name of your MobileServiceClient instance.
                // Sign-in using Facebook authentication.
                user = await App.MobileService
                    .LoginAsync(MobileServiceAuthenticationProvider.MicrosoftAccount);
                message =
                    string.Format("You are now signed in - {0}", user.UserId);
                var response = await App.MobileService.InvokeApiAsync("/.auth/me");
                //dynamic userDetails = JsonConvert.DeserializeObject(response);
                //App.User.Name = userDetails[0].user_Id;
                App.User.Id = user.UserId;
                
                App.MobileService.CurrentUser = user;

                success = true;
            }
            catch (InvalidOperationException)
            {
                message = "You must log in. Login Required";
            }

            var dialog = new MessageDialog(message);
            dialog.Commands.Add(new UICommand("OK"));
            await dialog.ShowAsync();
            return success;



            return success;
        }

        private async void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            // Login the user and then load data from the mobile app.
            if (await AuthenticateAsync())
            {
                // Switch the buttons and load items from the mobile app.
                ButtonLogin.Visibility = Visibility.Collapsed;

                this.NavigationCacheMode = NavigationCacheMode.Required;
                var items = DrinkItem.GetData();
                Items = items;
                this.Loaded += MainPage_Loaded;
                RootPage.Current.ImageCount = Items.Count;
            }
         
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
