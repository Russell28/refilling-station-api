using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using RefillingStation.Application.DTOs.Users;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Application.Interfaces.Services;
using RefillingStation.Application.Services;
using RefillingStation.Domain.Entities;
using RefillingStation.Domain.Enums;
using RefillingStation.Domain.ErrorCodes;
using RefillingStation.Domain.Exceptions;
using RefillingStation.Tests.Domain.Users;

namespace RefillingStation.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepository;
        private readonly Mock<IPasswordHasher> _passwordHasher;
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository;
        private readonly Mock<IValidator<UserCreateRequest>> _createValidator;
        private readonly Mock<IValidator<ChangePasswordRequest>> _changePasswordValidator;
        private readonly Mock<IValidator<ChangeUserRoleRequest>> _changeRoleValidator;

        private readonly UserService _userService;

        // Constructor: Runs before every test
        // Each [Fact] gets a fresh set of mocks and a fresh UserService
        public UserServiceTests()
        {
            _userRepository = new Mock<IUserRepository>();
            _passwordHasher = new Mock<IPasswordHasher>();
            _refreshTokenRepository = new Mock<IRefreshTokenRepository>();
            _createValidator = new Mock<IValidator<UserCreateRequest>>();
            _changePasswordValidator = new Mock<IValidator<ChangePasswordRequest>>();
            _changeRoleValidator = new Mock<IValidator<ChangeUserRoleRequest>>();

            _userService = new UserService(
                    _userRepository.Object,
                    _passwordHasher.Object,
                    _refreshTokenRepository.Object,
                    _createValidator.Object,
                    _changePasswordValidator.Object,
                    _changeRoleValidator.Object
                );
        }

        #region GetAllAsync
        [Fact] // xUnit attribute: marks this method as a unit test
        public async Task GetAllAsync_Should_Return_All_Users()
        {
            // -----------------------------
            // ARRANGE: set up test scenario
            // -----------------------------

            // Create a fake list of users that the repository will return.
            // Using the UserBuilder ensures consistent defaults and easy overrides.
            var users = new List<User>
            {
                // First user: built with default values from UserBuilder
                new UserBuilder()
                    .Build(),

                // Second user: override the username before building
                new UserBuilder()
                    .WithUsername("Admin02")
                    .Build()
            };

            // Configure the mock repository to return our fake list
            // whenever GetAllAsync() is called.
            _userRepository
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(users);

            // -----------------------------
            // ACT: execute the method under test
            // -----------------------------

            // Call the service method we want to test.
            // This is the "System Under Test" (SUT).
            var result = await _userService.GetAllAsync();

            // -----------------------------
            // ASSERT: verify the outcome
            // -----------------------------

            // FluentAssertions style: check that the result contains exactly 2 users.
            // This ensures the service correctly returned what the repository provided.
            result.Should().HaveCount(2);

            // (Optional extra assertion you might add for clarity)
            // result.Should().Contain(u => u.Username == "Admin02");
        }
        #endregion


        #region GetByIdAsync
        [Fact]
        public async Task GetByIdAsync_Should_Return_User()
        {
            // Arrange
            var user = new UserBuilder()
                .Build();

            _userRepository
                .Setup(r => r.GetByIdAsync(user.Id))
                .ReturnsAsync(user);

            // Act
            var result = await _userService.GetByIdAsync(user.Id);

            // Assert
            result.Username.Should().Be(user.Username);
        }

        [Fact] // Marks this as a unit test in xUnit
        public async Task GetByIdAsync_Should_Throw_When_User_Not_Found()
        {
            // -----------------------------
            // ARRANGE: set up test scenario
            // -----------------------------

            // Build a sample user (not actually used in this test).
            // Including this line shows how you'd normally prepare data,
            // but here the repository will return null to simulate "not found".
            var user = new UserBuilder()
                .Build();

            // Configure the mock repository:
            // When GetByIdAsync(1) is called, return null instead of a User.
            // This simulates the case where the requested user does not exist in the database.
            _userRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((User?)null);

            // -----------------------------
            // ACT: capture the action to test
            // -----------------------------

            // Instead of calling the service directly, wrap it in a Func<Task>.
            // This allows FluentAssertions to inspect the task and assert exceptions.
            Func<Task> act = () => _userService.GetByIdAsync(1);

            // -----------------------------
            // ASSERT: verify the outcome
            // -----------------------------

            // Assert that calling GetByIdAsync throws a NotFoundException.
            // FluentAssertions provides a clean syntax for async exception checks.
            await act.Should()
                .ThrowAsync<NotFoundException>();
        }
        #endregion

        #region CreateAsync
        [Fact] // Marks this as a unit test in xUnit
        public async Task CreateAsync_Should_Create_User_When_Request_Is_Valid()
        {
            // -----------------------------
            // ARRANGE: set up test scenario
            // -----------------------------

            // Define a valid user creation request.
            // This is the input to the service method under test.
            var request = new UserCreateRequest()
            {
                Username = "Admin01",
                Password = "Password123",
                Role = UserRole.Admin
            };

            // Mock the validator: when ValidateAsync is called with this request,
            // return an empty ValidationResult (meaning no validation errors).
            _createValidator
                .Setup(x => x.ValidateAsync(
                    request,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            // Mock the repository: simulate that no existing user has the same username.
            // Returning null means "user not found," so creation can proceed.
            _userRepository
                .Setup(r => r.GetByUsernameAsync(request.Username))
                .ReturnsAsync((User?)null);

            // Mock the password hasher: when hashing the provided password,
            // return a fake but valid bcrypt hash string (~60 characters).
            _passwordHasher
                .Setup(x => x.Hash(request.Password))
                .Returns("$2a$12$abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234");

            // -----------------------------
            // ACT: execute the method under test
            // -----------------------------

            // Call the service method to create the user.
            // This is the "System Under Test" (SUT).
            await _userService.CreateAsync(request);

            // -----------------------------
            // ASSERT: verify the outcome
            // -----------------------------

            // Verify that AddAsync was called exactly once with any User object.
            // This ensures the service attempted to persist the new user.
            _userRepository.Verify(
                    x => x.AddAsync(It.IsAny<User>()),
                    Times.Once
                );

            // Verify that SaveChangesAsync was called exactly once.
            // This ensures the service committed the changes to the repository.
            _userRepository.Verify(
                    x => x.SaveChangesAsync(),
                    Times.Once
                );
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_When_Request_Is_Invalid()
        {
            // Arrange
            var request = new UserCreateRequest();

            var validationResult = new ValidationResult(
                new[]
                {
                    new ValidationFailure("Username", "Required")
                });

            _createValidator
                .Setup(v => v.ValidateAsync(
                    request,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // Act
            Func<Task> act = () => _userService.CreateAsync(request);

            // Assert
            await act.Should().ThrowAsync<ValidationException>();
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_When_Username_Already_Exists()
        {
            // Arrange
            var request = new UserCreateRequest()
            {
                Username = "Admin01",
                Password = "Password123",
                Role = UserRole.Admin
            };

            // No validation errors
            _createValidator
                .Setup(x => x.ValidateAsync(
                    request,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            // Returns existing user
            _userRepository
                .Setup(r => r.GetByUsernameAsync(request.Username))
                .ReturnsAsync(new UserBuilder().Build());

            // Act
            Func<Task> act = () => _userService.CreateAsync(request);

            // Assert
            await act.Should().ThrowAsync<ConflictException>();
        }
        #endregion

        #region ChangePassword
        // Should Save password when request is valid
        [Fact]
        public async Task ChangePasswordAsync_Should_Hash_And_Save()
        {
            // Arrange
            var request = new ChangePasswordRequest()
            {
                Password = "Password123"
            };

            _changePasswordValidator
                .Setup(x => x.ValidateAsync(
                        request,
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var user = new UserBuilder().Build();

            _userRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(user);

            var passwordHash = "$2a$12$NEWabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234";
            _passwordHasher
                .Setup(x => x.Hash(request.Password))
                .Returns(passwordHash);

            // Act
            await _userService.ChangePasswordAsync(1, request);

            // Assert
            user.PasswordHash.Should().Be(passwordHash);
            _userRepository.Verify(
                x => x.SaveChangesAsync(), 
                Times.Once());

        }

        // Throw if validation failed
        [Fact]
        public async Task ChangePasswordAsync_Should_Throw_When_Request_Is_Invalid()
        {
            // Arrange
            var request = new ChangePasswordRequest();

            var validationResult = new ValidationResult(
                new[]
                {
                    new ValidationFailure("Password", "Required")
                });

            _changePasswordValidator
                .Setup(x => x.ValidateAsync(
                        request,
                        It.IsAny<CancellationToken>()
                    ))
                .ReturnsAsync(validationResult);

            // Act
            Func<Task> act = () => _userService.ChangePasswordAsync(1, request);

            // Assert
            await act.Should().ThrowAsync<ValidationException>();
        }

        // Throw if user not found
        [Fact]
        public async Task ChangePasswordAsync_Should_Throw_When_User_Not_Found()
        {
            // Arrange
            var request = new ChangePasswordRequest();

            _changePasswordValidator
                .Setup(x => x.ValidateAsync(
                        request,
                        It.IsAny<CancellationToken>()
                    ))
                .ReturnsAsync(new ValidationResult());

            _userRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((User?)null);

            // Act
            Func<Task> act = () => _userService.GetByIdAsync(1);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }
        #endregion

        #region ChangeRole
        // Should save role
        [Fact]
        public async Task ChangeRoleAsync_Should_Set_Role_And_Save()
        {
            // Arrange
            var request = new ChangeUserRoleRequest()
            {
                Role = UserRole.Employee
            };

            _changeRoleValidator
                .Setup(x => x.ValidateAsync(
                        request,
                        It.IsAny<CancellationToken>()
                    ))
                .ReturnsAsync(new ValidationResult());

            var user = new UserBuilder().Build();

            _userRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(user);

            // Act
            await _userService.ChangeRoleAsync(1, request);

            // Assert
            user.Role.Should().Be(request.Role);
            _userRepository.Verify(x => x.SaveChangesAsync(),
                Times.Once());
        }

        // Should throw if request is invalid
        [Fact]
        public async Task ChangeRoleAsync_Should_Throw_When_Request_Is_Invalid()
        {
            // arrange
            var request = new ChangeUserRoleRequest()
            {
                Role = default(UserRole)
            };

            var validationResult = new ValidationResult(
                new[]
                {
                    new ValidationFailure("Role", "Invalid")
                });

            _changeRoleValidator
                .Setup(x => x.ValidateAsync(
                    request,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(validationResult);

            // act
            Func<Task> act = () => _userService.ChangeRoleAsync(1, request);

            // assert
            await act.Should().ThrowAsync<ValidationException>();
        }

        // Should throw if user not found
        [Fact]
        public async Task ChangeRoleAsync_Should_Throw_When_User_NotFound()
        {
            // arrange
            var request = new ChangeUserRoleRequest();

            _changeRoleValidator
                .Setup(x => x.ValidateAsync(
                    request,
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _userRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((User?) null);

            // act
            Func<Task> act = () => _userService.ChangeRoleAsync(1, request);

            // assert
            await act.Should().ThrowAsync<NotFoundException>();
        }
        #endregion

        #region ActivateAsync
        // Should activate and save
        [Fact]
        public async Task ActivateAsync_Should_Activate_User_And_Save()
        {
            // Arrange
            var user = new UserBuilder().Build();
            user.Deactivate();

            _userRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(user);

            // Act
            await _userService.ActivateAsync(1);

            // Assert
            user.IsActive.Should().BeTrue(); // domain side effect
            _userRepository // Persistence
                .Verify(x => x.SaveChangesAsync(),
                Times.Once());
        }

        // Should throw when user notfound
        [Fact]
        public async Task ActivateAsync_Should_Throw_When_User_NotFound()
        {
            // Arrange
            _userRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((User?) null);

            // Act
            Func<Task> act = () => _userService.ActivateAsync(1);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }
        #endregion

        #region DeactivateAsync
        [Fact]
        public async Task DeactivateAsync_Should_Deactivate_User_Delete_Tokens()
        {
            // Arrange
            var user = new UserBuilder().Build();

            _userRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(user);

            // Act
            await _userService.DeactivateAsync(targetUserId: 1, requestingUserId: 2);

            // Assert
            user.IsActive.Should().BeFalse(); // domain side effect
            _refreshTokenRepository
                .Verify(x => x.DeleteAllByUserIdAsync(user.Id), Times.Once());
            _userRepository // Persistence
                .Verify(x => x.SaveChangesAsync(),
                Times.Once());
        }

        [Fact]
        public async Task DeactivateAsync_Should_Throw_When_RequestingUser_Tries_To_Deactivate_Self()
        {
            // Act
            Func<Task> act = () => _userService.DeactivateAsync(targetUserId: 1, requestingUserId: 1);

            // Assert
            await act.Should().ThrowAsync<DomainException>()
                .Where(e => e.Code == DomainErrorCodes.UserCode.CannotDeleteSelf);
        }

        [Fact]
        public async Task DeactivateAsync_Should_Throw_When_User_NotFound()
        {
            // Arrange
            _userRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((User?)null);

            // Act
            Func<Task> act = () => _userService.DeactivateAsync(targetUserId: 1, requestingUserId: 2);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }
        #endregion

        #region DeleteAsync
        [Fact]
        public async Task DeleteAsync_Should_Remove_User_And_Save()
        {
            var user = new UserBuilder().Build();

            _userRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(user);

            // Act
            await _userService.DeleteAsync(targetUserId: 1, requestingUserId: 2);

            // Assert
            _userRepository
                .Verify(x => x.Remove(user), Times.Once());
            _userRepository
                .Verify(x => x.SaveChangesAsync(), Times.Once());
        }

        [Fact]
        public async Task DeleteAsync_Should_Throw_When_RequestingUser_Tries_To_Delete_Self()
        {
            // Act
            Func<Task> act = () => _userService.DeleteAsync(targetUserId: 1, requestingUserId: 1);

            // Assert
            await act.Should().ThrowAsync<DomainException>()
                .Where(e => e.Code == DomainErrorCodes.UserCode.CannotDeleteSelf);
        }

        [Fact]
        public async Task DeleteAsync_Should_Throw_When_User_Not_Found()
        {
            // Arrange
            _userRepository
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((User?)null);

            // Act
            Func<Task> act = () => _userService.DeleteAsync(1, 2);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }
        #endregion





    }
}
