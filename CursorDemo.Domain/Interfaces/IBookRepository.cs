using CursorDemo.Domain.Entities;

namespace CursorDemo.Domain.Interfaces;

public interface IBookRepository
{
    Task<IEnumerable<Book>> GetAllAsync();
    Task<Book?> GetByIdAsync(Guid id);
    Task<Book> AddAsync(Book book);
    Task<Book?> UpdateAsync(Book book);
    Task<bool> DeleteAsync(Guid id);
    Task<(IEnumerable<Book> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        string? sortBy = null,
        bool desc = false);
}

