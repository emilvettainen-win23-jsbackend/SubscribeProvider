using Microsoft.EntityFrameworkCore;
using SubscribeProvider.Infrastructure.Data.Entities;

namespace SubscribeProvider.Infrastructure.Data.Contexts;

public class SubscribeDataContext(DbContextOptions<SubscribeDataContext> options) : DbContext(options)
{
    public DbSet<SubscribeEntity> Subscribers { get; set; }
}
