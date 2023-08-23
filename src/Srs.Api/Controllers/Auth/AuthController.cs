using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Srs.Api.Controllers.Auth;

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