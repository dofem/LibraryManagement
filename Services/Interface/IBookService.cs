using LibraryManagement.Data;
using LibraryManagement.Dto.Request;
using LibraryManagement.Dto.Response;
using LibraryManagement.Entities;
using System.Linq.Expressions;

namespace LibraryManagement.Services.Interface
{
    public interface IBookService 
    {
        Task<List<Book>> GetAllAsync();
        Task<Book> GetAsync(int id);
        Task<Book> GetByIsbn(string Isbn);
        Task CreateAsync(Book entity);
        Task Update(Book book);
        Task<List<Book>> GetAllBooksAvailableForBorrowAsync();
        Task UpdateIsAvailableStatusAsync(int bookId);
        Task RemoveAsync(Book entity);
        Task SaveAsync();
    }

}
