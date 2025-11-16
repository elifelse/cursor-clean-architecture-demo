using CursorDemo.Application.DTOs;
using CursorDemo.Application.Models;

namespace CursorDemo.Application.Interfaces;

public interface IBookService
{
    Task<IEnumerable<BookDto>> GetAllBooksAsync();
    Task<PagedResult<BookDto>> GetBooksPagedAsync(PaginationParameters parameters);
    Task<BookDto?> GetBookByIdAsync(Guid id);
    Task<BookDto> CreateBookAsync(CreateBookDto createBookDto);
}

