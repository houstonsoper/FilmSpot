using filmspot.api.Repositories;
using filmspot.api.Services;
using Microsoft.AspNetCore.Mvc;

namespace filmspot.api.Controllers;
[ApiController]
[Route("[controller]")]

public class MovieController : ControllerBase
{
	private readonly IMovieRepository _movieRepository;

	public MovieController(IMovieRepository movieRepository)
	{
		_movieRepository = movieRepository;
	}
	
	[HttpGet]
	public async Task<IActionResult> GetMovies()
	{
		var movies = await _movieRepository.GetAllAsync();
		return Ok(movies);
	}
}
