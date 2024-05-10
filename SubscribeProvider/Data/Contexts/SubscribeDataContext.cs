using Microsoft.EntityFrameworkCore;
using SubscribeProvider.Data.Entities;

namespace SubscribeProvider.Data.Contexts;

public class SubscribeDataContext(DbContextOptions<SubscribeDataContext> options) : DbContext(options)
{
    public DbSet<SubscribeEntity> Subscribers { get; set; }
}
