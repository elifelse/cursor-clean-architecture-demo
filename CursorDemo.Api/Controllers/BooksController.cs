using CursorDemo.Api.Models;
using CursorDemo.Application.DTOs;
using CursorDemo.Application.Interfaces;
using CursorDemo.Application.Models;
using CursorDemo.Application.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CursorDemo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BooksController : ControllerBase
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    /// <summary>
    /// Get all books with pagination, filtering, and sorting
    /// </summary>
    /// <param name="page">Page number (default: 1, minimum: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 10, range: 1-100)</param>
    /// <param name="search">Search term to filter by title, author, or ISBN</param>
    /// <param name="sortBy">Field to sort by (title, author, isbn, publishedDate, createdAt)</param>
    /// <param name="desc">Sort in descending order (default: false)</param>
    /// <returns>Paginated list of books</returns>
    /// <response code="200">Returns paginated books</response>
    /// <response code="400">Invalid pagination parameters</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<BookDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<BookDto>>> GetBooks(
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
        return Ok(result);
    }

    /// <summary>
    /// Get a book by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<BookDto>> GetBook(Guid id)
    {
        var book = await _bookService.GetBookByIdAsync(id);
        if (book == null)
        {
            return NotFound();
        }
        return Ok(book);
    }

    /// <summary>
    /// Create a new book
    /// </summary>
    /// <remarks>
    /// Validation rules:
    /// - Title: Required, minimum 3 characters
    /// - Author: Required, minimum 3 characters
    /// - ISBN: Required
    /// - PublishedDate: Required, cannot be in the future
    /// </remarks>
    /// <param name="createBookDto">Book creation data</param>
    /// <returns>Created book with generated ID</returns>
    /// <response code="201">Book created successfully</response>
    /// <response code="400">Validation errors</response>
    [HttpPost]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BookDto>> CreateBook(CreateBookDto createBookDto)
    {
        var book = await _bookService.CreateBookAsync(createBookDto);
        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
    }
}

