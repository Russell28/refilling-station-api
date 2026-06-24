using FluentAssertions;
using RefillingStation.Domain.ErrorCodes;
using RefillingStation.Domain.Exceptions;
using RefillingStation.Tests.Domain.Builders;

namespace RefillingStation.Tests.Domain
{
    public class EmployeeTests
    {
        [Fact]
        public void Activate_Should_SetIsActive_ToTrue_And_SetDeactivatedAt_ToNull()
        {
            var employee = new EmployeeBuilder().Build();
            employee.Deactivate();

            // Act
            employee.Activate();

            // Assert
            employee.IsActive.Should().BeTrue();
            employee.DeactivatedAt.Should().Be(null);
        }

        [Fact]
        public void Activate_Should_Throw_WhenAlreadyActive()
        {
            var employee = new EmployeeBuilder().Build();

            // Act
            Action act = () => employee.Activate();

            // Assert
            act.Should().Throw<DomainException>()
                .Where(e => e.Code == DomainErrorCodes.EmployeeCode.AlreadyActive);
        }

        [Fact]
        public void Deactivate_Should_SetIsActive_ToFalse_And_SetDeactivatedAt()
        {
            var employee = new EmployeeBuilder().Build();

            // Act
            employee.Deactivate();

            // Assert
            employee.IsActive.Should().BeFalse();
            employee.DeactivatedAt.Should().BeBefore(DateTime.UtcNow);
        }

        [Fact]
        public void Deactivate_Should_Throw_WhenAlreadyInactive()
        {
            var employee = new EmployeeBuilder().Build();
            employee.Deactivate();

            // Act
            Action act = () => employee.Deactivate();

            // Assert
            act.Should().Throw<DomainException>()
                .Where(e => e.Code == DomainErrorCodes.EmployeeCode.AlreadyInactive);
        }
    }
}
