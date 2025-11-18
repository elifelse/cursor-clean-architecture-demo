using CursorDemo.Application.DTOs;
using CursorDemo.Application.Interfaces;
using CursorDemo.Application.Models;
using CursorDemo.Application.Services;
using CursorDemo.Domain.Entities;
using CursorDemo.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CursorDemo.Tests.Services;

public class BookServiceTests
{
    private readonly Mock<IBookRepository> _mockRepository;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly Mock<ILogger<BookService>> _mockLogger;
    private readonly BookService _bookService;

    public BookServiceTests()
    {
        _mockRepository = new Mock<IBookRepository>();
        _mockCacheService = new Mock<ICacheService>();
        _mockLogger = new Mock<ILogger<BookService>>();
        _bookService = new BookService(_mockRepository.Object, _mockCacheService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllBooksAsync_ShouldReturnAllBooks()
    {
        // Arrange
        var books = new List<Book>
        {
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "Test Book 1",
                Author = "Author 1",
                ISBN = "1234567890",
                PublishedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            },
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "Test Book 2",
                Author = "Author 2",
                ISBN = "0987654321",
                PublishedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(books);

        // Act
        var result = await _bookService.GetAllBooksAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.First().Title.Should().Be("Test Book 1");
        _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetBookByIdAsync_WhenBookExists_ShouldReturnBook()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var book = new Book
        {
            Id = bookId,
            Title = "Test Book",
            Author = "Test Author",
            ISBN = "1234567890",
            PublishedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.GetByIdAsync(bookId)).ReturnsAsync(book);

        // Act
        var result = await _bookService.GetBookByIdAsync(bookId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(bookId);
        result.Title.Should().Be("Test Book");
        result.Author.Should().Be("Test Author");
        _mockRepository.Verify(r => r.GetByIdAsync(bookId), Times.Once);
    }

    [Fact]
    public async Task GetBookByIdAsync_WhenBookDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(bookId)).ReturnsAsync((Book?)null);

        // Act
        var result = await _bookService.GetBookByIdAsync(bookId);

        // Assert
        result.Should().BeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(bookId), Times.Once);
    }

    [Fact]
    public async Task GetBooksPagedAsync_WhenCacheHit_ShouldReturnCachedResult()
    {
        // Arrange
        var parameters = new PaginationParameters { Page = 1, PageSize = 10 };
        var cachedResult = new PagedResult<BookDto>
        {
            Items = new List<BookDto>(),
            Page = 1,
            PageSize = 10,
            TotalCount = 0,
            TotalPages = 0
        };

        _mockCacheService.Setup(c => c.Get<PagedResult<BookDto>>(It.IsAny<string>()))
            .Returns(cachedResult);

        // Act
        var result = await _bookService.GetBooksPagedAsync(parameters);

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(cachedResult);
        _mockCacheService.Verify(c => c.Get<PagedResult<BookDto>>(It.IsAny<string>()), Times.Once);
        _mockRepository.Verify(r => r.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task GetBooksPagedAsync_WhenCacheMiss_ShouldFetchFromRepositoryAndCache()
    {
        // Arrange
        var parameters = new PaginationParameters { Page = 1, PageSize = 10 };
        var books = new List<Book>
        {
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "Test Book",
                Author = "Test Author",
                ISBN = "1234567890",
                PublishedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockCacheService.Setup(c => c.Get<PagedResult<BookDto>>(It.IsAny<string>()))
            .Returns((PagedResult<BookDto>?)null);
        _mockRepository.Setup(r => r.GetPagedAsync(1, 10, null, null, false))
            .ReturnsAsync((books, 1));

        // Act
        var result = await _bookService.GetBooksPagedAsync(parameters);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        _mockRepository.Verify(r => r.GetPagedAsync(1, 10, null, null, false), Times.Once);
        _mockCacheService.Verify(c => c.Set(It.IsAny<string>(), It.IsAny<PagedResult<BookDto>>(), It.IsAny<TimeSpan>()), Times.Once);
    }

    [Fact]
    public async Task GetBooksPagedAsync_WithSearch_ShouldPassSearchToRepository()
    {
        // Arrange
        var parameters = new PaginationParameters 
        { 
            Page = 1, 
            PageSize = 10, 
            Search = "clean" 
        };
        var books = new List<Book>();

        _mockCacheService.Setup(c => c.Get<PagedResult<BookDto>>(It.IsAny<string>()))
            .Returns((PagedResult<BookDto>?)null);
        _mockRepository.Setup(r => r.GetPagedAsync(1, 10, "clean", null, false))
            .ReturnsAsync((books, 0));

        // Act
        var result = await _bookService.GetBooksPagedAsync(parameters);

        // Assert
        _mockRepository.Verify(r => r.GetPagedAsync(1, 10, "clean", null, false), Times.Once);
    }

    [Fact]
    public async Task GetBooksPagedAsync_WithSorting_ShouldPassSortingToRepository()
    {
        // Arrange
        var parameters = new PaginationParameters 
        { 
            Page = 1, 
            PageSize = 10, 
            SortBy = "title",
            Desc = true
        };
        var books = new List<Book>();

        _mockCacheService.Setup(c => c.Get<PagedResult<BookDto>>(It.IsAny<string>()))
            .Returns((PagedResult<BookDto>?)null);
        _mockRepository.Setup(r => r.GetPagedAsync(1, 10, null, "title", true))
            .ReturnsAsync((books, 0));

        // Act
        var result = await _bookService.GetBooksPagedAsync(parameters);

        // Assert
        _mockRepository.Verify(r => r.GetPagedAsync(1, 10, null, "title", true), Times.Once);
    }

    [Fact]
    public async Task CreateBookAsync_ShouldCreateBookAndInvalidateCache()
    {
        // Arrange
        var createDto = new CreateBookDto
        {
            Title = "New Book",
            Author = "New Author",
            ISBN = "1111111111",
            PublishedDate = DateTime.UtcNow
        };

        var createdBook = new Book
        {
            Id = Guid.NewGuid(),
            Title = createDto.Title,
            Author = createDto.Author,
            ISBN = createDto.ISBN,
            PublishedDate = createDto.PublishedDate,
            CreatedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<Book>()))
            .ReturnsAsync(createdBook);

        // Act
        var result = await _bookService.CreateBookAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("New Book");
        result.Author.Should().Be("New Author");
        result.ISBN.Should().Be("1111111111");
        _mockRepository.Verify(r => r.AddAsync(It.Is<Book>(b => 
            b.Title == createDto.Title && 
            b.Author == createDto.Author && 
            b.ISBN == createDto.ISBN)), Times.Once);
        _mockCacheService.Verify(c => c.RemoveByPattern("books:*"), Times.Once);
    }

    [Fact]
    public async Task GetBooksPagedAsync_ShouldCalculateTotalPagesCorrectly()
    {
        // Arrange
        var parameters = new PaginationParameters { Page = 1, PageSize = 5 };
        var books = new List<Book>
        {
            new Book { Id = Guid.NewGuid(), Title = "Book 1", Author = "Author", ISBN = "1", PublishedDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow },
            new Book { Id = Guid.NewGuid(), Title = "Book 2", Author = "Author", ISBN = "2", PublishedDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow },
            new Book { Id = Guid.NewGuid(), Title = "Book 3", Author = "Author", ISBN = "3", PublishedDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow },
            new Book { Id = Guid.NewGuid(), Title = "Book 4", Author = "Author", ISBN = "4", PublishedDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow },
            new Book { Id = Guid.NewGuid(), Title = "Book 5", Author = "Author", ISBN = "5", PublishedDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow },
            new Book { Id = Guid.NewGuid(), Title = "Book 6", Author = "Author", ISBN = "6", PublishedDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow },
            new Book { Id = Guid.NewGuid(), Title = "Book 7", Author = "Author", ISBN = "7", PublishedDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow }
        };

        _mockCacheService.Setup(c => c.Get<PagedResult<BookDto>>(It.IsAny<string>()))
            .Returns((PagedResult<BookDto>?)null);
        _mockRepository.Setup(r => r.GetPagedAsync(1, 5, null, null, false))
            .ReturnsAsync((books.Take(5), 7));

        // Act
        var result = await _bookService.GetBooksPagedAsync(parameters);

        // Assert
        result.TotalCount.Should().Be(7);
        result.TotalPages.Should().Be(2); // 7 items / 5 pageSize = 2 pages
        result.Items.Should().HaveCount(5);
    }
}

