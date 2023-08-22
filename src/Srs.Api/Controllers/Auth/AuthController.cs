using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Srs.Api.Domain;

namespace Srs.Api.Controllers.Auth;

[ApiController]
[Route("[controller]/[action]")]
public class AuthController : ControllerBase
{
    private readonly SrsDbContext _db;

    public record TokenRequest(string UserName, string Password);
    public record TokenResponse(string Token, DateTime ExpiresAt);

    public AuthController(SrsDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<TokenResponse> Token([FromBody] TokenRequest request)
    {
        // todo: hash password
        var passwordHash = request.Password;
        var user = await _db.Users.SingleOrDefaultAsync(x => x.Name.Equals(request.UserName) && x.PasswordHash.Equals(passwordHash));

        if (user == null) throw new Exception("Invalid user credentials");

        await _db.Entry(user)
            .Collection(x => x.Roles)
            .LoadAsync();

        var handler = new JwtSecurityTokenHandler();
        var secret = Constants.JWT_DEFAULT_SECRET;
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(5);
        var claims = new List<Claim>
        {
            new("id", user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        }.Concat(user.Roles.Select(x => new Claim(ClaimTypes.Role, x.Name)));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = Constants.JWT_VALID_ISSUER,
            Audience = Constants.JWT_VALID_AUDIENCE,
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256Signature),
        };

        var token = handler.CreateToken(descriptor);
        var jwt = handler.WriteToken(token);

        return new TokenResponse(jwt, expires);
    }
}