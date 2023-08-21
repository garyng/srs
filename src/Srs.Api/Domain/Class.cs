using Microsoft.EntityFrameworkCore;

namespace Srs.Api.Domain;

public class User
{
	public required int Id { get; set; }

	public required string Name { get; set; }
	public required string PasswordHash { get; set; }

	public User? ReportingManager { get; set; }
	public required ICollection<UserRole> Roles { get; set; }
}

public class UserRole
{
	public required int Id { get; set; }
	public required string Name { get; set; }
}

public class SaleTransaction
{
	public int Id { get; set; }
	public required double Total { get; set; }
	public required DateTime CreatedAt { get; set; }
	public required DateTime LastUpdatedAt { get; set; }


	public required ICollection<SaleItem> Items { get; set; }
	public required User User { get; set; }
}

public class SaleItem
{
	public required int Id { get; set; }

	public required Product Product { get; set; }
	public required double Quantity { get; set; }
	public required double Total { get; set; }
}

public class Product
{
	public required int Id { get; set; }
	public required string Name { get; set; }
	public string? Description { get; set; }

	public required double UnitPrice { get; set; }
}


public class SrsDbContext : DbContext
{
	public required DbSet<User> Users { get; set; }
	public required DbSet<UserRole> UserRoles { get; set; }
	public required DbSet<SaleTransaction> SaleTransactions { get; set; }
	public required DbSet<SaleItem> SaleItems { get; set; }
	public required DbSet<Product> Products { get; set; }

	public SrsDbContext(DbContextOptions<SrsDbContext> options) : base(options)
	{
	}
}