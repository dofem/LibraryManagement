using AutoMapper;
using LibraryManagement.Data;
using LibraryManagement.Dto.Request;
using LibraryManagement.Dto.Response;
using LibraryManagement.Entities;
using LibraryManagement.ExceptionHandler;
using LibraryManagement.Services.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using System.Data.Entity;
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
        private IMapper _mapper;

        public BorrowService(ApplicationDbContext context,IBookService bookService, UserManager<IdentityUser> userManager,IEncryptionService encryptionService,IMapper mapper)
        {
            _context = context;
            _bookService = bookService;
            _userManager = userManager;
            _encryptionService = encryptionService;
            _mapper = mapper;
        }

        public async Task<BorrowedBookResponse> CreateAsync(BorrowBook entity)
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
            string maskedPhoneNumber = MaskPhoneNumber(entity.PhoneNumber);

            Borrow borrow = new Borrow()
            {
                BorrowDate= DateTime.Now,
                BookId= book.Id,
                UserId= int.Parse(encryptedUserId),
                DueDate= DateTime.Now.AddDays(14),
                PhoneNumber= maskedPhoneNumber
            };
            await _context.Borrows.AddAsync(borrow);
            var borrowed = _mapper.Map<BorrowedBookResponse>(borrow);
            await SaveAsync();

            // update the availability status of the book
            book.IsAvailable = false;
            await _bookService.Update(book);


            return borrowed;

            
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

        public async Task<List<Borrow>> GetOverdueBorrowedBooks()
        {
            return await _context.Borrows
            .Where(b => b.DueDate < DateTime.Now)
            .ToListAsync();
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
