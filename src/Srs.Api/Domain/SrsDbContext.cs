using Microsoft.EntityFrameworkCore;

namespace Srs.Api.Domain;

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

	protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
	{
		configurationBuilder.Properties<Decimal>()
			.HavePrecision(19, 6);
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<User>()
			.HasMany(x => x.Roles)
			.WithMany(x => x.Users)
			.UsingEntity<UserUserRole>();
	}
}