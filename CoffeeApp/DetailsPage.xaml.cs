using Microsoft.Toolkit.Uwp.UI.Animations;
using CoffeeApp.Data;
using CoffeeApp.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using CoffeApp.Data;

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

            var likesAnimation = animationService.GetAnimation("Likes");
            if (likesAnimation != null)
            {
                likesAnimation.TryStart(LikesStack);
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
            OrderItem item = new OrderItem
            {
                Text = "Awesome item",
                Complete = false
            };
            await App.MobileService.GetTable<OrderItem>().InsertAsync(item);
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
