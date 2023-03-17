using LibraryManagement.Dto.Request;
using LibraryManagement.Dto.Response;
using LibraryManagement.Entities;
using System.Linq.Expressions;

namespace LibraryManagement.Services.Interface
{
    public interface IBorrowService
    {
        Task<List<Borrow>> GetAllAsync();
        Task<Borrow> GetAsync(int id);
        Task<List<Borrow>> GetOverdueBorrowedBooks();
        Task<BorrowedBookResponse> CreateAsync(BorrowBook entity);
        Task SaveAsync();
    }
}
