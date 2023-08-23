using System.Security.Cryptography;
using System.Text;
using MediatR;

namespace Srs.Api.Controllers.Auth;

public record GetHashOf(string PlainText) : IRequest<string>;

public class GetHashOfRequestHandler : IRequestHandler<GetHashOf, string>
{
	public async Task<string> Handle(GetHashOf request, CancellationToken cancellationToken)
	{
		var text = Encoding.UTF8.GetBytes(request.PlainText);
		var hash = SHA256.HashData(text);
		return Convert.ToHexString(hash);
	}
}