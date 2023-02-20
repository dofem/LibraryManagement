﻿using LibraryManagement.Dto.Request;
using LibraryManagement.Dto.Response;
using LibraryManagement.Entities;
using System.Linq.Expressions;

namespace LibraryManagement.Services.Interface
{
    public interface IBorrowService
    {
        Task<List<Borrow>> GetAllAsync(Expression<Func<Borrow, bool>> filter = null);
        Task<Borrow> GetAsync(Expression<Func<Borrow, bool>> filter = null, bool tracked = true);
        Task<List<Borrow>> GetOverdueBorrowedBooks();
        Task<BorrowedBookResponse> CreateAsync(BorrowBook entity);
        Task SaveAsync();
    }
}
