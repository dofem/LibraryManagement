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
                        var borrowService = scope.ServiceProvider.GetRequiredService<IBorrowService>();
                        var smsNotificationService = scope.ServiceProvider.GetRequiredService<ISmsNotificationService>();

                        var overdueBorrowedBooks = await borrowService.GetOverdueBorrowedBooks();
                        foreach (var borrow in overdueBorrowedBooks)
                        {
                            var daysBeforeOverdue = (borrow.DueDate - DateTime.Now).Days;
                            if (daysBeforeOverdue == 2)
                            {
                                var message = $"Reminder: The book you borrowed is overdue in 2 days time. Please return it on or before {borrow.DueDate} to avoid a fine.";
                                smsNotificationService.SendSms(borrow.PhoneNumber, message);
                            }
                        }

                        foreach (var borrow in overdueBorrowedBooks)
                        {
                            var daysOverdue = (DateTime.Now - borrow.DueDate).Days;
                            if (daysOverdue > 0)
                            {
                                var lateFee = 500 * daysOverdue;
                                var message = $"Reminder: Your borrowed book is {daysOverdue} days overdue. A late fee of NGN{lateFee} has been added to your account.";
                                smsNotificationService.SendSms(borrow.PhoneNumber, message);
                            }
                        }

                        await Task.Delay(TimeSpan.FromDays(1), stoppingToken);

                    }
                }
                catch (Exception ex)
                {

                }


            }
        }
    }
}
                
