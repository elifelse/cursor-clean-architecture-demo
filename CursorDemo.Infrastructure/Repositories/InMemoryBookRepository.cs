using CursorDemo.Domain.Entities;
using CursorDemo.Domain.Interfaces;

namespace CursorDemo.Infrastructure.Repositories;

public class InMemoryBookRepository : IBookRepository
{
    private readonly List<Book> _books = new();

    public InMemoryBookRepository()
    {
        // Seed some initial data
        _books.AddRange(new[]
        {
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "Clean Code",
                Author = "Robert C. Martin",
                ISBN = "978-0132350884",
                PublishedDate = new DateTime(2008, 8, 11),
                CreatedAt = DateTime.UtcNow
            },
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "The Clean Architecture",
                Author = "Robert C. Martin",
                ISBN = "978-0134494166",
                PublishedDate = new DateTime(2017, 9, 20),
                CreatedAt = DateTime.UtcNow
            },
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "Design Patterns",
                Author = "Gang of Four",
                ISBN = "978-0201633610",
                PublishedDate = new DateTime(1994, 10, 21),
                CreatedAt = DateTime.UtcNow
            },
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "Refactoring",
                Author = "Martin Fowler",
                ISBN = "978-0201485677",
                PublishedDate = new DateTime(1999, 7, 8),
                CreatedAt = DateTime.UtcNow
            },
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "Domain-Driven Design",
                Author = "Eric Evans",
                ISBN = "978-0321125217",
                PublishedDate = new DateTime(2003, 8, 30),
                CreatedAt = DateTime.UtcNow
            },
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "Test Driven Development",
                Author = "Kent Beck",
                ISBN = "978-0321146533",
                PublishedDate = new DateTime(2002, 11, 18),
                CreatedAt = DateTime.UtcNow
            },
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "The Pragmatic Programmer",
                Author = "Andrew Hunt",
                ISBN = "978-0201616224",
                PublishedDate = new DateTime(1999, 10, 20),
                CreatedAt = DateTime.UtcNow
            },
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "Code Complete",
                Author = "Steve McConnell",
                ISBN = "978-0735619678",
                PublishedDate = new DateTime(2004, 6, 9),
                CreatedAt = DateTime.UtcNow
            },
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "Working Effectively with Legacy Code",
                Author = "Michael Feathers",
                ISBN = "978-0131177055",
                PublishedDate = new DateTime(2004, 9, 22),
                CreatedAt = DateTime.UtcNow
            },
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "You Don't Know JS",
                Author = "Kyle Simpson",
                ISBN = "978-1491924464",
                PublishedDate = new DateTime(2015, 5, 1),
                CreatedAt = DateTime.UtcNow
            },
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "Effective Java",
                Author = "Joshua Bloch",
                ISBN = "978-0134685991",
                PublishedDate = new DateTime(2018, 1, 6),
                CreatedAt = DateTime.UtcNow
            },
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "Clean Architecture",
                Author = "Robert C. Martin",
                ISBN = "978-0134494166",
                PublishedDate = new DateTime(2017, 9, 20),
                CreatedAt = DateTime.UtcNow
            }
        });
    }

    public Task<IEnumerable<Book>> GetAllAsync()
    {
        return Task.FromResult<IEnumerable<Book>>(_books);
    }

    public Task<Book?> GetByIdAsync(Guid id)
    {
        var book = _books.FirstOrDefault(b => b.Id == id);
        return Task.FromResult(book);
    }

    public Task<Book> AddAsync(Book book)
    {
        _books.Add(book);
        return Task.FromResult(book);
    }

    public Task<Book?> UpdateAsync(Book book)
    {
        var existingBook = _books.FirstOrDefault(b => b.Id == book.Id);
        if (existingBook == null)
        {
            return Task.FromResult<Book?>(null);
        }

        existingBook.Title = book.Title;
        existingBook.Author = book.Author;
        existingBook.ISBN = book.ISBN;
        existingBook.PublishedDate = book.PublishedDate;
        existingBook.UpdatedAt = DateTime.UtcNow;

        return Task.FromResult<Book?>(existingBook);
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        var book = _books.FirstOrDefault(b => b.Id == id);
        if (book == null)
        {
            return Task.FromResult(false);
        }

        _books.Remove(book);
        return Task.FromResult(true);
    }

    public Task<(IEnumerable<Book> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search = null,
        string? sortBy = null,
        bool desc = false)
    {
        var query = _books.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLowerInvariant();
            query = query.Where(b =>
                b.Title.ToLowerInvariant().Contains(searchLower) ||
                b.Author.ToLowerInvariant().Contains(searchLower) ||
                b.ISBN.ToLowerInvariant().Contains(searchLower));
        }

        // Apply sorting
        query = sortBy?.ToLowerInvariant() switch
        {
            "title" => desc ? query.OrderByDescending(b => b.Title) : query.OrderBy(b => b.Title),
            "author" => desc ? query.OrderByDescending(b => b.Author) : query.OrderBy(b => b.Author),
            "isbn" => desc ? query.OrderByDescending(b => b.ISBN) : query.OrderBy(b => b.ISBN),
            "publisheddate" => desc ? query.OrderByDescending(b => b.PublishedDate) : query.OrderBy(b => b.PublishedDate),
            "createdat" => desc ? query.OrderByDescending(b => b.CreatedAt) : query.OrderBy(b => b.CreatedAt),
            _ => query.OrderBy(b => b.Title) // Default sort by title
        };

        var totalCount = query.Count();

        // Apply pagination
        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult((Items: items.AsEnumerable(), TotalCount: totalCount));
    }
}

