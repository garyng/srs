using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Srs.Api.Domain;

namespace Srs.Api.Controllers.Auth;

public record GenerateToken(string UserName, string Password) : IRequest<GenerateTokenResult>;

public class GenerateTokenRequestHandler : IRequestHandler<GenerateToken, GenerateTokenResult>
{
    private readonly SrsDbContext _db;
    private readonly IMediator _mediator;

    public GenerateTokenRequestHandler(SrsDbContext db, IMediator mediator)
    {
        _db = db;
        _mediator = mediator;
    }

    public async Task<GenerateTokenResult> Handle(GenerateToken request, CancellationToken cancellationToken)
    {
        var passwordHash = await _mediator.Send(new GetHashOf(request.Password), cancellationToken);
        var user = await _db.Users.SingleOrDefaultAsync(x => x.Name.Equals(request.UserName) && x.PasswordHash.Equals(passwordHash), cancellationToken);

        if (user == null) throw new Exception("Invalid user credentials");

        await _db.Entry(user)
            .Collection(x => x.Roles)
            .LoadAsync(cancellationToken);

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

        return new GenerateTokenResult(jwt, expires);
    }
}

public record GenerateTokenResult(string Token, DateTime ExpiresAt);

[ApiController]
[Route("[controller]/[action]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<GenerateTokenResult> Token([FromBody] GenerateToken request)
    {
        return await _mediator.Send(request);
    }
}