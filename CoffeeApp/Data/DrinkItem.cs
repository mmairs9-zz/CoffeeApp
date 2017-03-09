using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace CoffeeApp.Data
{
    public class DrinkItem
    {
        public string Title { get; set; }
        public string HeroTitle { get; set; }
        public string Summary { get; set; }
        public string HeroImage { get; set; }
        public string drinkImage { get; set; }
        public bool IsHero { get; set; } = false;

        public static List<DrinkItem> GetData()
        {
            var items = new List<DrinkItem>();
            items.Add(new DrinkItem()
            {
                Title = "Flat White",
                HeroTitle = "Introducing the Flat White",
                HeroImage= "ms-appx:///Assets/Optimized-flatwhite.jpg",
                drinkImage = "https://globalassets.starbucks.com/assets/1ba88037116d4234807bce3ee442900e.jpg",
                Summary = "Bold ristretto shots of espresso get the perfect amount of steamed whole milk to create a not too strong, not too creamy, just right flavor.",
                IsHero = true
            });
            items.Add(new DrinkItem()
            {
                Title = "Caffè Americano",
                Summary = "Espresso shots are topped with hot water to produce a light layer of crema in true European style.",
                HeroImage = "ms-appx:///Assets/Optimized-americano.jpg"
            });
            items.Add(new DrinkItem()
            {
                Title = "Caffe Latte",
                Summary = "Our dark, rich espresso is balanced with steamed milk and topped with a light layer of foam.A perfect milk - forward warm up.",
                HeroImage = "ms-appx:///Assets/Optimized-latte.jpg"
            });
            items.Add(new DrinkItem()
            {
                Title = "Cappuccino",
                Summary = "Our dark, rich espresso is balanced with steamed milk and topped with a light layer of foam.A perfect milk - forward warm up.",
                HeroImage = "ms-appx:///Assets/Optimized-cappuciano.jpg"
            });
          

            return items;
        }

        public static List<string> GetListOfTopics()
        {
            var topics = new List<string>();
            topics.Add("Menu");
            return topics;
        }
    }
}
