using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoffeApp.Data
{
    /// <summary>
    /// Provides information about authentication with an identity provider (IDP). 
    /// </summary>
    public class ProviderInfo 
    {
        /// <summary>
        /// The IDP account Id.
        /// </summary>
        public string AccountId { get; set; }
       

        /// <summary>
        /// The user's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The user's email.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The url to the user's photo.
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// The provider token.
        /// </summary>
        public string Token { get; set; }
    }
}
