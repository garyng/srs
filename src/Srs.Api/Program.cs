using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Srs.Api.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
	o.AddSecurityDefinition("bearer auth", new OpenApiSecurityScheme
	{
		Type = SecuritySchemeType.Http,
		Scheme = JwtBearerDefaults.AuthenticationScheme,
		BearerFormat = "JWT",
		Description = "JWT Authorization using the Bearer scheme."
	});
	o.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "bearer auth" }
			},
			new string[] {}
		}
	});
});

// database

builder.Services.AddDbContext<SrsDbContext>(o =>
	o.UseSqlServer("Server=srs.db;Database=srs;User Id=sa;Password=123!@#qweQWE;Trust Server Certificate=True"));

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

// todo: add policy for roles
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


public static class Constants
{
	public static byte[] JWT_DEFAULT_SECRET = "very-long-long-long-jwt-secret"u8.ToArray();
	public static string JWT_VALID_ISSUER = "srs.api";
	public static string JWT_VALID_AUDIENCE = "srs.api";
	public static string ADMIN_ROLE_NAME = "admin";
	public static string AGENT_ROLE_NAME = "agent";
}