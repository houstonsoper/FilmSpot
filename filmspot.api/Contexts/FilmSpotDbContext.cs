using filmspot.api.Models;
using Microsoft.EntityFrameworkCore;

namespace filmspot.api.Contexts;

public class FilmSpotDbContext : DbContext
{
	public DbSet<Movie> Movies { get; set; }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		base.OnConfiguring(optionsBuilder);
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
	}

	public FilmSpotDbContext(DbContextOptions<FilmSpotDbContext> options) : base(options)
	{
		
	}
}