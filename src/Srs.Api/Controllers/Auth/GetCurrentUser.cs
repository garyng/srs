using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Srs.Api.Domain;

namespace Srs.Api.Controllers.Auth;

public record GetCurrentUser : IRequest<User>;

public class GetCurrentUserRequestHandler : IRequestHandler<GetCurrentUser, User>
{
	private readonly IHttpContextAccessor _accessor;
	private readonly SrsDbContext _db;

	public GetCurrentUserRequestHandler(IHttpContextAccessor accessor, SrsDbContext db)
	{
		_accessor = accessor;
		_db = db;
	}

	public async Task<User> Handle(GetCurrentUser request, CancellationToken cancellationToken)
	{
		var sub = _accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		if (sub == null) throw new UnauthorizedAccessException("Invalid user");
		if (!int.TryParse(sub, out var id)) throw new UnauthorizedAccessException("Invalid user");

		var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
		if (user == null) throw new UnauthorizedAccessException("Invalid user");
		return user;
	}
}
