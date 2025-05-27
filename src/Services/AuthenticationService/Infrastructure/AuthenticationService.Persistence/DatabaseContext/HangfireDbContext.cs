using Microsoft.EntityFrameworkCore;

namespace Services.Core.DatabaseContext
{
    public class HangfireDbContext : BaseDatabaseContext<HangfireDbContext>
    {
        public HangfireDbContext(DbContextOptions<HangfireDbContext> options) : base(options)
        {
        }

        // note: Hangfire creates its own tables so no DbSets are needed heree
        // This context is just for database creation and EF migrations
    }
}