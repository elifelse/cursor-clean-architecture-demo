using CursorDemo.Application.DTOs;
using CursorDemo.Application.Interfaces;
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
    /// Get all books
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks()
    {
        var books = await _bookService.GetAllBooksAsync();
        return Ok(books);
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
    [HttpPost]
    public async Task<ActionResult<BookDto>> CreateBook(CreateBookDto createBookDto)
    {
        var book = await _bookService.CreateBookAsync(createBookDto);
        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
    }
}

