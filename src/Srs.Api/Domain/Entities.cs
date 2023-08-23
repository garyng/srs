namespace Srs.Api.Domain;

public class User
{
	public required int Id { get; set; }

	public required string Name { get; set; }
	public required string PasswordHash { get; set; }

	public User? ReportingManager { get; set; }
	public ICollection<UserRole> Roles { get; set; } = new HashSet<UserRole>();
}

public class UserUserRole
{
	public int UserId { get; set; }
	public int UserRoleId { get; set; }
}

public class UserRole
{
	public required int Id { get; set; }
	public required string Name { get; set; }

	public ICollection<User> Users { get; set; } = new HashSet<User>();
}

public class SaleTransaction
{
	public int Id { get; set; }
	public decimal Total { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime LastUpdatedAt { get; set; }

	public ICollection<SaleItem> Items { get; set; } = new HashSet<SaleItem>();

	public int UserId { get; set; }
	public User User { get; set; }
}

public class SaleItem
{
	public int Id { get; set; }

	public int ProductId { get; set; }
	public Product Product { get; set; }
	public int Quantity { get; set; }
	public decimal Total { get; set; }
}

public class Product
{
	public required int Id { get; set; }
	public required string Name { get; set; }
	public string? Description { get; set; }

	public required decimal UnitPrice { get; set; }
}