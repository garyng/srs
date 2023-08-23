using MediatR;
using Srs.Api.Domain;

namespace Srs.Api.Controllers.DbAdmin;

public record RecreateDatabase : IRequest;

public class RecreateDatabaseRequestHandler : IRequestHandler<RecreateDatabase>
{
	private readonly SrsDbContext _db;

	public RecreateDatabaseRequestHandler(SrsDbContext db)
	{
		_db = db;
	}

	public async Task Handle(RecreateDatabase request, CancellationToken cancellationToken)
	{
		await _db.Database.EnsureDeletedAsync(cancellationToken);
		await _db.Database.EnsureCreatedAsync(cancellationToken);
	}
}
