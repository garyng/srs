using Microsoft.EntityFrameworkCore;

namespace Srs.Api.Domain;

public class SrsDbContext : DbContext
{
	public DbSet<User> Users { get; set; } = null!;
	public DbSet<UserRole> UserRoles { get; set; } = null!;
	public DbSet<SaleTransaction> SaleTransactions { get; set; } = null!;
	public DbSet<SaleItem> SaleItems { get; set; } = null!;
	public DbSet<Product> Products { get; set; } = null!;

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