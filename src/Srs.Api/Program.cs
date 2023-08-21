using Microsoft.EntityFrameworkCore;
using Srs.Api.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<SrsDbContext>(o =>
	o.UseSqlServer("Server=srs.db;Database=srs;User Id=sa;Password=123!@#qweQWE;Trust Server Certificate=True"));

var app = builder.Build();

// todo: move db migration out
using (var scope = app.Services.CreateScope())
{
	var db = scope.ServiceProvider.GetRequiredService<SrsDbContext>();
	await db.Database.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
