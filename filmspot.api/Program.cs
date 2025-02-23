using filmspot.api.Contexts;
using filmspot.api.Repositories;
using filmspot.api.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<IMovieBackgroundService, MovieBackgroundService>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

//Connect SQL Database
builder.Services.AddDbContext<FilmSpotDbContext>(options =>
{
	options.UseSqlServer(
		builder.Configuration["ConnectionStrings:FilmSpotDbContextConnection"]);
});

// Enable CORS
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowSpecificOrigin", policy =>
	{
		policy.WithOrigins("http://localhost:3000") //front-end URL
			.AllowAnyHeader()
			.AllowAnyMethod()
			.AllowCredentials();
	});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference(options =>
	{
		options
			.WithTitle("Film Spot API")
			.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
	});
}
app.UseCors("AllowSpecificOrigin");
app.UseHttpsRedirection();
app.MapControllers();
app.Run();

