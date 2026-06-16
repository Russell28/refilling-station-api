using FluentValidation;
using RefillingStation.Application.DTOs.Users;
using RefillingStation.Application.Interfaces.Repositories;
using RefillingStation.Application.Interfaces.Services;
using RefillingStation.Application.Validators;
using RefillingStation.Domain.Entities;
using RefillingStation.Domain.ErrorCodes;
using RefillingStation.Domain.Exceptions;

namespace RefillingStation.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IValidator<UserCreateRequest> _createValidator;
        private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;
        private readonly IValidator<ChangeUserRoleRequest> _changeRoleValidator;
        


        public UserService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IRefreshTokenRepository refreshTokenRepository,
            IValidator<UserCreateRequest> createValidator,
            IValidator<ChangePasswordRequest> changePasswordValidator,
            IValidator<ChangeUserRoleRequest> changeRoleValidator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _refreshTokenRepository = refreshTokenRepository;
            _createValidator = createValidator;
            _changePasswordValidator = changePasswordValidator;
            _changeRoleValidator = changeRoleValidator;
        }

        public async Task<List<UserDetailResponse>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync();

            return users
                .Select(x => new UserDetailResponse
                (
                    x.Id,
                    x.Username,
                    x.Role.ToString(),
                    x.IsActive
                ))
                .ToList();
        }

        public async Task<UserDetailResponse> GetByIdAsync(int id)
        {

            var user = await _userRepository.GetByIdAsync(id);

            if (user is null)
                throw new NotFoundException("Payroll", id);

            return new UserDetailResponse(
                user.Id,
                    user.Username,
                    user.Role.ToString(),
                    user.IsActive
            );
        }

        public async Task<int> CreateAsync(UserCreateRequest request)
        {
            var validation = await _createValidator.ValidateAsync(request);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var usernameExists = await _userRepository.GetByUsernameAsync(request.Username);

            if (usernameExists != null)
                throw new ConflictException($"Username '{request.Username}' already exists.");

            var hash = _passwordHasher.Hash(request.Password);

            var user = new User(
                request.Username,
                hash,
                request.Role
            );

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return user.Id;
        }

        public async Task ChangePasswordAsync(int id, ChangePasswordRequest request)
        {
            var validation = await _changePasswordValidator.ValidateAsync(request);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new NotFoundException("User", id);

            var hash = _passwordHasher.Hash(request.Password);

            user.ChangePassword(hash);

            await _userRepository.SaveChangesAsync();
        }

        public async Task ChangeRoleAsync(int id, ChangeUserRoleRequest request)
        {
            var validation = await _changeRoleValidator.ValidateAsync(request);

            if (!validation.IsValid)
                throw new ValidationException(validation.Errors);

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new NotFoundException("User", id);

            user.ChangeRole(request.Role);

            await _userRepository.SaveChangesAsync();
        }

        public async Task ActivateAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new NotFoundException("User", id);

            user.Activate();

            await _userRepository.SaveChangesAsync();
        }

        public async Task DeactivateAsync(int targetUserId, int requestingUserId)
        {
            if (targetUserId == requestingUserId)
                throw new DomainException(DomainErrorCodes.UserCode.CannotDeleteSelf, "You cannot deactivate your own account.");

            var user = await _userRepository.GetByIdAsync(targetUserId);

            if (user == null) 
                throw new NotFoundException("User", targetUserId);

            user.Deactivate();

            // Delete tokens after deactivate
            await _refreshTokenRepository.DeleteAllByUserIdAsync(user.Id);
            await _userRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int targetUserId, int requestingUserId)
        {
            if (targetUserId == requestingUserId)
                throw new DomainException(DomainErrorCodes.UserCode.CannotDeleteSelf, "You cannot delete your own account.");

            var user = await _userRepository.GetByIdAsync(targetUserId);

            if (user == null)
                throw new NotFoundException("User", targetUserId);

            _userRepository.Remove(user);
            await _userRepository.SaveChangesAsync();
        }
    }
}
