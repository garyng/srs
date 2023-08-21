using Microsoft.EntityFrameworkCore;
using Srs.Api.Domain;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<SrsDbContext>(o =>
	o.UseSqlServer("Server=srs.db;Database=srs;User Id=sa;Password=123!@#qweQWE;Trust Server Certificate=True"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
