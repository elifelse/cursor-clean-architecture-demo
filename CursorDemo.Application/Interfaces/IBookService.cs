using CursorDemo.Application.DTOs;

namespace CursorDemo.Application.Interfaces;

public interface IBookService
{
    Task<IEnumerable<BookDto>> GetAllBooksAsync();
    Task<BookDto?> GetBookByIdAsync(Guid id);
    Task<BookDto> CreateBookAsync(CreateBookDto createBookDto);
}

