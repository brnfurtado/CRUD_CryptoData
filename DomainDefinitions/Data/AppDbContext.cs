using Microsoft.EntityFrameworkCore;
using DomainDefinitions.Models;

namespace DomainDefinitions.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<BookData> bookDatas { get; set; }
        public DbSet<Subscription> subscriptions { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }
}
