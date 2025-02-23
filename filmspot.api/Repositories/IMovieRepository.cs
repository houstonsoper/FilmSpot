using filmspot.api.Models;

namespace filmspot.api.Repositories;

public interface IMovieRepository : IGenericRepository<Movie>
{
	Task<Movie?> GetMovieByNameAsync (string movieName);
	Task<List<Movie>> GetCurrentShowingsAsync();
}