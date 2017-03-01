using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace CoffeApp.Helpers
{
    public class AccountTokenStorage
    {

        private const string _KeyName = "MicrosoftAccountToken";

        public void SaveMicrosoftAccountToken(string token)
        {
            var vault = new PasswordVault();

            PasswordCredential credential = null;

            try
            {
                // Try to get an existing credential from the vault.
                credential = vault.FindAllByResource(_KeyName).FirstOrDefault();
            }
            catch (Exception)
            {
                // When there is no matching resource an error occurs, which we ignore.
            }

            if (credential != null)
            {
                vault.Remove(credential);
            }

            credential = new PasswordCredential(_KeyName, "username-not-used", token);

            vault.Add(credential);
        }

        public string GetMicrosoftAccountToken()
        {
            var vault = new PasswordVault();

            PasswordCredential credential = null;

            try
            {
                // Try to get an existing credential from the vault.
                credential = vault.FindAllByResource(_KeyName).FirstOrDefault();
            }
            catch (Exception)
            {
                // When there is no matching resource an error occurs, which we ignore.
            }

            if (credential != null)
            {
                credential.RetrievePassword();

                return credential.Password;
            }
            else
            {
                return null;
            }
        }
    }
}
