using filmspot.api.Models;

namespace filmspot.api.Extensions;

public static class MovieExtension
{
	public static Movie ToMovie(this TmdbMovie tmdbMovie)
	{
		return new Movie
		{
			Id = Guid.NewGuid(),
			Title = tmdbMovie.Title,
			Overview = tmdbMovie.Overview,
			PosterPath = "https://image.tmdb.org/t/p/original" + tmdbMovie.Poster_Path,
			ReleaseDate = DateTime.Parse(tmdbMovie.Release_Date)
		};
	}
}