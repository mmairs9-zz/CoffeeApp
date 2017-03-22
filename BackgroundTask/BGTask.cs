using LightBuzz.SMTP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Email;
using Windows.UI.Notifications;

namespace BackgroundTask
{
    public sealed class BGTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral;
        string[] OrderDetails;
        
        async
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            var details = taskInstance.TriggerDetails as ToastNotificationActionTriggerDetail;
            if (!details.Argument.Equals("no"))
            {
                OrderDetails = details.Argument.Split(';');
                var input = details.UserInput;
                await sendEmail();
            }
            _deferral.Complete();
            // await App.MobileService.GetTable<OrderItem>().(item);

        }

        private async Task sendEmail()
        {
            using (SmtpClient client = new SmtpClient("smtp.gmail.com", 465, true, "coffeeux@gmail.com", "manuelseaz"))
            {
                EmailMessage emailMessage = new EmailMessage();

                emailMessage.To.Add(new EmailRecipient("mmairs9@gmail.com"));
                emailMessage.To.Add(new EmailRecipient("brenda_walsh@outlook.com"));
                emailMessage.To.Add(new EmailRecipient("manuelsaez@hotmail.com"));
                emailMessage.Subject = "New Coffee order v2";
                emailMessage.Body = OrderDetails[0]+" has made a coffee order! \n\nThey have requested their usual order:\n"
                     + "Drink: " + OrderDetails[2] + "\n"
                      + "Size: " + OrderDetails[3] + "\n"
                 + "Decaf: " + OrderDetails[4] + "\n"
                  + "Milk: " + (OrderDetails[5] == "null" ? "n/a" : OrderDetails[5]) + "\n"
                    + "Syrup: " + (OrderDetails[6] == "null" ? "n/a" : OrderDetails[6]) + "\n"
                      + "Shots: " + OrderDetails[7] + "\n"
                        + "Room for Milk: " + OrderDetails[8] + "\n"
                           + "Take Out: " + OrderDetails[9] + "\n"
                              + "Extra Foam: " + OrderDetails[10] + "\n\n"
                                    + "Starting the order?\n\n"
                 + "To start the order please visit https://coffeeappux.azurewebsites.net/swagger/ui/ "
                    + "open the patch section and paste the order id: " + OrderDetails[1] + " into the request id field."
                 + " Then in the request body paste the following {\"status\":\"In Progress\"} and scroll to the bottom of the section and click the \"Try it out\" Button. "
                 + "This will start the order and send a push notification to inform the customer that their drink is being prepared for collection. \n\n"
                 + "Completing the order?\n\n"
                 + " In the request body field past the following {\"status\":\"Completed\"} and scroll to the bottom of the section and click the \"Try it out\" Button. "
                 + "This will complete the order and send a push notification to inform the customer that their drink is ready for collection.";


                await client.SendMailAsync(emailMessage);
            }
        }
    }
}
