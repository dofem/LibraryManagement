using LibraryManagement.Dto.Request;
using LibraryManagement.Entities;
using System.Linq.Expressions;

namespace LibraryManagement.Services.Interface
{
    public interface IBorrowService
    {
        Task<List<Borrow>> GetAllAsync(Expression<Func<Borrow, bool>> filter = null);
        Task<Borrow> GetAsync(Expression<Func<Borrow, bool>> filter = null, bool tracked = true);
        Task CreateAsync(BorrowBook entity);
        Task SaveAsync();
    }
}
