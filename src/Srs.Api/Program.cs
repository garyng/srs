using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NSwag.Generation.Processors.Security;
using NSwag;
using Srs.Api;
using Srs.Api.Controllers;
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
if (RuntimeContext.IsIntegrationTests)
{
	builder.Services.AddDbContext<SrsDbContext>(o =>
		o.UseSqlServer(config.DbConnectionString), ServiceLifetime.Singleton, ServiceLifetime.Singleton);
} else
{
	builder.Services.AddDbContext<SrsDbContext>(o =>
		o.UseSqlServer(config.DbConnectionString));
}

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

// mediatr

builder.Services.AddMediatR(c => c.RegisterServicesFromAssemblyContaining<IMediatorMarker>());

var app = builder.Build();

// seeding

if (config.ShouldSeedDatabase)
{
	using var scope = app.Services.CreateScope();
	var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
	await mediator.Send(new ReseedDatabase());
	return;
}

app.UseOpenApi();
app.UseSwaggerUi3();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}