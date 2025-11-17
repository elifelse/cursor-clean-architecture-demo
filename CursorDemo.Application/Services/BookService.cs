using CursorDemo.Application.DTOs;
using CursorDemo.Application.Interfaces;
using CursorDemo.Application.Models;
using CursorDemo.Domain.Entities;
using CursorDemo.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;

namespace CursorDemo.Application.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<BookService> _logger;
    private const int CacheExpirationSeconds = 30;
    private const string CacheKeyPrefix = "books:";

    public BookService(IBookRepository bookRepository, ICacheService cacheService, ILogger<BookService> logger)
    {
        _bookRepository = bookRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<IEnumerable<BookDto>> GetAllBooksAsync()
    {
        var books = await _bookRepository.GetAllAsync();
        return books.Select(MapToDto);
    }

    public async Task<PagedResult<BookDto>> GetBooksPagedAsync(PaginationParameters parameters)
    {
        // Generate cache key based on pagination parameters
        var cacheKey = GenerateCacheKey(parameters);

        // Try to get from cache
        var cachedResult = _cacheService.Get<PagedResult<BookDto>>(cacheKey);
        if (cachedResult != null)
        {
            _logger.LogInformation("Cache HIT for key: {CacheKey}", cacheKey);
            return cachedResult;
        }

        _logger.LogInformation("Cache MISS for key: {CacheKey}. Fetching from repository...", cacheKey);

        // If not in cache, fetch from repository
        var (items, totalCount) = await _bookRepository.GetPagedAsync(
            parameters.Page,
            parameters.PageSize,
            parameters.Search,
            parameters.SortBy,
            parameters.Desc);

        var totalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize);

        var result = new PagedResult<BookDto>
        {
            Items = items.Select(MapToDto),
            Page = parameters.Page,
            PageSize = parameters.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };

        // Cache the result for 30 seconds
        _cacheService.Set(cacheKey, result, TimeSpan.FromSeconds(CacheExpirationSeconds));
        _logger.LogInformation("Cached result for key: {CacheKey} (expires in {Seconds} seconds)", cacheKey, CacheExpirationSeconds);

        return result;
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

        // Invalidate all book list caches when a new book is created
        _cacheService.RemoveByPattern($"{CacheKeyPrefix}*");
        _logger.LogInformation("Cache invalidated for pattern: {Pattern}", $"{CacheKeyPrefix}*");

        return MapToDto(createdBook);
    }

    private static string GenerateCacheKey(PaginationParameters parameters)
    {
        var keyBuilder = new StringBuilder("books:paged:");
        keyBuilder.Append($"page:{parameters.Page}");
        keyBuilder.Append($":pageSize:{parameters.PageSize}");
        
        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            keyBuilder.Append($":search:{parameters.Search}");
        }
        
        if (!string.IsNullOrWhiteSpace(parameters.SortBy))
        {
            keyBuilder.Append($":sortBy:{parameters.SortBy}");
        }
        
        keyBuilder.Append($":desc:{parameters.Desc}");

        return keyBuilder.ToString();
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

