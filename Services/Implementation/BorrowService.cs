using LibraryManagement.Data;
using LibraryManagement.Dto.Request;
using LibraryManagement.Entities;
using LibraryManagement.ExceptionHandler;
using LibraryManagement.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace LibraryManagement.Services.Implementation
{
    public class BorrowService : IBorrowService
    {
        private ApplicationDbContext _context;
        private IBookService _bookService;
        private UserManager<IdentityUser> _userManager;
        private IEncryptionService _encryptionService;

        public BorrowService(ApplicationDbContext context,IBookService bookService, UserManager<IdentityUser> userManager,IEncryptionService encryptionService)
        {
            _context = context;
            _bookService = bookService;
            _userManager = userManager;
            _encryptionService = encryptionService;
        }

        public async Task CreateAsync(BorrowBook entity)
        {
            // check if the book is available
            Book book =await _bookService.GetAsync(u => u.Id == entity.BookId);   
            if (!book.IsAvailable)
            {
                throw new BookAlreadyBorrowedException("The book is already borrowed");
            }

            // create a new borrow record
            var user = _userManager.FindByIdAsync(entity.UserId.ToString()).Result;
            if (user == null)
            {
                throw new Exception("User not found");
            }

            // encrypt the user ID
            string encryptedUserId = _encryptionService.Encrypt(user.Id);

            // mask the user's phone number
            string maskedPhoneNumber = MaskPhoneNumber(user.PhoneNumber);

            Borrow borrow = new Borrow()
            {
                BorrowDate= DateTime.Now,
                BookId= book.Id,
                UserId= int.Parse(encryptedUserId),
                DueDate= DateTime.Now.AddDays(14),
            };
            await _context.Borrows.AddAsync(borrow);
            await SaveAsync();

            // update the availability status of the book
            book.IsAvailable = false;
            _bookService.Update(book);

            return borrow;

            
        }

        public async Task<List<Borrow>> GetAllAsync(Expression<Func<Borrow, bool>> filter = null)
        {
            IQueryable<Borrow> query = _context.Borrows;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return await query.ToListAsync();
        }

        public async Task<Borrow> GetAsync(Expression<Func<Borrow, bool>> filter = null, bool tracked = true)
        {
            IQueryable<Borrow> query = _context.Borrows;
            //if(!tracked) 
            //{
            //    query = query.AsNoTracking();
            //}
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return await query.FirstOrDefaultAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }


        private string MaskPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                return string.Empty;
            }

            int digitsToKeep = 4;
            int digitsToMask = phoneNumber.Length - digitsToKeep;
            string mask = new string('*', digitsToMask);
            string maskedPhoneNumber = mask + phoneNumber[^digitsToKeep..];

            return maskedPhoneNumber;
        }


    }
}
