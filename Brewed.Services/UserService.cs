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
        Task<bool> ConfirmEmailAsync(string token);
        Task<bool> ForgotPasswordAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
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
                throw new Exception("Email already exists");
            }

            var user = _mapper.Map<User>(userDto);
            user.PasswordHash = HashPassword(userDto.Password);
            user.EmailConfirmed = false;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            // Generate email confirmation token
            var token = GenerateToken();
            var userToken = new UserToken
            {
                UserId = user.Id,
                Token = token,
                TokenType = "EmailConfirmation",
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };

            await _context.UserTokens.AddAsync(userToken);
            await _context.SaveChangesAsync();

            // Send confirmation email
            await _emailService.SendEmailConfirmationAsync(user.Email, user.Name, token);

            return _mapper.Map<UserDto>(user);
        }

        public async Task<string> LoginAsync(UserLoginDto userDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userDto.Email);

            if (user == null || !VerifyPassword(userDto.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            // Optional: Require email confirmation
            // if (!user.EmailConfirmed)
            // {
            //     throw new UnauthorizedAccessException("Please confirm your email address");
            // }

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

            if (user == null)
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
                throw new Exception("Email already exists");
            }

            _mapper.Map(userDto, user);
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

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ConfirmEmailAsync(string token)
        {
            var userToken = await _context.UserTokens
                .Include(ut => ut.User)
                .FirstOrDefaultAsync(ut => ut.Token == token && ut.TokenType == "EmailConfirmation");

            if (userToken == null)
            {
                throw new Exception("Invalid token");
            }

            if (userToken.IsUsed)
            {
                throw new Exception("Token already used");
            }

            if (userToken.ExpiresAt < DateTime.UtcNow)
            {
                throw new Exception("Token expired");
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
                // Don't reveal if user exists
                return true;
            }

            var token = GenerateToken();
            var userToken = new UserToken
            {
                UserId = user.Id,
                Token = token,
                TokenType = "PasswordReset",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            await _context.UserTokens.AddAsync(userToken);
            await _context.SaveChangesAsync();

            await _emailService.SendPasswordResetAsync(user.Email, user.Name, token);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var userToken = await _context.UserTokens
                .Include(ut => ut.User)
                .FirstOrDefaultAsync(ut => ut.Token == token && ut.TokenType == "PasswordReset");

            if (userToken == null)
            {
                throw new Exception("Invalid token");
            }

            if (userToken.IsUsed)
            {
                throw new Exception("Token already used");
            }

            if (userToken.ExpiresAt < DateTime.UtcNow)
            {
                throw new Exception("Token expired");
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