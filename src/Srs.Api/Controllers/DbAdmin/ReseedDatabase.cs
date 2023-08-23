using MediatR;

namespace Srs.Api.Controllers.DbAdmin;

public record ReseedDatabase : IRequest;

public class ReseedDatabaseRequestHandler : IRequestHandler<ReseedDatabase>
{
	private readonly IMediator _mediator;
	private readonly ILogger<ReseedDatabaseRequestHandler> _logger;

	public ReseedDatabaseRequestHandler(IMediator mediator, ILogger<ReseedDatabaseRequestHandler> logger)
	{
		_mediator = mediator;
		_logger = logger;
	}

	public async Task Handle(ReseedDatabase request, CancellationToken cancellationToken)
	{
		_logger.LogInformation("Reseeding database");
		await _mediator.Send(new RecreateDatabase(), cancellationToken);
		await _mediator.Send(new SeedUsers(), cancellationToken);
		await _mediator.Send(new SeedProducts(), cancellationToken);
		await _mediator.Send(new SeedSaleTransactions(), cancellationToken);
		_logger.LogInformation("Database reseeded");

	}
}
