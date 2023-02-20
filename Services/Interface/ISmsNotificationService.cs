namespace LibraryManagement.Services.Interface
{
   
        public interface ISmsNotificationService
        {
            void SendSms(string phoneNumber, string message);
        }

    
}
