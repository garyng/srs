using MediatR;

namespace Srs.Api.Controllers.DbAdmin;

public record SeedDatabase(bool Recreate = false) : IRequest;

public class SeedDatabaseRequestHandler : IRequestHandler<SeedDatabase>
{
	private readonly IMediator _mediator;
	private readonly ILogger<SeedDatabaseRequestHandler> _logger;

	public SeedDatabaseRequestHandler(IMediator mediator, ILogger<SeedDatabaseRequestHandler> logger)
	{
		_mediator = mediator;
		_logger = logger;
	}

	public async Task Handle(SeedDatabase request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Seeding database");

		if (request.Recreate)
		{
			_logger.LogInformation("Recreating database");
			await _mediator.Send(new RecreateDatabase(), cancellationToken);
			_logger.LogInformation("Database recreated");
		}

		await _mediator.Send(new SeedUsers(), cancellationToken);
		await _mediator.Send(new SeedProducts(), cancellationToken);
		await _mediator.Send(new SeedSaleTransactions(), cancellationToken);
		_logger.LogInformation("Database reseeded");
	}
}