using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CursorDemo.Application.DTOs;
using CursorDemo.Application.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace CursorDemo.Tests.Integration;

public class BooksControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public BooksControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetBooks_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/books");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetBooks_WithValidToken_ShouldReturnOk()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/books");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetBooks_WithPagination_ShouldReturnPagedResults()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/books?page=1&pageSize=5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<BookDto>>();
        result.Should().NotBeNull();
        result!.Page.Should().Be(1);
        result.PageSize.Should().Be(5);
        result.Items.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBooks_WithSearch_ShouldFilterResults()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/books?search=Clean");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<BookDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetBookById_WithValidId_ShouldReturnBook()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // First, get a list of books to find a valid ID
        var listResponse = await _client.GetAsync("/api/v1/books?page=1&pageSize=1");
        var listResult = await listResponse.Content.ReadFromJsonAsync<PagedResult<BookDto>>();
        var bookId = listResult!.Items.First().Id;

        // Act
        var response = await _client.GetAsync($"/api/v1/books/{bookId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var book = await response.Content.ReadFromJsonAsync<BookDto>();
        book.Should().NotBeNull();
        book!.Id.Should().Be(bookId);
    }

    [Fact]
    public async Task GetBookById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/v1/books/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateBook_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var newBook = new CreateBookDto
        {
            Title = "Integration Test Book",
            Author = "Test Author",
            ISBN = "INT-TEST-001",
            PublishedDate = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(newBook);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/v1/books", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdBook = await response.Content.ReadFromJsonAsync<BookDto>();
        createdBook.Should().NotBeNull();
        createdBook!.Title.Should().Be("Integration Test Book");
        createdBook.Author.Should().Be("Test Author");
    }

    [Fact]
    public async Task CreateBook_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var invalidBook = new CreateBookDto
        {
            Title = "AB", // Too short (minimum 3 characters)
            Author = "", // Empty
            ISBN = "",
            PublishedDate = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(invalidBook);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/v1/books", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var errorResponse = await response.Content.ReadAsStringAsync();
        errorResponse.Should().Contain("errors");
    }

    [Fact]
    public async Task GetBooks_WithInvalidPagination_ShouldReturnBadRequest()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/books?page=0&pageSize=200");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var loginDto = new LoginDto
        {
            Username = "elif",
            Password = "1234"
        };

        var json = JsonSerializer.Serialize(loginDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/v1/auth/login", content);
        response.EnsureSuccessStatusCode();

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponseDto>();
        return tokenResponse!.Token;
    }
}

// We need to expose Program class for WebApplicationFactory
public partial class Program { }

