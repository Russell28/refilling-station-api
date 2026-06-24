// Unit tests for ReportsService
// Follow Arrange-Act-Assert pattern
// Use xUnit for test framework
// Use Moq for mocking dependencies
// Use FluentAssertions for assertions
// Include tests for valid data, empty data, cache, and edge cases
// Organize tests by method

using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using RefillingStation.Application.DTOs.Employees;
using RefillingStation.Application.DTOs.ExpenseCategories;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Application.Interfaces.Services;
using RefillingStation.Application.Services;
using RefillingStation.Domain.Entities;
using RefillingStation.Domain.Exceptions;
using RefillingStation.Tests.Domain.Builders;

namespace RefillingStation.Tests.Services
{
    public class ExpenseCategoryServiceTests
    {
        private readonly Mock<IExpenseCategoryRepository> _expenseCategoryRepository;
        private readonly Mock<IMemoryCache> _mockMemoryCache;

        private readonly ExpenseCategoryService _expenseCategoryService;

        public ExpenseCategoryServiceTests()
        {
            _expenseCategoryRepository = new Mock<IExpenseCategoryRepository>();
            _mockMemoryCache = new Mock<IMemoryCache>();

            _expenseCategoryService = new ExpenseCategoryService(
                    _expenseCategoryRepository.Object,
                    _mockMemoryCache.Object
                );
        }

        #region GetAllAsync
        [Fact]
        public async Task GetAllAsync_Should_Return_Cached_Results_When_Cache_Hit()
        {
            // Arrange
            var cachedResult = new List<ExpenseCategoryListItemResponse>
            {
                new ExpenseCategoryListItemResponse(1, "Gas")
            };

            object cachedValue = cachedResult;
            _mockMemoryCache
                .Setup(c => c.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(true);

            // Act
            var result = await _expenseCategoryService.GetAllAsync();

            // Assert
            result.Should().BeEquivalentTo(cachedResult);
            _expenseCategoryRepository.Verify(r => r.GetAllAsync(), Times.Never);
        }
        #endregion

        #region GetByIdAsync
        [Fact]
        public async Task GetByIdAsync_Should_Return_ExpenseCategory_When_Exists()
        {
            // Arrange
            var category = new ExpenseCategoryBuilder()
                .WithId(5)
                .WithName("Gas")
                .Build();

            _expenseCategoryRepository
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(category);

            // Act
            var result = await _expenseCategoryService.GetByIdAsync(5);

            // Assert
            result.Id.Should().Be(5);
            result.Name.Should().Be("Gas");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Throw_When_Category_Does_Not_Exists()
        {
            // Arrange
            _expenseCategoryRepository
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync((ExpenseCategory?) null);

            // Act
            Func<Task> act = () => _expenseCategoryService.GetByIdAsync(5);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        #endregion
    }
}
