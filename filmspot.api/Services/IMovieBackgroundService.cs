namespace filmspot.api.Services;

public interface IMovieBackgroundService
{
	protected Task FetchMoviesAsync(CancellationToken stoppingToken);
}