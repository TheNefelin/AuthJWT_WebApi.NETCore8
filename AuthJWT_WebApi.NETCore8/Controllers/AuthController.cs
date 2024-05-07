using AuthJWT_WebApi.NETCore8.Connection;
using AuthJWT_WebApi.NETCore8.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthJWT_WebApi.NETCore8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ApiDbContext _dapper;

        public AuthController(IConfiguration configuration, ApiDbContext dapper)
        {
            _configuration = configuration;
            _dapper = dapper;
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(
            AuthUserDTO user,
            CancellationToken cancellationToken)
        {
            var login = await _dapper.AuthIniciarSesion(user, cancellationToken);

            if (login.Estado)
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]!));
                var issuer = _configuration["JwtSettings:Issuer"];
                var audience = _configuration["JwtSettings:Audience"];
                var expire = _configuration["JwtSettings:ExpirationMinutes"];
                String TokenId = Guid.NewGuid().ToString();

                var tokenOptions = new JwtSecurityToken(
                           issuer: issuer,
                           audience: audience,
                           claims: new List<Claim> {
                                new (JwtRegisteredClaimNames.Jti, TokenId),
                                new (JwtRegisteredClaimNames.Sub, user.Email),
                                new (JwtRegisteredClaimNames.Email, user.Email),
                           },
                           expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(expire)),
                           signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
                       );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
                return Ok(new { Token = tokenString });
            }

            return Unauthorized();
        }

        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register(
            AuthUserDTO user,
            CancellationToken cancellationToken)
        {
            String Id = Guid.NewGuid().ToString();

            var newUser = await _dapper.AuthCrearUsuario(Id, user, cancellationToken);

            return Ok(newUser);
        }
    }
}
