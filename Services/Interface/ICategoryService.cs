using LibraryManagement.Entities;
using System.Linq.Expressions;

namespace LibraryManagement.Services.Interface
{
        public interface ICategoryService
        {
            Task<List<Category>> GetAllAsync();
            Task<Category> GetAsync(int id);
            Task<Category> GetByName(string name);
            Task CreateAsync(Category entity);
            Task RemoveAsync(Category entity);
            Task SaveAsync();
        }
}
