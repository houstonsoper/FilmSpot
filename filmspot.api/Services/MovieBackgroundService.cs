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
			var response = await httpClient.SendAsync(request, stoppingToken);
			response.EnsureSuccessStatusCode();
			var body = await response.Content.ReadAsStringAsync(stoppingToken);

			//Deserialize response
			var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
			var movieData = JsonSerializer.Deserialize<TmdbShowingsResponse>(body, options)
				?? throw new InvalidOperationException("Unable to fetch showings from TMDB");
			
			//Check if there is a difference between the API result and the movies stored in the db
			//If there is then set "IsShowing" to false for the ones that exist in the DB but not in the API 
			//This is because they are no longer showing in the cinema
			var currentShowings = await movieRepository.GetCurrentShowingsAsync();
			var movieTitlesFromApi = movieData.Results.Select(s => s.Title).ToList();
			
			var removedMovies = currentShowings.Where(m => !movieTitlesFromApi.Contains(m.Title)).ToList();
			if (removedMovies.Count > 0)
			{
				foreach (var movie in removedMovies)
				{
					movie.IsShowing = false;
				}
			}
			
			//Add new movies to the database
			var movieTitlesFromDb = currentShowings.Select(s => s.Title).ToList();
			var newMovies = movieData.Results
				.Where(m => !movieTitlesFromDb.Contains(m.Title))
				.Select(m => m.ToMovie())
				.ToList();

			if (newMovies.Count > 0)
			{
				foreach (var movie in newMovies)
				{
					await movieRepository.AddAsync(movie);
				}
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