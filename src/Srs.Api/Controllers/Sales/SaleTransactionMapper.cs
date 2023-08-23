using Riok.Mapperly.Abstractions;
using Srs.Api.Domain;

namespace Srs.Api.Controllers.Sales;

[Mapper]
public static partial class SaleTransactionMapper
{
	public static partial SaleTransactionResponseDto ToResponseDto(this SaleTransaction transaction);
}