using System.ComponentModel.DataAnnotations;

namespace filmspot.api.Models;

public class Movie
{
	public Guid Id { get; set; }
	
	[Required(ErrorMessage = "Title is required")]
	[StringLength(50, ErrorMessage = "Title cannot be longer than 100 characters")]
	public required string Title { get; set; }
	
	[Required(ErrorMessage = "Overview is required")]
	public required string Overview { get; set; }
	
	[Required(ErrorMessage = "Poster path is required")]
	[StringLength(500, ErrorMessage = "Post path cannot be longer than 500 characters")]
	public required string PosterPath { get; set; }
	
	[Required(ErrorMessage = "Release date is required")]
	public required DateTime ReleaseDate { get; set; }
}

//https://image.tmdb.org/t/p/original -> postpath  /pzIddUEMWhWzfvLI3TwxUG2wGoi.jpg