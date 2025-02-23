using filmspot.api.Contexts;
using Microsoft.EntityFrameworkCore;

namespace filmspot.api.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
	protected readonly FilmSpotDbContext _context;

	public GenericRepository(FilmSpotDbContext context)
	{
		_context = context;
	}

	public async Task<IEnumerable<T>> GetAllAsync()
	{
		return await _context.Set<T>().ToListAsync();
	}

	public IQueryable<T> GetAllQuery()
	{
		return _context.Set<T>().AsQueryable();
	}

	public async Task<T?> GetByIdAsync(Guid id)
	{
		return await _context.Set<T>().FindAsync(id);
	}

	public async Task AddAsync(T entity)
	{
		await _context.Set<T>().AddAsync(entity);
	}

	public void Update(T entity)
	{
		_context.Set<T>().Update(entity);
	}

	public void Delete(T entity)
	{
		_context.Set<T>().Remove(entity);
	}

	public async Task SaveAsync()
	{
		await _context.SaveChangesAsync();
	}
}