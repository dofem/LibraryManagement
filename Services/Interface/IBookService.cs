using LibraryManagement.Data;
using LibraryManagement.Dto.Request;
using LibraryManagement.Dto.Response;
using LibraryManagement.Entities;
using System.Linq.Expressions;

namespace LibraryManagement.Services.Interface
{
    public interface IBookService 
    {
        Task<List<Book>> GetAllAsync(Expression<Func<Book,bool>> filter = null);
        Task<Book> GetAsync(Expression<Func<Book,bool>> filter = null, bool tracked = true);
        Task CreateAsync(Book entity);
        Task Update(Book book);
        Task RemoveAsync(Book entity);
        Task SaveAsync();
    }

}
