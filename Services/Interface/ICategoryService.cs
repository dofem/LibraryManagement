using LibraryManagement.Entities;
using System.Linq.Expressions;

namespace LibraryManagement.Services.Interface
{
        public interface ICategoryService
        {
            Task<List<Category>> GetAllAsync(Expression<Func<Category, bool>> filter = null);
            Task<Category> GetAsync(Expression<Func<Category, bool>> filter = null, bool tracked = true);
            Task CreateAsync(Category entity);
            Task RemoveAsync(Category entity);
            Task SaveAsync();
        }
}
