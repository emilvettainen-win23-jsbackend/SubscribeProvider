using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SubscribeProvider.Infrastructure.Data.Contexts;
using SubscribeProvider.Infrastructure.Data.Entities;
using System.Linq.Expressions;

namespace SubscribeProvider.Infrastructure.Data.Repositories;

public class SubscribeRepository
{

    private readonly ILogger<SubscribeRepository> _logger;
    private readonly IDbContextFactory<SubscribeDataContext> _dbContextFaxtory;

    public SubscribeRepository(ILogger<SubscribeRepository> logger, IDbContextFactory<SubscribeDataContext> dbContextFaxtory)
    {
        _logger = logger;
        _dbContextFaxtory = dbContextFaxtory;
    }

    public async Task<bool> ExistsAsync(Expression<Func<SubscribeEntity, bool>> predicate)
    {
        try
        {
            await using var context = _dbContextFaxtory.CreateDbContext();
            var entityExists = await context.Subscribers.AnyAsync(predicate);
            return entityExists;
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : SubscribeRepository.ExistsAsync() :: {ex.Message}");
            return false;
        }
    }


    public async Task<SubscribeEntity> CreateAsync(SubscribeEntity entity)
    {
        try
        {
            await using var context = _dbContextFaxtory.CreateDbContext();
            context.Subscribers.Add(entity);
            await context.SaveChangesAsync();
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : SubscribeRepository.CreateAsync() :: {ex.Message}");
            return null!;
        }
    }


    public async Task<IEnumerable<SubscribeEntity>> GetAllAsync()
    {
        try
        {
            await using var context = _dbContextFaxtory.CreateDbContext();
            var entities = await context.Subscribers.ToListAsync();
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : SubscribeRepository.GetAllAsync() :: {ex.Message}");
            return Enumerable.Empty<SubscribeEntity>();
        }
    }

    public async Task<SubscribeEntity> GetOneAsync(Expression<Func<SubscribeEntity, bool>> predicate)
    {
        try
        {
            await using var context = _dbContextFaxtory.CreateDbContext();
            var entity = await context.Subscribers.FirstOrDefaultAsync(predicate);
            if (entity != null)
            {
                return entity;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : SubscribeRepository.GetOneAsync() :: {ex.Message}");
        }
        return null!;
    }

    public async Task<SubscribeEntity> UpdateSubscriptionAsync(Expression<Func<SubscribeEntity, bool>> predicate, SubscribeEntity updatedEntity)
    {
        try
        {
            await using var context = _dbContextFaxtory.CreateDbContext();
            var entity = await context.Subscribers.FirstOrDefaultAsync(predicate);
            if (entity != null && updatedEntity != null) 
            {
                context.Entry(entity).CurrentValues.SetValues(updatedEntity);
                await context.SaveChangesAsync();
                return entity;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : SubscribeRepository.UpdateSubscriptionAsync() :: {ex.Message}");
        }
        return null!;
    }

    public async Task<bool> DeleteSubscriptionAsync(Expression<Func<SubscribeEntity, bool>> predicate)
    {
        try
        {
            await using var context = _dbContextFaxtory.CreateDbContext();
            var entity = await context.Subscribers.FirstOrDefaultAsync(predicate);
            if (entity != null)
            {
                context.Subscribers.Remove(entity);
                await context.SaveChangesAsync();
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : SubscribeRepository.DeleteSubscriptionAsync() :: {ex.Message}");
        }
        return false;
    }
}