using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SubscribeProvider.Data.Contexts;
using SubscribeProvider.Data.Entities;
using System.Linq.Expressions;

namespace SubscribeProvider.Data.Repositories
{
    public class SubscribeRepository
    {
        private readonly SubscribeDataContext _context;
        private readonly ILogger<SubscribeRepository> _logger;
        public SubscribeRepository(SubscribeDataContext context, ILogger<SubscribeRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> ExistsAsync(Expression<Func<SubscribeEntity, bool>> predicate)
        {
            try
            {
                var entityExists = await _context.Subscribers.AnyAsync(predicate);
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
                _context.Subscribers.Add(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR : SubscribeRepository.CreateAsync() :: {ex.Message}");
                return null!;
            }
        }


    }
}
