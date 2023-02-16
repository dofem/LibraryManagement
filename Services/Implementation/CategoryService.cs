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

        public async Task<List<Category>> GetAllAsync(Expression<Func<Category, bool>> filter = null)
        {
            IQueryable<Category> query = _context.Categories;
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return await query.ToListAsync();
        }

        public async Task<Category> GetAsync(Expression<Func<Category, bool>> filter = null, bool tracked = true)
        {
            IQueryable<Category> query = _context.Categories;
            //if (!tracked)
            //{
            //    query = query.AsNoTracking();
            //}
            if (filter != null)
            {
                query = query.Where(filter);
            }
            return await query.FirstOrDefaultAsync();
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
