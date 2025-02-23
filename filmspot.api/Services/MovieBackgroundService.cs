using System.Text.Json;
using filmspot.api.Extensions;
using filmspot.api.Models;
using filmspot.api.Repositories;

namespace filmspot.api.Services;

public class MovieBackgroundService : BackgroundService
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly IServiceProvider _serviceProvider;

	public MovieBackgroundService(IServiceProvider serviceProvider, IHttpClientFactory httpClientFactory)
	{
		_serviceProvider = serviceProvider;
		_httpClientFactory = httpClientFactory;
	}

	public async Task FetchMoviesAsync(CancellationToken stoppingToken)
	{
		using (var scope = _serviceProvider.CreateScope())
		{
			var movieRepository = scope.ServiceProvider.GetRequiredService<IMovieRepository>();

			var httpClient = _httpClientFactory.CreateClient();

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
			var response = await httpClient.SendAsync(request);
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
				var existingMovie = await movieRepository.GetMovieByNameAsync(movie.Title);
				if (existingMovie != null) continue;

				await movieRepository.AddAsync(movie);
			}

			await movieRepository.SaveAsync();
		}
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await FetchMoviesAsync(stoppingToken);

		while (!stoppingToken.IsCancellationRequested)
		{
			//Fetch movies every hour
			await Task.Delay(TimeSpan.FromHours(1), stoppingToken); 
			await FetchMoviesAsync(stoppingToken); 
		}
	}
}