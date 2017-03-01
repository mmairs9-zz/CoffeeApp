using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeApp.Data
{
    public class User
    {
        /// <summary>
        /// The user's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The user's email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The user's IDP info. 
        /// </summary>
        public ProviderInfo Auth { get; set; }
        

        /// <summary>
        /// URL to the user's IDP photo.
        /// </summary>
        public string PhotoUrl { get; set; }
    }
}
