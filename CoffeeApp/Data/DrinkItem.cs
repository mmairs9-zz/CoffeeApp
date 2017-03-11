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

        public static List<DrinkItem> GetCoffeeDrinks()
        {
            var items = new List<DrinkItem>();
            items.Add(new DrinkItem()
            {
                Title = "Flat White",
                HeroTitle = "Introducing the Flat White",
                HeroImage= "ms-appx:///Assets/Optimized-flatwhite.jpg",
                drinkImage = "https://globalassets.starbucks.com/assets/1ba88037116d4234807bce3ee442900e.jpg",
                Summary = "A double-shot espresso finished off with silky milk all in an 8oz cup. Serious coffee!",
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
            items.Add(new DrinkItem()
            {
                Title = "Espresso",
                Summary = "",
                HeroImage = "ms-appx:///Assets/Optimized-espresso.jpg"
            });
            items.Add(new DrinkItem()
            {
                Title = "Macchiato",
                Summary = "",
                HeroImage = "ms-appx:///Assets/Optimized-macchiato.jpg"
            });


            return items;
        }
        public static List<DrinkItem> GetChocolateDrinks()
        {
            var items = new List<DrinkItem>();
            items.Add(new DrinkItem()
            {
                Title = "Mocha",
                Summary = "",
                HeroImage = "ms-appx:///Assets/Optimized-mocha.jpg"
            });
            items.Add(new DrinkItem()
            {
                Title = "White Mocha",
                Summary = "",
                HeroImage = "ms-appx:///Assets/Optimized-mocha.jpg"
            });
            items.Add(new DrinkItem()
            {
                Title = "OSO Caramelli",
                Summary = "Latte, Caramel Syrup, Caramel Drizzle",
                HeroImage = "ms-appx:///Assets/Optimized-hotChocolate.jpg"
            });
            items.Add(new DrinkItem()
            {
                Title = "Hot Chocolate",
                Summary = "",
                HeroImage = "ms-appx:///Assets/Optimized-hotChocolate.jpg"
            });
            items.Add(new DrinkItem()
            {
                Title = "Hot White Chocolate",
                Summary = "",
                HeroImage = "ms-appx:///Assets/Optimized-hotChocolate.jpg"
            });
            items.Add(new DrinkItem()
            {
                Title = "Macchiato",
                Summary = "",
                HeroImage = "ms-appx:///Assets/Optimized-hotChocolate.jpg"
            });


            return items;
        }
        public static List<DrinkItem> GetTeaDrinks()
        {
            var items = new List<DrinkItem>();
            items.Add(new DrinkItem()
            {
                Title = "Regular Tea",
                Summary = "",
                HeroImage = "ms-appx:///Assets/Optimized-englishTea.jpg"
            });
            items.Add(new DrinkItem()
            {
                Title = "Yunnan Tea",
                Summary = "",
                HeroImage = "ms-appx:///Assets/Optimized-chamolieTea.jpg"
            });
            items.Add(new DrinkItem()
            {
                Title ="Lemon Green Tea",
                Summary = "",
                HeroImage = "ms-appx:///Assets/Optimized-lemonTea.jpg"
            });
            items.Add(new DrinkItem()
            {
                Title = "White Peony Tea",
                Summary = "",
                HeroImage = "ms-appx:///Assets/Optimized-redTea.jpg"
            });
            items.Add(new DrinkItem()
            {
                Title = "Peppermint Tea",
                Summary = "",
                HeroImage = "ms-appx:///Assets/Optimized-chamolieTea.jpg"
            });
            items.Add(new DrinkItem()
            {
                Title = "Camillo Tea",
                Summary = "",
                HeroImage = "ms-appx:///Assets/Optimized-chamolieTea.jpg"
            });
            items.Add(new DrinkItem()
            {
                Title = "Rooiboos Tea",
                Summary = "",
                HeroImage = "ms-appx:///Assets/Optimized-redTea.jpg"
            });
            items.Add(new DrinkItem()
            {
                Title = "Wild Berry Tea",
                Summary = "",
                HeroImage = "ms-appx:///Assets/Optimized-chamolieTea.jpg"
            });
            items.Add(new DrinkItem()
            {
                Title = "Blood Orange Tea",
                Summary = "",
                HeroImage = "ms-appx:///Assets/Optimized-redTea.jpg"
            });
            items.Add(new DrinkItem()
            {
                Title = "Liquorice Tea",
                Summary = "",
                HeroImage = "ms-appx:///Assets/Optimized-chamolieTea.jpg"
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
