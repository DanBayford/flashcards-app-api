using System.Net;
using Flashcards.Api.Features.Categories;
using Flashcards.Api.Features.Users;
using Xunit;

namespace tests;

public class CategoryTests : IClassFixture<TestApiFactory>
{
    private const string TestUserEmail = "test@test.com";
    private readonly HttpClient _client;
    
    public CategoryTests(TestApiFactory factory)
    {
        _client = factory.CreateClient();
    }
    
    // Unit tests (only testing underlying C# code)
    [Fact]
    public void ToDto_MapsCorrectly()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Test Category"
        };

        // Act
        var dto = category.ToDto();
        
        // Assert
        Assert.Equal(category.Id, dto.Id);
        Assert.Equal("Test Category", dto.Name);
    }

    [Fact]
    public void CreateFromDto_AssignsUserCorrectly()
    {
        var user = new User { Id = Guid.NewGuid(), Email = TestUserEmail };
        var dto = new CreateCategoryDto {  Name = "Test Category" };
        
        var result = dto.CreateFromDto(user);
        
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal("Test Category", result.Name);
    }

    // Integration tests (using the client provided by TestApiFactory)
    [Fact]
    public async Task GetCategories_ReturnsUnauthorized_NoLogin()
    {
        var response = await _client.GetAsync("/api/category");
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
