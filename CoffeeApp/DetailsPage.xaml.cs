using Microsoft.Toolkit.Uwp.UI.Animations;
using CoffeeApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using LightBuzz.SMTP;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using CoffeApp.Data;
using Windows.UI.Popups;
using Windows.ApplicationModel.Email;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CoffeeApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DetailsPage : Page
    {
        List<DrinkItem> Items;
        DrinkItem Item;
        OrderItem item;


        private bool section3FirstTime = true;
        private bool section4FirstTime = true;
        private bool relatedsectionFirstTime = true;
        DispatcherTimer videoControlsTimer;

        bool IsFullscreen = false;

        List<AnimatableSection> animatableSections = new List<AnimatableSection>();

        public DetailsPage()
        {
            Items = DrinkItem.GetData().Take(6).ToList();
            this.InitializeComponent();
            Window.Current.SizeChanged += Current_SizeChanged;
            CoreWindow.GetForCurrentThread().KeyDown += DetailsPage_KeyDown;

            Loaded += DetailsPage_Loaded;
            
            UpdateSize(new Size(Window.Current.Bounds.Width, Window.Current.Bounds.Height));

            RootElement.ViewChanged += RootElement_ViewChanged;
            myProfile.Source = App.User.PhotoUrl;

            animatableSections.Add(new AnimatableSection(Section1, Section1Animate));
        }

        private void DetailsPage_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
          
        }

        private void DetailsPage_Loaded(object sender, RoutedEventArgs e)
        {
            AnimateSections();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var item = e.Parameter as DrinkItem;
            if (item != null)
            {
                Item = item;
            }

            var animationService = ConnectedAnimationService.GetForCurrentView();

            var titleAnimation = animationService.GetAnimation("Title");
            if (titleAnimation != null)
            {
                titleAnimation.TryStart(TitleLine);
            }

            var summaryAnimation = animationService.GetAnimation("Summary");
            if (summaryAnimation != null)
            {
                summaryAnimation.TryStart(SummaryText);
            }

           
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
           
            base.OnNavigatingFrom(e);
        }

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            UpdateSize(e.Size);
            AnimateSections();
        }

        private void UpdateSize(Size size)
        {
            HeroGrid.Height = size.Height;
            Section1.Height = size.Height - 100;
        }

        private void RootElement_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            AnimateSections();
        }

        private void AnimateSections()
        {
            foreach (var section in animatableSections)
            {
                if (IsVisibileToUser(section.Element, RootElement))
                {
                    section.Animate();
                }
            }
        }

        private bool IsVisibileToUser(FrameworkElement element, FrameworkElement container)
        {
            if (element == null || container == null)
                return false;

            if (element.Visibility != Visibility.Visible)
                return false;

            Rect elementBounds = element.TransformToVisual(container).TransformBounds(new Rect(0.0, 0.0, element.ActualWidth, element.ActualHeight));
            Rect containerBounds = new Rect(0.0, 0.0, container.ActualWidth, container.ActualHeight);

            return (elementBounds.Height - (containerBounds.Height - elementBounds.Top)) < (elementBounds.Height / 2);
        }

        private void Section1Animate()
        {
          
            GlifMain.Offset(offsetY: 20, duration: 0).Then().Fade(1).Offset().Start();
        }

        async
        private void OrderButton_Click(object sender, RoutedEventArgs e)
        {
            spinner.IsActive = true;
            string userID = App.User.Id;
             item = new OrderItem
            {
                userId = userID,
                userName = App.User.Name,
                drinkName = Item.Title,
                shots = int.Parse(shotCount.Text),
                size = selectedSize,
                milkType = selectedMilk,
                syrup = selectedSyrup,
                extraFoam = (bool)ExtraFoam.IsChecked,
                roomForMilk = (bool)RoomForMilk.IsChecked,
                takeout = (bool)Takeout.IsChecked,
                decaf = (bool)decaf.IsChecked,

            };
            await App.MobileService.GetTable<OrderItem>().InsertAsync(item);
            await sendEmail();
            var dialog = new MessageDialog("Your Order has been recieved. We will let you know once it is ready");
            dialog.Commands.Add(new UICommand("OK"));
            await dialog.ShowAsync();
            spinner.IsActive = false;
            Frame.Navigate(typeof(MainPage));
        }

        private async Task sendEmail()
        {
            using (SmtpClient client = new SmtpClient("smtp.gmail.com", 465, true, "coffeeux@gmail.com", "manuelseaz"))
            {
                EmailMessage emailMessage = new EmailMessage();

                emailMessage.To.Add(new EmailRecipient("mmairs9@gmail.com"));
                emailMessage.To.Add(new EmailRecipient("brenda_walsh@outlook.com"));
                emailMessage.To.Add(new EmailRecipient("manuelsaez@hotmail.com")); 
                emailMessage.Subject = "New Coffee order";
                emailMessage.Body = App.User.Name + " has made a coffee order! \n\nOrder Details:\n"
                    + "Drink: " + item.drinkName + "\n"
                     + "Size: " + item.size + "\n"
                + "Decaf: " + item.decaf + "\n"
                 + "Milk: " + (item.milkType == null ? "n/a" : item.milkType) + "\n"
                   + "Syrup: " + (item.syrup == null ? "n/a" : item.syrup  )+"\n"
                     + "Shots: " + item.shots + "\n"
                       + "Room for Milk: " + item.roomForMilk + "\n"
                          + "Take Out: " + item.takeout + "\n"
                             + "Extra Foam: " + item.extraFoam + "\n\n"
                               + "Completing the order?\n\n"
                + "To complete order please visit https://coffeeappux.azurewebsites.net/swagger/ui/ "
                   + "open the patch section and paste the order id: "+item.Id  +" into the request id field."
                +" Then in the request body paste the following {\"completed\":true} and scroll to the bottom of the section and click the \"Try it out\" Button. " 
                +"This will complete the order and send a push notification to inform the customer that their drink is ready for collection.";

                await client.SendMailAsync(emailMessage);
            }
        }

    

        private void AddShotButton_Click(object sender, RoutedEventArgs e)
        {
            
            if (int.Parse(shotCount.Text)>=5)
            {
                return;
               
            }
            else
            {
                shotCount.Text = (int.Parse(shotCount.Text) + 1).ToString();
            }
        }

        private void RemoveShotButton_Click(object sender, RoutedEventArgs e)
        {
       
            if (int.Parse(shotCount.Text) ==1)
            {
                return;
            }
            else
            {
                shotCount.Text = (int.Parse(shotCount.Text) - 1).ToString();
            }
          
        }
        string selectedMilk;
        private void milkRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton ck = sender as RadioButton;
            if (ck.IsChecked.Value)
                selectedMilk = ck.Content.ToString();
        }
        string selectedSyrup;
        private void syrupRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton ck = sender as RadioButton;
            if (ck.IsChecked.Value)
                selectedSyrup = ck.Content.ToString();
        }
        string selectedSize = "Regular";
        private void sizeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton ck = sender as RadioButton;
            if (ck.IsChecked.Value)
                selectedSize = ck.Content.ToString();
        }
    }
   
    internal class AnimatableSection
    {
        public FrameworkElement Element { get; set; }
        private Action Animation { get; set; }

        private bool _alreadyAnimated = false;

        public AnimatableSection(FrameworkElement element, Action animation)
        {
            Element = element;
            Animation = animation;
        }

        public void Animate()
        {
            if (_alreadyAnimated) return;

            _alreadyAnimated = true;
            Animation?.Invoke();
        }
    }
}
