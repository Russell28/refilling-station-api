using FluentAssertions;
using RefillingStation.Domain.Entities;
using RefillingStation.Domain.Enums;
using RefillingStation.Domain.ErrorCodes;
using RefillingStation.Domain.Exceptions;

namespace RefillingStation.Tests.Domain.Users
{
    public class UserTests
    {
        #region Constructor
        [Fact]
        public void Constructor_Should_Create_User_With_Valid_Data()
        {
            // Arrange
            var username = "admin01";
            var passwordHash = "$2a$12$abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234";

            // Act
            var user = new User(
                    username,
                    passwordHash,
                    UserRole.Admin
                );

            // Assert
            Assert.Equal(username, user.Username);
            Assert.Equal(passwordHash, user.PasswordHash);
            Assert.Equal(UserRole.Admin, user.Role);
            Assert.True(user.IsActive);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Constructor_Should_Throw_When_Username_Is_Empty(string username) // executed 3 times with 3 different values from InlineData
        {
            // Arrange
            var builder = new UserBuilder()
                .WithUsername(username);

            // Act + Assert (xUnit style)
            var ex = Assert.Throws<DomainException>(() => builder.Build());
            Assert.Equal(DomainErrorCodes.CommonCode.RequiredField, ex.Code);
        }

        [Fact]
        public void Constructor_Should_Throw_When_Username_Is_Too_Short()
        {
            // Arrange
            var builder = new UserBuilder()
                .WithUsername("abcd");

            // Act + Assert (xUnit style)
            var ex = Assert.Throws<DomainException>(() => builder.Build());

            // Check error code
            Assert.Equal(DomainErrorCodes.UserCode.InvalidUsernameLength, ex.Code);
        }

        [Fact]
        public void Constructor_Should_Throw_When_Username_Is_Too_Long()
        {
            // Arrange
            var builder = new UserBuilder()
                .WithUsername(new string('a', 21));

            // Act
            Action act = () => builder.Build();

            // Assert (FluentAssertions style)
            act.Should().Throw<DomainException>()
                .Where(e => e.Code == DomainErrorCodes.UserCode.InvalidUsernameLength);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Constructor_Should_Throw_When_PasswordHash_Is_Empty(string passwordHash)
        {
            // Arrange
            var builder = new UserBuilder()
                .WithPasswordHash(passwordHash);

            // Act
            Action act = () => builder.Build();

            // Assert (FluentAssertions style)
            act.Should().Throw<DomainException>()
                .Where(e => e.Code == DomainErrorCodes.CommonCode.RequiredField);
        }

        [Fact]
        public void Constructor_Should_Throw_When_PasswordHash_Is_Too_Short()
        {
            // Arrange
            var builder = new UserBuilder()
                .WithPasswordHash(new string('a', 49));

            // Act
            Action act = () => builder.Build();

            // Assert (FluentAssertions style)
            act.Should().Throw<DomainException>()
                .Where(e => e.Code == DomainErrorCodes.UserCode.InvalidPassword);
        }

        [Fact]
        public void Constructor_Should_Throw_When_Role_Is_Invalid()
        {
            var builder = new UserBuilder()
                .WithRole(default);

            Action act = () => builder.Build();

            act.Should().Throw<DomainException>()
                .Where(e => e.Code == DomainErrorCodes.UserCode.InvalidUserRole);
        }
        #endregion

        #region ChangePassword
        [Fact]
        public void ChangePassword_Should_Update_PasswordHash_And_Timestamp()
        {
            // Arrange
            var user = new UserBuilder().Build();
            var newHash = "$2a$12$NEWabcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234";
            var originalUpdatedAt = user.UpdatedAt;

            // Act
            user.ChangePassword(newHash);

            // Asert
            user.PasswordHash.Should().Be(newHash);
            user.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        }

        [Fact]
        public void ChangePassword_Should_Throw_When_New_Password_Is_Same()
        {
            // Arrange
            var user = new UserBuilder().Build();

            // Act
            Action act = () => user.ChangePassword(user.PasswordHash);

            // Assert
            act.Should().Throw<DomainException>()
                .Where(e => e.Code == DomainErrorCodes.UserCode.InvalidPassword);
        }
        #endregion

        #region ChangeRole
        [Fact]
        public void ChangeRole_Should_Update_Role_And_Timestamp()
        {
            // Arrange
            var user = new UserBuilder().Build();
            var originalUpdatedAt = user.UpdatedAt;


            // Act
            user.ChangeRole(UserRole.Employee);

            // Assert
            user.Role.Should().Be(UserRole.Employee);
            user.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        }

        [Fact]
        public void ChangeRole_Should_Throw_When_New_Role_Is_Same()
        {
            // Arrange
            var user = new UserBuilder()
                .Build();

            // Act
            Action act = () => user.ChangeRole(UserRole.Admin);

            // Assert
            act.Should().Throw<DomainException>()
                .Where(e => e.Code == DomainErrorCodes.UserCode.InvalidUserRole);
        }
        #endregion

        #region Activate
        [Fact]
        public void Activate_Should_Set_IsActive_To_True_And_Update_Timestamp()
        {
            // Arrange
            var user = new UserBuilder().Build();
            var originalUpdatedAt = user.UpdatedAt;

            user.Deactivate();

            // Act
            user.Activate();

            // Assert
            user.IsActive.Should().BeTrue();
            user.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        }

        [Fact]
        public void Activate_Should_Throw_When_User_Is_Already_Active()
        {
            // Arrange
            var user = new UserBuilder()
                .Build();

            // Act 
            Action act = () => user.Activate();

            // Assert
            act.Should().Throw<DomainException>()
                .Where(e => e.Code == DomainErrorCodes.UserCode.AlreadyActive);
        }
        #endregion

        #region Deactivate
        [Fact]
        public void Deactivate_Should_Set_IsActive_To_False_And_Update_Timestamp()
        {
            // Arrange
            var user = new UserBuilder().Build();
            var originalUpdatedAt = user.UpdatedAt;

            // Act
            user.Deactivate();

            // Assert
            user.IsActive.Should().BeFalse();
            user.UpdatedAt.Should().BeAfter(originalUpdatedAt);
        }

        [Fact]
        public void Deactivate_Should_Throw_When_User_Is_Already_Inactive()
        {
            // Arrange
            var user = new UserBuilder()
                .Build();

            user.Deactivate();

            // Act
            Action act = () => user.Deactivate();

            // Assert
            act.Should().Throw<DomainException>()
                .Where(e => e.Code == DomainErrorCodes.UserCode.AlreadyInactive);
        }
        #endregion

    }
}
