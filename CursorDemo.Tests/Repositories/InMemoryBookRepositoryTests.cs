using CursorDemo.Domain.Entities;
using CursorDemo.Infrastructure.Repositories;
using FluentAssertions;
using Xunit;

namespace CursorDemo.Tests.Repositories;

public class InMemoryBookRepositoryTests
{
    private readonly InMemoryBookRepository _repository;

    public InMemoryBookRepositoryTests()
    {
        _repository = new InMemoryBookRepository();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllBooks()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_WhenBookExists_ShouldReturnBook()
    {
        // Arrange
        var allBooks = await _repository.GetAllAsync();
        var existingBook = allBooks.First();

        // Act
        var result = await _repository.GetByIdAsync(existingBook.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(existingBook.Id);
        result.Title.Should().Be(existingBook.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WhenBookDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldAddBookToRepository()
    {
        // Arrange
        var newBook = new Book
        {
            Id = Guid.NewGuid(),
            Title = "New Test Book",
            Author = "Test Author",
            ISBN = "9999999999",
            PublishedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.AddAsync(newBook);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(newBook.Id);
        result.Title.Should().Be("New Test Book");

        // Verify it was actually added
        var retrieved = await _repository.GetByIdAsync(newBook.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Title.Should().Be("New Test Book");
    }

    [Fact]
    public async Task UpdateAsync_WhenBookExists_ShouldUpdateBook()
    {
        // Arrange
        var allBooks = await _repository.GetAllAsync();
        var existingBook = allBooks.First();
        var updatedBook = new Book
        {
            Id = existingBook.Id,
            Title = "Updated Title",
            Author = "Updated Author",
            ISBN = "8888888888",
            PublishedDate = DateTime.UtcNow,
            CreatedAt = existingBook.CreatedAt
        };

        // Act
        var result = await _repository.UpdateAsync(updatedBook);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated Title");
        result.Author.Should().Be("Updated Author");
        result.ISBN.Should().Be("8888888888");
        result.UpdatedAt.Should().NotBeNull();

        // Verify it was actually updated
        var retrieved = await _repository.GetByIdAsync(existingBook.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Title.Should().Be("Updated Title");
    }

    [Fact]
    public async Task UpdateAsync_WhenBookDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var nonExistentBook = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Non Existent",
            Author = "Author",
            ISBN = "0000000000",
            PublishedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.UpdateAsync(nonExistentBook);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenBookExists_ShouldRemoveBook()
    {
        // Arrange
        var newBook = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Book To Delete",
            Author = "Author",
            ISBN = "7777777777",
            PublishedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
        await _repository.AddAsync(newBook);

        // Act
        var result = await _repository.DeleteAsync(newBook.Id);

        // Assert
        result.Should().BeTrue();

        // Verify it was actually deleted
        var retrieved = await _repository.GetByIdAsync(newBook.Id);
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenBookDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.DeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedResults()
    {
        // Arrange
        var page = 1;
        var pageSize = 5;

        // Act
        var (items, totalCount) = await _repository.GetPagedAsync(page, pageSize);

        // Assert
        items.Should().NotBeNull();
        items.Should().HaveCountLessThanOrEqualTo(pageSize);
        totalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetPagedAsync_WithSearch_ShouldFilterResults()
    {
        // Arrange
        var searchTerm = "Clean";

        // Act
        var (items, totalCount) = await _repository.GetPagedAsync(1, 10, search: searchTerm);

        // Assert
        items.Should().NotBeNull();
        items.All(b => 
            b.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            b.Author.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            b.ISBN.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .Should().BeTrue();
    }

    [Fact]
    public async Task GetPagedAsync_WithSorting_ShouldSortResults()
    {
        // Arrange
        var sortBy = "title";
        var desc = false;

        // Act
        var (items, totalCount) = await _repository.GetPagedAsync(1, 10, sortBy: sortBy, desc: desc);

        // Assert
        items.Should().NotBeNull();
        var itemsList = items.ToList();
        if (itemsList.Count > 1)
        {
            for (int i = 0; i < itemsList.Count - 1; i++)
            {
                string.Compare(itemsList[i].Title, itemsList[i + 1].Title, StringComparison.OrdinalIgnoreCase)
                    .Should().BeLessThanOrEqualTo(0);
            }
        }
    }

    [Fact]
    public async Task GetPagedAsync_WithDescendingSort_ShouldSortDescending()
    {
        // Arrange
        var sortBy = "title";
        var desc = true;

        // Act
        var (items, totalCount) = await _repository.GetPagedAsync(1, 10, sortBy: sortBy, desc: desc);

        // Assert
        items.Should().NotBeNull();
        var itemsList = items.ToList();
        if (itemsList.Count > 1)
        {
            for (int i = 0; i < itemsList.Count - 1; i++)
            {
                string.Compare(itemsList[i].Title, itemsList[i + 1].Title, StringComparison.OrdinalIgnoreCase)
                    .Should().BeGreaterThanOrEqualTo(0);
            }
        }
    }

    [Fact]
    public async Task GetPagedAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var page1 = 1;
        var page2 = 2;
        var pageSize = 5;

        // Act
        var (items1, totalCount1) = await _repository.GetPagedAsync(page1, pageSize);
        var (items2, totalCount2) = await _repository.GetPagedAsync(page2, pageSize);

        // Assert
        totalCount1.Should().Be(totalCount2); // Total count should be same
        items1.Should().NotBeEquivalentTo(items2); // Different pages should have different items
    }
}

