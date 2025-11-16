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
}

