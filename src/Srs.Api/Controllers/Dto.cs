namespace Srs.Api.Controllers;

public class SaleTransactionRequestDto
{
	public List<SaleItemRequestDto> Items { get; set; }
}

public class SaleItemRequestDto
{
	public int ProductId { get; set; }
	public int Quantity { get; set; }
}

public class SaleTransactionResponseDto
{
	public int Id { get; set; }
	public decimal Total { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime LastUpdatedAt { get; set; }

	public List<SaleItemResponseDto> Items { get; set; }
	public UserResponseDto User { get; set; }
}

public class SaleItemResponseDto
{
	public int Id { get; set; }
	public ProductResponseDto Product { get; set; }
	public int Quantity { get; set; }
	public decimal Total { get; set; }
}

public class ProductResponseDto
{
	public int Id { get; set; }
	public string Name { get; set; }
	public string? Description { get; set; }
	public decimal UnitPrice { get; set; }
}

public class UserResponseDto
{
	public int Id { get; set; }
	public string Name { get; set; }
}