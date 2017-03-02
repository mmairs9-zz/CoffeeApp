using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeApp.Data
{
    public class OrderItem
    {
        public string Id { get; set; }
        public string userId { get; set; }
        public string drinkName { get; set; }
        public string size { get; set; }
        public int shots { get; set; }
        public bool decaf { get; set; }
        public string milkType { get; set; }
        public string syrup { get; set; }
        public bool roomForMilk { get; set; }
        public bool extraFoam { get; set; }
        public bool takeout { get; set; }
    }
}
