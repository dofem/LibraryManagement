using LibraryManagement.Data;
using LibraryManagement.Dto.Request;
using LibraryManagement.Dto.Response;
using LibraryManagement.Entities;
using LibraryManagement.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LibraryManagement.Services
{
    public class BookService : IBookService
    {

        private ApplicationDbContext _context;

        public BookService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async  Task CreateAsync(Book entity)
        {
           await _context.Books.AddAsync(entity);
            await SaveAsync();

        }

        public async Task<Book> GetAsync(int id)
        {
            var book = await _context.Books.Where(u => u.Id == id).FirstOrDefaultAsync();
            return book;
        }

        public async Task<List<Book>> GetAllAsync()
        {
          
            return await _context.Books.ToListAsync();
        }

        public async Task RemoveAsync(Book entity)
        {
            _context.Books.Remove(entity);
            await SaveAsync();
        }

        public async Task Update(Book book)
        {
            _context.Books.Update(book);
            await SaveAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }


        public async Task<List<Book>> GetAllBooksAvailableForBorrowAsync()
        {
            return await _context.Books.Where(b => b.IsAvailable == true).ToListAsync();
        }

        public async Task UpdateIsAvailableStatusAsync(int bookId)
        {
            var book = await _context.Books.FindAsync(bookId);
            book.IsAvailable = true;
            _context.Books.Update(book);
            await SaveAsync();
        }

        public async Task<Book> GetByIsbn(string Isbn)
        {
            var book = await _context.Books.Where(u=>u.Isbn == Isbn).FirstOrDefaultAsync();
            return book;
        }
    }
}
