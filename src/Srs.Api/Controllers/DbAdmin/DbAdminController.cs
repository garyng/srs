using MediatR;
using Microsoft.AspNetCore.Mvc;
using Srs.Api.Controllers.Auth;

namespace Srs.Api.Controllers.DbAdmin;

[ApiController]
[Route("[controller]/[action]")]
[AuthorizeAdmin]
public class DbAdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public DbAdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task RecreateDatabase()
    {
        await _mediator.Send(new RecreateDatabase());
    }

    [HttpPost]
    public async Task SeedProducts(SeedProducts request)
    {
        await _mediator.Send(request);
    }

    [HttpPost]
    public async Task SeedUsers(SeedUsers request)
    {
        await _mediator.Send(request);
    }

    [HttpPost]
    public async Task SeedSaleTransactions(SeedSaleTransactions request)
    {
        await _mediator.Send(request);
    }
}