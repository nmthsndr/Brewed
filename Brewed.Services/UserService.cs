using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Brewed.DataContext.Context;
using Brewed.DataContext.Dtos;
using Brewed.DataContext.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Brewed.Services
{
    public interface IUserService
    {
        Task<UserDto> RegisterAsync(UserRegisterDto userDto);
        Task<string> LoginAsync(UserLoginDto userDto);
        Task<UserDto> GetUserByIdAsync(int userId);
        Task<UserDto> UpdateUserAsync(int userId, UserUpdateDto userDto);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> SoftDeleteUserAsync(int userId, int requestingUserId, bool isAdmin);
        Task<bool> ConfirmEmailAsync(string token);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<List<UserDto>> GetAllUsersAsync();
        Task<UserDto> CreateUserByAdminAsync(UserRegisterDto userDto);
        Task<UserDto> UpdateUserByAdminAsync(int userId, UserUpdateDto userDto);
    }

    public class UserService : IUserService
    {
        private readonly BrewedDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public UserService(BrewedDbContext context, IMapper mapper, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<UserDto> RegisterAsync(UserRegisterDto userDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
            {
                throw new Exception("This email is already associated with an account. Please use a different email or log in.");
            }

            var user = _mapper.Map<User>(userDto);
            user.PasswordHash = HashPassword(userDto.Password);
            user.EmailConfirmed = false;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Generate 6-digit verification code
            var verificationCode = GenerateVerificationCode();
            var userToken = new UserToken
            {
                UserId = user.Id,
                Token = verificationCode,
                TokenType = "EmailConfirmation",
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            await _context.UserTokens.AddAsync(userToken);
            await _context.SaveChangesAsync();

            // Send confirmation email with code
            await _emailService.SendEmailConfirmationAsync(user.Email, user.Name, verificationCode);

            return _mapper.Map<UserDto>(user);
        }

        private string GenerateVerificationCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        public async Task<string> LoginAsync(UserLoginDto userDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userDto.Email);

            if (user == null || !VerifyPassword(userDto.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            if (user.IsDeleted)
            {
                throw new UnauthorizedAccessException("This account has been deleted.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var expireDays = int.Parse(_configuration["Jwt:ExpireDays"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Name),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(expireDays),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<UserDto> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null || user.IsDeleted)
            {
                throw new KeyNotFoundException("User not found");
            }

            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> UpdateUserAsync(int userId, UserUpdateDto userDto)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            if (await _context.Users.AnyAsync(u => u.Email == userDto.Email && u.Id != userId))
            {
                throw new Exception("This email is already in use. Please use a different email.");
            }

            user.Name = userDto.Name;
            user.Email = userDto.Email;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            // Check if user has orders (cannot delete users with orders to maintain order history)
            var hasOrders = await _context.Orders
                .AnyAsync(o => o.UserId == userId);

            if (hasOrders)
            {
                throw new InvalidOperationException("Cannot delete user with existing orders. User has order history that must be preserved.");
            }

            // Delete related CartItems first
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart != null)
            {
                var cartItems = await _context.CartItems
                    .Where(ci => ci.CartId == cart.Id)
                    .ToListAsync();

                if (cartItems.Any())
                {
                    _context.CartItems.RemoveRange(cartItems);
                }

                _context.Carts.Remove(cart);
            }

            // Delete related Addresses
            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId)
                .ToListAsync();

            if (addresses.Any())
            {
                _context.Addresses.RemoveRange(addresses);
            }

            // Delete related Reviews
            var reviews = await _context.Reviews
                .Where(r => r.UserId == userId)
                .ToListAsync();

            if (reviews.Any())
            {
                _context.Reviews.RemoveRange(reviews);
            }

            // Delete related UserTokens
            var userTokens = await _context.UserTokens
                .Where(ut => ut.UserId == userId)
                .ToListAsync();

            if (userTokens.Any())
            {
                _context.UserTokens.RemoveRange(userTokens);
            }

            // Delete related UserCoupons
            var userCoupons = await _context.UserCoupons
                .Where(uc => uc.UserId == userId)
                .ToListAsync();

            if (userCoupons.Any())
            {
                _context.UserCoupons.RemoveRange(userCoupons);
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SoftDeleteUserAsync(int userId, int requestingUserId, bool isAdmin)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null || user.IsDeleted)
            {
                throw new KeyNotFoundException("User not found");
            }

            // Admin can delete any non-admin user; users can delete themselves
            if (isAdmin)
            {
                if (user.Role == "Admin")
                {
                    throw new InvalidOperationException("Cannot delete an admin user.");
                }
            }
            else
            {
                if (userId != requestingUserId)
                {
                    throw new UnauthorizedAccessException("You can only delete your own account.");
                }
            }

            // Soft delete: set IsDeleted flag
            user.IsDeleted = true;
            _context.Users.Update(user);

            // Remove user coupon assignments
            var userCoupons = await _context.UserCoupons
                .Where(uc => uc.UserId == userId)
                .ToListAsync();

            if (userCoupons.Any())
            {
                _context.UserCoupons.RemoveRange(userCoupons);
            }

            await _context.SaveChangesAsync();

            // Send deletion notification email
            try
            {
                await _emailService.SendAccountDeletionAsync(user.Email, user.Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send account deletion email to {user.Email}: {ex.Message}");
            }

            return true;
        }

        public async Task<bool> ConfirmEmailAsync(string code)
        {
            var userToken = await _context.UserTokens
                .Include(ut => ut.User)
                .FirstOrDefaultAsync(ut => ut.Token == code && ut.TokenType == "EmailConfirmation");

            if (userToken == null)
            {
                throw new Exception("Invalid verification code");
            }

            if (userToken.IsUsed)
            {
                throw new Exception("Verification code already used");
            }

            if (userToken.ExpiresAt < DateTime.UtcNow)
            {
                throw new Exception("Verification code expired");
            }

            userToken.User.EmailConfirmed = true;
            userToken.IsUsed = true;

            _context.Users.Update(userToken.User);
            _context.UserTokens.Update(userToken);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                throw new Exception("No account found with this email address.");
            }

            if (user.IsDeleted)
            {
                throw new Exception("This account has been deleted. Password reset is not available.");
            }

            var verificationCode = GenerateVerificationCode();

            var userToken = new UserToken
            {
                UserId = user.Id,
                Token = verificationCode,
                TokenType = "PasswordReset",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            await _context.UserTokens.AddAsync(userToken);
            await _context.SaveChangesAsync();

            await _emailService.SendPasswordResetAsync(user.Email, user.Name, verificationCode);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(string code, string newPassword)
        {
            var userToken = await _context.UserTokens
                .Include(ut => ut.User)
                .FirstOrDefaultAsync(ut => ut.Token == code && ut.TokenType == "PasswordReset");

            if (userToken == null)
            {
                throw new Exception("Invalid verification code");
            }

            if (userToken.IsUsed)
            {
                throw new Exception("Verification code already used");
            }

            if (userToken.ExpiresAt < DateTime.UtcNow)
            {
                throw new Exception("Verification code expired");
            }

            if (userToken.User.IsDeleted)
            {
                throw new Exception("This account has been deleted. Password reset is not available for deleted accounts.");
            }

            userToken.User.PasswordHash = HashPassword(newPassword);
            userToken.IsUsed = true;

            _context.Users.Update(userToken.User);
            _context.UserTokens.Update(userToken);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            if (!VerifyPassword(currentPassword, user.PasswordHash))
            {
                throw new Exception("Current password is incorrect");
            }

            user.PasswordHash = HashPassword(newPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Where(u => !u.IsDeleted)
                .ToListAsync();
            return _mapper.Map<List<UserDto>>(users);
        }

        public async Task<UserDto> CreateUserByAdminAsync(UserRegisterDto userDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
            {
                throw new Exception("This email is already associated with an account. Please use a different email or log in.");
            }

            var user = _mapper.Map<User>(userDto);
            user.PasswordHash = HashPassword(userDto.Password);
            user.EmailConfirmed = true; // Admin created users are auto-confirmed

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto> UpdateUserByAdminAsync(int userId, UserUpdateDto userDto)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            if (await _context.Users.AnyAsync(u => u.Email == userDto.Email && u.Id != userId))
            {
                throw new Exception("This email is already in use. Please use a different email.");
            }

            user.Name = userDto.Name;
            user.Email = userDto.Email;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }

        private string GenerateToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
    }
}