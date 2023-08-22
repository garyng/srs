using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NSwag.Generation.Processors.Security;
using NSwag;
using Srs.Api.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(c =>
{
	c.Title = "Srs.Api";
	c.AddSecurity("bearer auth", new OpenApiSecurityScheme
	{
		Type = OpenApiSecuritySchemeType.Http,
		Scheme = JwtBearerDefaults.AuthenticationScheme,
		BearerFormat = "JWT",
		Description = "JWT Authorization using the Bearer scheme."
	});
	c.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("bearer auth"));
});

// config

var config = builder.Configuration.GetSection(nameof(SrsConfig)).Get<SrsConfig>();

// database

builder.Services.AddDbContext<SrsDbContext>(o =>
	o.UseSqlServer(config.DbConnectionString));

// jwt auth

builder.Services.AddAuthentication(o =>
{
	o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
	o.RequireHttpsMetadata = false;
	o.TokenValidationParameters = new TokenValidationParameters
	{
		ValidIssuer = Constants.JWT_VALID_ISSUER,
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(Constants.JWT_DEFAULT_SECRET),

		ValidAudience = Constants.JWT_VALID_AUDIENCE
	};
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseOpenApi();
	app.UseSwaggerUi3();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


public record SrsConfig
{
	public string DbConnectionString { get; set; }
}

public static class Constants
{
	public static byte[] JWT_DEFAULT_SECRET = "very-long-long-long-jwt-secret"u8.ToArray();
	public static string JWT_VALID_ISSUER = "srs.api";
	public static string JWT_VALID_AUDIENCE = "srs.api";
	public static string ADMIN_ROLE_NAME = "admin";
	public static string AGENT_ROLE_NAME = "agent";
}

public partial class Program {}