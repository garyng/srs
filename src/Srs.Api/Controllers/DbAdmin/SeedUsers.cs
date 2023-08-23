using Bogus;
using MediatR;
using Srs.Api.Controllers.Auth;
using Srs.Api.Domain;

namespace Srs.Api.Controllers.DbAdmin;

public record SeedUsers(
	string AdminUserName = "admin",
	string AdminPassword = "admin",
	string AgentUserNamePrefix = "agent",
	string AgentUserPasswordPrefix = "agent",
	int AgentUserCount = 10
) : IRequest<SeedUsersResult>;

public record SeedUsersResult(
	User Admin,
	List<User> Agents
);

public class SeedUsersRequestHandler : IRequestHandler<SeedUsers, SeedUsersResult>
{
	private readonly SrsDbContext _db;
	private readonly IMediator _mediator;

	public SeedUsersRequestHandler(SrsDbContext db, IMediator mediator)
	{
		_db = db;
		_mediator = mediator;
	}

	public async Task<SeedUsersResult> Handle(SeedUsers request, CancellationToken cancellationToken)
	{
		var adminRole = new UserRole { Id = 0, Name = Constants.ADMIN_ROLE_NAME };
		var agentRole = new UserRole { Id = 0, Name = Constants.AGENT_ROLE_NAME };
		await _db.UserRoles.AddRangeAsync(adminRole, agentRole);

		var adminUser = new User
		{
			Id = 0,
			Name = request.AdminUserName,
			PasswordHash = await _mediator.Send(new GetHashOf(request.AdminPassword), cancellationToken),
			Roles = new List<UserRole> { adminRole }
		};
		var agentUsers = new Faker<User>()
			.Rules((f, u) =>
			{
				u.Name = $"{request.AgentUserNamePrefix}{f.IndexVariable}";
				u.PasswordHash = _mediator.Send(new GetHashOf($"{request.AgentUserPasswordPrefix}{f.IndexVariable}"), cancellationToken).GetAwaiter().GetResult();
				u.ReportingManager = adminUser;
				u.Roles = new List<UserRole> { agentRole };
				f.IndexVariable++;
			})
			.Generate(request.AgentUserCount);
		await _db.Users.AddRangeAsync(agentUsers.Prepend(adminUser), cancellationToken);

		await _db.SaveChangesAsync(cancellationToken);

		return new SeedUsersResult(adminUser, agentUsers);
	}
}
