using LibraryManagement.Model;
using LibraryManagement.Services.Interface;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;

namespace LibraryManagement.Services.Implementation
{
    public class TwilioNotificationService : ISmsNotificationService
    {
        private readonly TwilioSettings _twilioSettings;
        private readonly TwilioRestClient _twilioClient;

        public TwilioNotificationService(IOptions<TwilioSettings> twilioSettings)
        {
            _twilioSettings = twilioSettings.Value;

            TwilioClient.Init(_twilioSettings.AccountSid, _twilioSettings.AuthToken);
            _twilioClient = new TwilioRestClient(_twilioSettings.AccountSid, _twilioSettings.AuthToken);
        }

        public void SendSms(string toPhoneNumber, string message)
        {
            MessageResource.Create(
                body: message,
                from: new Twilio.Types.PhoneNumber(_twilioSettings.TwilioPhoneNumber),
                to: new Twilio.Types.PhoneNumber(toPhoneNumber)
            );
        }
    }

}
