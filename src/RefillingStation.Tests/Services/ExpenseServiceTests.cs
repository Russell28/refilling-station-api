using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using RefillingStation.Application.DTOs.Common;
using RefillingStation.Application.DTOs.Expenses;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Application.Services;
using RefillingStation.Domain.Entities;
using RefillingStation.Domain.Exceptions;
using RefillingStation.Tests.Domain.Builders;
using RefillingStation.Tests.Services.Builders;
using System.Linq.Expressions;

namespace RefillingStation.Tests.Services
{
    public class ExpenseServiceTests
    {
        private readonly Mock<IExpenseRepository> _expenseRepository;
        private readonly Mock<IExpenseCategoryRepository> _expenseCategoryRepository;
        private readonly Mock<IValidator<ExpenseCreateRequest>> _validator;

        private readonly ExpenseService _expenseService;

        public ExpenseServiceTests()
        {
            _expenseRepository = new Mock<IExpenseRepository>();
            _expenseCategoryRepository = new Mock<IExpenseCategoryRepository>();
            _validator = new Mock<IValidator<ExpenseCreateRequest>>();

            _expenseService = new ExpenseService(
                _expenseRepository.Object,
                _expenseCategoryRepository.Object,
                _validator.Object
            );
        }

        #region GetAllAsync
        [Fact]
        public async Task GetAllAsync_Should_Map_All_Records()
        {
            // Arrange
            var category = new ExpenseCategoryBuilder()
                .WithName("Utilities")
                .Build();

            var expenses = new List<Expense>
            {
                new ExpenseBuilder()
                    .WithId(1)
                    .WithCategory(category)
                    .WithAmount(50m)
                    .Build(),
                new ExpenseBuilder()
                    .WithId(2)
                    .WithCategory(category)
                    .WithAmount(75m)
                    .Build()
            };

            _expenseRepository
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(expenses);

            // Act
            var result = await _expenseService.GetAllAsync();

            // Assert
            result.Should().NotBeEmpty();
            result.Should().HaveCount(2);
            result[0].Id.Should().Be(1);
            result[0].Amount.Should().Be(50m);
            result[0].ExpenseCategory.Should().Be("Utilities");
            result[1].Id.Should().Be(2);
            result[1].Amount.Should().Be(75m);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Empty_List_When_No_Records_Exist()
        {
            // Arrange
            _expenseRepository
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync([]);

            // Act
            var result = await _expenseService.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }
        #endregion

        #region GetByIdAsync
        [Fact]
        public async Task GetByIdAsync_Should_Return_MappedResponse_When_Expense_Exists()
        {
            // Arrange
            var category = new ExpenseCategoryBuilder()
                .WithId(1)
                .WithName("Office Supplies")
                .Build();

            var expense = new ExpenseBuilder()
                .WithId(5)
                .WithCategory(category)
                .WithAmount(150m)
                .WithNotes("Monthly office supplies")
                .Build();

            _expenseRepository
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(expense);

            var expected = new ExpenseDetailResponse(
                expense.Id,
                expense.Date,
                expense.ExpenseCategoryId,
                expense.Category.Name,
                expense.Amount,
                expense.Notes
            );

            // Act
            var result = await _expenseService.GetByIdAsync(5);

            // Assert
            result.Should().BeEquivalentTo(expected);
            result.Id.Should().Be(5);
            result.Amount.Should().Be(150m);
            result.ExpenseCategory.Should().Be("Office Supplies");
        }

        [Fact]
        public async Task GetByIdAsync_Should_Throw_NotFoundException_When_Expense_Does_Not_Exist()
        {
            // Arrange
            _expenseRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Expense?)null);

            // Act
            Func<Task> action = async () => await _expenseService.GetByIdAsync(999);

            // Assert
            await action.Should().ThrowAsync<NotFoundException>();
        }
        #endregion

        #region CreateAsync
        [Fact]
        public async Task CreateAsync_Should_Create_Expense_When_Valid_Request_Provided()
        {
            // Arrange
            var request = new ExpenseCreateRequestBuilder()
                .WithAmount(200m)
                .WithExpenseCategoryId(1)
                .WithNotes("New equipment")
                .Build();

            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _expenseCategoryRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<ExpenseCategory, bool>>>()))
                .ReturnsAsync(true);

            _expenseRepository
                .Setup(r => r.AddAsync(It.IsAny<Expense>()))
                .Callback<Expense>(e => e.Id = 1)
                .Returns(Task.CompletedTask);

            _expenseRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            var result = await _expenseService.CreateAsync(request);

            // Assert
            result.Should().BeGreaterThan(0);
            result.Should().Be(1);
            _expenseRepository.Verify(r => r.AddAsync(It.IsAny<Expense>()), Times.Once);
            _expenseRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_ValidationException_When_Request_Is_Invalid()
        {
            // Arrange
            var request = new ExpenseCreateRequestBuilder()
                .WithAmount(-50m)
                .Build();

            var validationFailure = new List<ValidationFailure>
            {
                new ValidationFailure("Amount", "Amount must be greater than zero")
            };

            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(validationFailure));

            // Act
            Func<Task> action = async () => await _expenseService.CreateAsync(request);

            // Assert
            await action.Should().ThrowAsync<ValidationException>();
            _expenseRepository.Verify(r => r.AddAsync(It.IsAny<Expense>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_NotFoundException_When_Category_Does_Not_Exist()
        {
            // Arrange
            var request = new ExpenseCreateRequestBuilder()
                .WithExpenseCategoryId(999)
                .Build();

            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _expenseCategoryRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<ExpenseCategory, bool>>>()))
                .ReturnsAsync(false);

            // Act
            Func<Task> action = async () => await _expenseService.CreateAsync(request);

            // Assert
            await action.Should().ThrowAsync<NotFoundException>();
            _expenseRepository.Verify(r => r.AddAsync(It.IsAny<Expense>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_Should_Set_Correct_Properties_On_Created_Expense()
        {
            // Arrange
            var date = new DateOnly(2024, 1, 15);
            var request = new ExpenseCreateRequestBuilder()
                .WithDate(date)
                .WithExpenseCategoryId(3)
                .WithAmount(500m)
                .WithNotes("Quarterly maintenance")
                .Build();

            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _expenseCategoryRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<ExpenseCategory, bool>>>()))
                .ReturnsAsync(true);

            Expense? capturedExpense = null;
            _expenseRepository
                .Setup(r => r.AddAsync(It.IsAny<Expense>()))
                .Callback<Expense>(e => capturedExpense = e)
                .Returns(Task.CompletedTask);

            _expenseRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _expenseService.CreateAsync(request);

            // Assert
            capturedExpense.Should().NotBeNull();
            capturedExpense!.Date.Should().Be(date);
            capturedExpense.ExpenseCategoryId.Should().Be(3);
            capturedExpense.Amount.Should().Be(500m);
            capturedExpense.Notes.Should().Be("Quarterly maintenance");
        }
        #endregion

        #region UpdateAsync
        [Fact]
        public async Task UpdateAsync_Should_Update_Expense_When_Valid_Request_Provided()
        {
            // Arrange
            var category = new ExpenseCategoryBuilder()
                .WithId(1)
                .Build();

            var existingExpense = new ExpenseBuilder()
                .WithId(5)
                .WithCategory(category)
                .WithAmount(100m)
                .Build();

            var request = new ExpenseCreateRequestBuilder()
                .WithAmount(250m)
                .WithNotes("Updated notes")
                .Build();

            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _expenseCategoryRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<ExpenseCategory, bool>>>()))
                .ReturnsAsync(true);

            _expenseRepository
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(existingExpense);

            _expenseRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _expenseService.UpdateAsync(5, request);

            // Assert
            existingExpense.Amount.Should().Be(250m);
            existingExpense.Notes.Should().Be("Updated notes");
            _expenseRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_NotFoundException_When_Expense_Does_Not_Exist()
        {
            // Arrange
            var request = new ExpenseCreateRequestBuilder().Build();

            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _expenseCategoryRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<ExpenseCategory, bool>>>()))
                .ReturnsAsync(true);

            _expenseRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Expense?)null);

            // Act
            Func<Task> action = async () => await _expenseService.UpdateAsync(999, request);

            // Assert
            await action.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_ValidationException_When_Request_Is_Invalid()
        {
            // Arrange
            var request = new ExpenseCreateRequestBuilder()
                .WithAmount(-100m)
                .Build();

            var validationFailure = new List<ValidationFailure>
            {
                new ValidationFailure("Amount", "Amount must be greater than zero")
            };

            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(validationFailure));

            // Act
            Func<Task> action = async () => await _expenseService.UpdateAsync(5, request);

            // Assert
            await action.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task UpdateAsync_Should_Throw_NotFoundException_When_Category_Does_Not_Exist()
        {
            // Arrange
            var category = new ExpenseCategoryBuilder().Build();
            var existingExpense = new ExpenseBuilder()
                .WithId(5)
                .WithCategory(category)
                .Build();

            var request = new ExpenseCreateRequestBuilder()
                .WithExpenseCategoryId(999)
                .Build();

            _validator
                .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _expenseCategoryRepository
                .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<ExpenseCategory, bool>>>()))
                .ReturnsAsync(false);

            // Act
            Func<Task> action = async () => await _expenseService.UpdateAsync(5, request);

            // Assert
            await action.Should().ThrowAsync<NotFoundException>();
        }
        #endregion

        #region DeleteAsync
        [Fact]
        public async Task DeleteAsync_Should_Delete_Expense_When_Expense_Exists()
        {
            // Arrange
            var category = new ExpenseCategoryBuilder().Build();
            var expense = new ExpenseBuilder()
                .WithId(5)
                .WithCategory(category)
                .Build();

            _expenseRepository
                .Setup(r => r.GetByIdAsync(5))
                .ReturnsAsync(expense);

            _expenseRepository
                .Setup(r => r.Remove(It.IsAny<Expense>()))
                .Callback<Expense>(e => { });

            _expenseRepository
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            // Act
            await _expenseService.DeleteAsync(5);

            // Assert
            _expenseRepository.Verify(r => r.GetByIdAsync(5), Times.Once);
            _expenseRepository.Verify(r => r.Remove(expense), Times.Once);
            _expenseRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_Should_Throw_NotFoundException_When_Expense_Does_Not_Exist()
        {
            // Arrange
            _expenseRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Expense?)null);

            // Act
            Func<Task> action = async () => await _expenseService.DeleteAsync(999);

            // Assert
            await action.Should().ThrowAsync<NotFoundException>();
            _expenseRepository.Verify(r => r.Remove(It.IsAny<Expense>()), Times.Never);
        }
        #endregion

        #region SearchByDateRangeAsync
        [Fact]
        public async Task SearchByDateRangeAsync_Should_Return_Expenses_Within_Date_Range()
        {
            // Arrange
            var category = new ExpenseCategoryBuilder()
                .WithName("Travel")
                .Build();

            var startDate = new DateOnly(2024, 1, 1);
            var endDate = new DateOnly(2024, 1, 31);

            var expenses = new List<Expense>
            {
                new ExpenseBuilder()
                    .WithId(1)
                    .WithDate(new DateOnly(2024, 1, 10))
                    .WithCategory(category)
                    .WithAmount(100m)
                    .Build(),
                new ExpenseBuilder()
                    .WithId(2)
                    .WithDate(new DateOnly(2024, 1, 20))
                    .WithCategory(category)
                    .WithAmount(150m)
                    .Build()
            };

            var request = new DateRangeRequest
            {
                StartDate = startDate,
                EndDate = endDate,
                Page = 1
            };

            _expenseRepository
                .Setup(r => r.SearchByDateRangeAsync(It.IsAny<DateRangeOptions>()))
                .ReturnsAsync(expenses);

            // Act
            var result = await _expenseService.SearchByDateRangeAsync(request);

            // Assert
            result.Should().NotBeEmpty();
            result.Should().HaveCount(2);
            result.All(e => e.ExpenseCategory == "Travel").Should().BeTrue();
            _expenseRepository.Verify(r => r.SearchByDateRangeAsync(It.IsAny<DateRangeOptions>()), Times.Once);
        }

        [Fact]
        public async Task SearchByDateRangeAsync_Should_Return_Empty_List_When_No_Expenses_In_Range()
        {
            // Arrange
            var request = new DateRangeRequest
            {
                StartDate = new DateOnly(2024, 1, 1),
                EndDate = new DateOnly(2024, 1, 31),
                Page = 1
            };

            _expenseRepository
                .Setup(r => r.SearchByDateRangeAsync(It.IsAny<DateRangeOptions>()))
                .ReturnsAsync([]);

            // Act
            var result = await _expenseService.SearchByDateRangeAsync(request);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchByDateRangeAsync_Should_Pass_Correct_Options_To_Repository()
        {
            // Arrange
            var startDate = new DateOnly(2024, 3, 1);
            var endDate = new DateOnly(2024, 3, 31);
            var page = 2;

            var request = new DateRangeRequest
            {
                StartDate = startDate,
                EndDate = endDate,
                Page = page
            };

            _expenseRepository
                .Setup(r => r.SearchByDateRangeAsync(It.IsAny<DateRangeOptions>()))
                .ReturnsAsync([]);

            // Act
            await _expenseService.SearchByDateRangeAsync(request);

            // Assert
            _expenseRepository.Verify(
                r => r.SearchByDateRangeAsync(It.Is<DateRangeOptions>(o =>
                    o.StartDate == startDate &&
                    o.EndDate == endDate &&
                    o.Page == page
                )),
                Times.Once
            );
        }
        #endregion
    }
}
