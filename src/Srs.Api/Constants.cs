namespace Srs.Api;

public static class Constants
{
	public static byte[] JWT_DEFAULT_SECRET = "very-long-long-long-jwt-secret"u8.ToArray();
	public static string JWT_VALID_ISSUER = "srs.api";
	public static string JWT_VALID_AUDIENCE = "srs.api";
	public static string ADMIN_ROLE_NAME = "admin";
	public static string AGENT_ROLE_NAME = "agent";
}