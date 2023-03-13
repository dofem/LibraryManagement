using LibraryManagement.Entities;
using LibraryManagement.Services.Interface;

namespace LibraryManagement.Services.Implementation
{
    public class SmsNotificationService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<SmsNotificationService> _logger;

        public SmsNotificationService(IServiceScopeFactory serviceScopeFactory, ILogger<SmsNotificationService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        // Perform background tasks
                        var borrowService = scope.ServiceProvider.GetRequiredService<IBorrowService>();
                        var smsNotificationService = scope.ServiceProvider.GetRequiredService<ISmsNotificationService>();

                        var overdueBorrowedBooks = await borrowService.GetOverdueBorrowedBooks();
                        foreach (var borrow in overdueBorrowedBooks)
                        {
                            // Send reminder messages
                            var daysBeforeOverdue = (borrow.DueDate - DateTime.Now).Days;
                            if (daysBeforeOverdue == 2)
                            {
                                var message = $"Reminder: The book you borrowed is overdue in 2 days time. Please return it on or before {borrow.DueDate} to avoid a fine.";
                                smsNotificationService.SendSms(borrow.PhoneNumber, message);
                            }

                            // Charge late fees
                            var daysOverdue = (DateTime.Now - borrow.DueDate).Days;
                            if (daysOverdue > 0)
                            {
                                var lateFee = 500 * daysOverdue;
                                var message = $"Reminder: Your borrowed book is {daysOverdue} days overdue. A late fee of NGN{lateFee} has been added to your account.";
                                smsNotificationService.SendSms(borrow.PhoneNumber, message);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions
                }

                // Wait for the specified interval before running the loop again
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }



    }
}
    

                
