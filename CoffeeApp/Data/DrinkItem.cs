using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeeApp.Data
{
    public class DrinkItem
    {
        public string Title { get; set; }
        public string Summary { get; set; }
        public string HeroImage { get; set; }
        public string drinkImage { get; set; }
        public bool IsHero { get; set; } = false;

        public static List<DrinkItem> GetData()
        {
            var items = new List<DrinkItem>();
            items.Add(new DrinkItem()
            {
                Title = "Introducing the Flat White",
                HeroImage = "https://static.pexels.com/photos/29612/pexels-photo-29612.jpg",
                drinkImage = "https://globalassets.starbucks.com/assets/1ba88037116d4234807bce3ee442900e.jpg",
                Summary = "Bold ristretto shots of espresso get the perfect amount of steamed whole milk to create a not too strong, not too creamy, just right flavor.",
                IsHero = true
            });
            items.Add(new DrinkItem()
            {
                Title = "Caffè Americano",
                Summary = "Espresso shots are topped with hot water to produce a light layer of crema in true European style.",
                HeroImage = "https://globalassets.starbucks.com/assets/02e313dd98204b7380730e96f8d50c38.jpg"
            });
            items.Add(new DrinkItem()
            {
                Title = "Caffe Latte",
                Summary = "Our dark, rich espresso is balanced with steamed milk and topped with a light layer of foam.A perfect milk - forward warm up.",
                HeroImage = "https://globalassets.starbucks.com/assets/f6c298b781144d9d9042bc05f659dc70.jpg"
            });
            items.Add(new DrinkItem()
            {
                Title = "Cappuccino",
                Summary = "Our dark, rich espresso is balanced with steamed milk and topped with a light layer of foam.A perfect milk - forward warm up.",
                HeroImage = "https://globalassets.starbucks.com/assets/3f3c928b6db142999b4a8f2b0671afb0.jpg"
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
