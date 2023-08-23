namespace Srs.Api;

public record SrsConfig
{
	public string DbConnectionString { get; set; }
	public bool ShouldSeedDatabase { get; set; } = false;
}