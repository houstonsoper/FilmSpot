using filmspot.api.Contexts;
using filmspot.api.Models;
using Microsoft.EntityFrameworkCore;

namespace filmspot.api.Repositories;

public class MovieRepository : GenericRepository<Movie>, IMovieRepository
{
	public MovieRepository(FilmSpotDbContext context) : base(context)
	{
	}

	public async Task<Movie?> GetMovieByNameAsync(string movieName)
	{
		var query = GetAllQuery();
		return await query.FirstOrDefaultAsync(m => m.Title.Contains(movieName));
	}
}