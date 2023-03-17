using LibraryManagement.Data;
using LibraryManagement.Entities;
using LibraryManagement.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace LibraryManagement.Services.Implementation
{
    public class CategoryService : ICategoryService
    {
        private ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(Category entity)
        {
            await _context.Categories.AddAsync(entity);
            await SaveAsync();
        }

        public async Task<List<Category>> GetAllAsync()
        {          
            return await _context.Categories.ToListAsync();
        }

        public async Task<Category> GetAsync(int id)
        {
            
            return await _context.Categories.Where(u=>u.Id==id).FirstOrDefaultAsync();
        }

        public async Task<Category> GetByName(string name)
        {
            var category = await _context.Categories.Where(u=>u.Name.ToLower()==name.ToLower()).FirstOrDefaultAsync();
            return category;
        }

        public async Task RemoveAsync(Category entity)
        {
            _context.Categories.Remove(entity);
            await SaveAsync();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
