using Asp.Versioning;
using CursorDemo.Api.Models;
using CursorDemo.Application.DTOs;
using CursorDemo.Application.Interfaces;
using CursorDemo.Application.Models;
using CursorDemo.Application.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CursorDemo.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("2.0")]
[Authorize]
public class BooksV2Controller : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksV2Controller(IBookService bookService)
    {
        _bookService = bookService;
    }

    /// <summary>
    /// Get all books with pagination, filtering, and sorting (v2.0)
    /// </summary>
    /// <param name="page">Page number (default: 1, minimum: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10, range: 1-100)</param>
    /// <param name="search">Search term to filter by title, author, or ISBN</param>
    /// <param name="sortBy">Field to sort by (title, author, isbn, publishedDate, createdAt)</param>
    /// <param name="desc">Sort in descending order (default: false)</param>
    /// <returns>Paginated list of books with additional v2.0 fields</returns>
    /// <response code="200">Returns paginated books</response>
    /// <response code="400">Invalid pagination parameters</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<BookDtoV2>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<BookDtoV2>>> GetBooks(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool desc = false)
    {
        var parameters = new PaginationParameters
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            SortBy = sortBy,
            Desc = desc
        };

        // Validate parameters
        var validator = new PaginationParametersValidator();
        var validationResult = await validator.ValidateAsync(parameters);
        
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            var errorResponse = new ErrorResponse
            {
                StatusCode = 400,
                Message = "Invalid pagination parameters.",
                Errors = errors
            };

            return BadRequest(errorResponse);
        }

        var result = await _bookService.GetBooksPagedAsync(parameters);
        
        // Map to v2 DTO with additional fields
        var v2Items = result.Items.Select(book => new BookDtoV2
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            ISBN = book.ISBN,
            PublishedDate = book.PublishedDate,
            Summary = $"Book by {book.Author}, published in {book.PublishedDate:yyyy}",
            Version = "2.0"
        });

        var v2Result = new PagedResult<BookDtoV2>
        {
            Items = v2Items,
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            TotalPages = result.TotalPages
        };

        return Ok(v2Result);
    }

    /// <summary>
    /// Get a book by ID (v2.0)
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BookDtoV2), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookDtoV2>> GetBook(Guid id)
    {
        var book = await _bookService.GetBookByIdAsync(id);
        if (book == null)
        {
            return NotFound();
        }

        // Map to v2 DTO
        var bookV2 = new BookDtoV2
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            ISBN = book.ISBN,
            PublishedDate = book.PublishedDate,
            Summary = $"Book by {book.Author}, published in {book.PublishedDate:yyyy}",
            Version = "2.0"
        };

        return Ok(bookV2);
    }

    /// <summary>
    /// Create a new book (v2.0)
    /// </summary>
    /// <remarks>
    /// Validation rules:
    /// - Title: Required, minimum 3 characters
    /// - Author: Required, minimum 3 characters
    /// - ISBN: Required
    /// - PublishedDate: Required, cannot be in the future
    /// </remarks>
    /// <param name="createBookDto">Book creation data</param>
    /// <returns>Created book with generated ID and v2.0 fields</returns>
    /// <response code="201">Book created successfully</response>
    /// <response code="400">Validation errors</response>
    [HttpPost]
    [ProducesResponseType(typeof(BookDtoV2), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BookDtoV2>> CreateBook(CreateBookDto createBookDto)
    {
        var book = await _bookService.CreateBookAsync(createBookDto);
        
        // Map to v2 DTO
        var bookV2 = new BookDtoV2
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            ISBN = book.ISBN,
            PublishedDate = book.PublishedDate,
            Summary = $"Book by {book.Author}, published in {book.PublishedDate:yyyy}",
            Version = "2.0"
        };

        return CreatedAtAction(nameof(GetBook), new { id = bookV2.Id, version = "2.0" }, bookV2);
    }
}

