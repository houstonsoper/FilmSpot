using System.Text.Json;
using filmspot.api.Extensions;
using filmspot.api.Models;
using filmspot.api.Repositories;

namespace filmspot.api.Services;

public class MovieBackgroundService : BackgroundService, IMovieBackgroundService
{
	private readonly HttpClient _httpClient = new HttpClient();
	private readonly IMovieRepository _movieRepository;

	public MovieBackgroundService(IMovieRepository movieRepository)
	{
		_movieRepository = movieRepository;
	}
	
	public async Task FetchMoviesAsync(CancellationToken stoppingToken)
	{
		//Fetch showings from TMDB
		var request = new HttpRequestMessage
		{
			Method = HttpMethod.Get,
			RequestUri = new Uri("https://api.themoviedb.org/3/movie/now_playing?language=en&page=1"),
			Headers =
			{
				{ "accept", "application/json" },
				{
					"Authorization",
					"Bearer eyJhbGciOiJIUzI1NiJ9.eyJhdWQiOiJjZjkyMjFkZTAxMjU0YTA3ZDU2ZGNhM2ZlMTFmYWIwNyIsIm5iZiI6MTczNTI2MzMxMC4wMDgsInN1YiI6IjY3NmUwNDRlZjliYzAyZTQ5ODkyOTZmMCIsInNjb3BlcyI6WyJhcGlfcmVhZCJdLCJ2ZXJzaW9uIjoxfQ.K0jsaOOKSrjZOYAIUSG66p2EGOdP0bqhRAep8zVYlXI"
				}
			},
		};
		var response = await _httpClient.SendAsync(request);
		response.EnsureSuccessStatusCode();
		var body = await response.Content.ReadAsStringAsync();

		//Deserialize response
		var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
		var movieData = JsonSerializer.Deserialize<TmdbShowingsResponse>(body, options);

		if (movieData == null)
		{
			throw new InvalidOperationException("Unable to fetch showings from TMDB");
		}
		
		//Create movie from each result and add it to the db
		foreach (var tmdbMovie in movieData.Results)
		{
			var movie = tmdbMovie.ToMovie();
			
			//Check that it doesn't already exist
			var existingMovie = await _movieRepository.GetMovieByNameAsync(movie.Title);
			if (existingMovie != null) continue;
			
			await _movieRepository.AddAsync(movie);
		}
		await _movieRepository.SaveAsync();
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await FetchMoviesAsync(stoppingToken);

		while (!stoppingToken.IsCancellationRequested)
		{
			//Fetch movies at midnight
			var currentTime = DateTime.UtcNow;
			var midnight = currentTime.Date.AddDays(1);
			
			var delay = midnight - currentTime;
			
			await Task.Delay(delay, stoppingToken);
			
			await FetchMoviesAsync(stoppingToken);
		}
	}
}