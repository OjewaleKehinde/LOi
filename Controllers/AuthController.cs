using AutoMapper;
using LOi.DatabaseContext;
using LOi.Models;
using LOi.Services.UserService;
using LOi.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace LOi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public static User user = new User();
        private readonly IConfiguration configuration;
        private readonly IUserService userService;
        private readonly AppDbContext dbContext;
        private readonly ILogger<AuthController> logger;
        private readonly IMapper mapper;



        public AuthController(IConfiguration configuration, IUserService userService, IMapper mapper, AppDbContext dbContext, ILogger<AuthController> logger)
        {
            this.configuration = configuration;
            this.userService = userService;
            this.dbContext = dbContext;
            this.logger = logger;
            this.mapper = mapper;

        }

        [HttpGet, Authorize]
        public ActionResult<string> GetUser()
        {
            var Email = userService.GetEmail();
            return Ok(Email);
        }

        [HttpPost("signup")]
        public async Task<ActionResult<User>> SignUp(UserDto userDto)
        {
            CreatePasswordHash(userDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            user.Email = userDto.Email;
            user.ID = Guid.NewGuid();
            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.PhoneNumber = userDto.PhoneNumber;
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            UserValidator userValidator = new();
            var validatorResult = userValidator.Validate(user);
            if (validatorResult.IsValid)
            {
                logger.LogInformation($"New user with ID {user.ID} signed up at {DateTime.UtcNow.AddHours(1)}");
                await dbContext.UserDataTable.AddAsync(user);
                await dbContext.SaveChangesAsync();
                return Ok(user);
            }

            return StatusCode(StatusCodes.Status400BadRequest, validatorResult.Errors);
        }

        [HttpDelete, Authorize(Roles = "User")]
        [Route("delete")]
        public async Task<IActionResult> DeleteUser()
        {
            var Email = userService.GetEmail();
            var User = await dbContext.UserDataTable.FirstOrDefaultAsync(x => x.Email == Email);
            Guid userID = User.ID;
            dbContext.OrderTable.RemoveRange(dbContext.OrderTable.Where(x => x.User == User));
            dbContext.UserDataTable.Remove(User);
            await dbContext.SaveChangesAsync();
            logger.LogInformation($"Account with ID {userID} deleted at {DateTime.UtcNow.AddHours(1)}");
            return Ok("Account was deleted successfully");

        }


        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginDto userDto)
        {

            var Email = userService.GetEmail();
            var user = await dbContext.UserDataTable.FirstOrDefaultAsync(x => x.Email == userDto.Email);
            if (user == null)
            {
                logger.LogInformation($"Attempted login failed at {DateTime.UtcNow.AddHours(1)}. Incorrect email");
                return BadRequest("User not found.");

            }

            if (!VerifyPasswordHash(userDto.Password, user.PasswordHash, user.PasswordSalt))
            {
                logger.LogInformation($"Attempted login failed at {DateTime.UtcNow.AddHours(1)}. Incorrect password");
                return BadRequest("Wrong password.");

            }

            string token = CreateToken(user);

            var refreshToken = GenerateRefreshToken();
            SetRefreshToken(refreshToken);
            logger.LogInformation($"User with ID {user.ID} logged in at {DateTime.UtcNow.AddHours(1)}");

            return Ok(token);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<string>> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (!user.RefreshToken.Equals(refreshToken))
            {
                return Unauthorized("Invalid Refresh Token.");
            }
            else if (user.TokenExpires < DateTime.Now)
            {
                return Unauthorized("Token expired.");
            }

            string token = CreateToken(user);
            var newRefreshToken = GenerateRefreshToken();
            SetRefreshToken(newRefreshToken);

            return Ok(token);
        }

        private RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddHours(12),
                Created = DateTime.Now
            };

            return refreshToken;
        }

        private void SetRefreshToken(RefreshToken newRefreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = newRefreshToken.Expires
            };
            Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);

            user.RefreshToken = newRefreshToken.Token;
            user.TokenCreated = newRefreshToken.Created;
            user.TokenExpires = newRefreshToken.Expires;
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, "User")
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(5),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}