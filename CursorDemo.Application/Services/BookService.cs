using CursorDemo.Application.DTOs;
using CursorDemo.Application.Interfaces;
using CursorDemo.Application.Models;
using CursorDemo.Domain.Entities;
using CursorDemo.Domain.Interfaces;

namespace CursorDemo.Application.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;

    public BookService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<IEnumerable<BookDto>> GetAllBooksAsync()
    {
        var books = await _bookRepository.GetAllAsync();
        return books.Select(MapToDto);
    }

    public async Task<PagedResult<BookDto>> GetBooksPagedAsync(PaginationParameters parameters)
    {
        var (items, totalCount) = await _bookRepository.GetPagedAsync(
            parameters.Page,
            parameters.PageSize,
            parameters.Search,
            parameters.SortBy,
            parameters.Desc);

        var totalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize);

        return new PagedResult<BookDto>
        {
            Items = items.Select(MapToDto),
            Page = parameters.Page,
            PageSize = parameters.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<BookDto?> GetBookByIdAsync(Guid id)
    {
        var book = await _bookRepository.GetByIdAsync(id);
        return book != null ? MapToDto(book) : null;
    }

    public async Task<BookDto> CreateBookAsync(CreateBookDto createBookDto)
    {
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = createBookDto.Title,
            Author = createBookDto.Author,
            ISBN = createBookDto.ISBN,
            PublishedDate = createBookDto.PublishedDate,
            CreatedAt = DateTime.UtcNow
        };

        var createdBook = await _bookRepository.AddAsync(book);
        return MapToDto(createdBook);
    }

    private static BookDto MapToDto(Book book)
    {
        return new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            ISBN = book.ISBN,
            PublishedDate = book.PublishedDate
        };
    }
}

