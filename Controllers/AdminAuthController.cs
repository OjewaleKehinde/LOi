using AutoMapper;
using LOi.DatabaseContext;
using LOi.Models;
using LOi.Services.AdminService;
using LOi.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace LOi.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminAuthController : ControllerBase
    {
        public static Admin admin = new Admin();
        private readonly IConfiguration configuration;
        private readonly IAdminService adminService;
        private readonly AppDbContext dbContext;
        private readonly UserValidator userValidator;
        private readonly ILogger<AdminAuthController> logger;
        private readonly IMapper mapper;




        public AdminAuthController(IConfiguration configuration, IAdminService adminService, IMapper mapper, AppDbContext dbContext, ILogger<AdminAuthController> logger)
        {
            this.configuration = configuration;
            this.adminService = adminService;
            this.dbContext = dbContext;
            userValidator = new();
            this.logger = logger;
            this.mapper = mapper;

        }

        [HttpGet, Authorize]
        public ActionResult<string> GetAdmin()
        {
            var Name = adminService.GetName();
            return Ok(Name);
        }

        [HttpPost("create")]
        public async Task<ActionResult<Admin>> CreateAdmin(AdminDto adminDto)
        {
            CreatePasswordHash(adminDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            admin.Name = adminDto.Name;
            admin.ID = Guid.NewGuid();
            admin.PasswordHash = passwordHash;
            admin.PasswordSalt = passwordSalt;

            await dbContext.AdminDataTable.AddAsync(admin);
            await dbContext.SaveChangesAsync();
            logger.LogInformation($"New Admin with ID {admin.ID} created at {DateTime.UtcNow.AddHours(1)}");

            return Ok(admin);
        }

        [HttpDelete, Authorize(Roles = "Admin")]
        [Route("delete-user/{ID}")]
        public async Task<IActionResult> DeleteUser(Guid ID)
        {
            var User = await dbContext.UserDataTable.FirstOrDefaultAsync(x => x.ID == ID);

            if (User != null)
            {
                Guid userID = User.ID;
                dbContext.OrderTable.RemoveRange(dbContext.OrderTable.Where(x => x.User == User));
                dbContext.UserDataTable.Remove(User);
                await dbContext.SaveChangesAsync();
                logger.LogInformation($"Account with ID {userID} deleted at {DateTime.UtcNow.AddHours(1)}");
                return Ok($"Account of user with ID {userID} was deleted successfully");
            }

            return NotFound("User not fiund.Invalid user ID.");

        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(AdminDto adminDto)
        {
            var Name = adminService.GetName();
            var admin = await dbContext.AdminDataTable.FirstOrDefaultAsync(x => x.Name == adminDto.Name);
            if (admin == null)
            {
                return BadRequest("admin not found.");
            }

            if (!VerifyPasswordHash(adminDto.Password, admin.PasswordHash, admin.PasswordSalt))
            {
                return BadRequest("Wrong password.");
            }

            string token = CreateToken(admin);

            var refreshToken = GenerateRefreshToken();
            SetRefreshToken(refreshToken);
            logger.LogInformation($"Admin with ID {admin.ID} logged in at {DateTime.UtcNow.AddHours(1)}");
            return Ok(token);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<string>> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (!admin.RefreshToken.Equals(refreshToken))
            {
                return Unauthorized("Invalid Refresh Token.");
            }
            else if (admin.TokenExpires < DateTime.Now)
            {
                return Unauthorized("Token expired.");
            }

            string token = CreateToken(admin);
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

            admin.RefreshToken = newRefreshToken.Token;
            admin.TokenCreated = newRefreshToken.Created;
            admin.TokenExpires = newRefreshToken.Expires;
        }

        private string CreateToken(Admin admin)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, admin.Name),
                new Claim(ClaimTypes.Role, "Admin")
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