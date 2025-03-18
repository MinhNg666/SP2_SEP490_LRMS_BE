using LRMS_API;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;

namespace Repository.Implementations;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly LRMSDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericRepository(LRMSDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<T>();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        try
        {
            return await _dbSet.AsNoTracking().ToListAsync();
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public virtual async Task<T> GetByIdAsync(int id)
    {
        try
        {
            return await _dbSet.FindAsync(id);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task AddAsync(T entity)
    {
        try
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task UpdateAsync(T entity)
    {
        try
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task DeleteAsync(T entity)
    {
        try
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    public async Task DeleteRangeAsync(IEnumerable<T> entities)
    {
        if (entities == null || !entities.Any())
            throw new ArgumentException("Entities cannot be null or empty");

        _dbSet.RemoveRange(entities);
        await _context.SaveChangesAsync();
    }

}