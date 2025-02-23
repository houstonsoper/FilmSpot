namespace filmspot.api.Models;

public class TmdbShowingsResponse
{
	public List<TmdbMovie> Results { get; set; } = [];
	public int Page { get; set; }
	public int TotalResults { get; set; }
	public int TotalPages { get; set; }
}