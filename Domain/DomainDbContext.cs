using Microsoft.EntityFrameworkCore;

namespace Domain
{
    public class DomainDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public DbSet<Group> Groups { get; set; }

        public DbSet<Permission> Permissions { get; set; }

        public DomainDbContext(DbContextOptions<DomainDbContext> options) 
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(u => u.Groups)
                .WithMany(g => g.Users);

            modelBuilder.Entity<Group>()
                .HasMany(g => g.Permissions)
                .WithMany();

        }

    }

    public class DatabaseInitializer
    {
        public static async Task InitializeAsync(DomainDbContext dbContext)
        {
            await dbContext.Database.MigrateAsync();

            if (!dbContext.Users.Any())
            {
                
                var readPerm = new Permission
                {
                    Name = "Read",
                    Description = "Allows reading data"
                };

                var writePerm = new Permission
                {
                    Name = "Write",
                    Description = "Allows writing data"
                };

                var adminGroup = new Group
                {
                    Name = "Admin",
                    Description = "Role with highest level of permissions"
                    ,
                    Permissions = { readPerm, writePerm }
                };

                dbContext.Permissions.Add(readPerm);
                dbContext.Permissions.Add(writePerm);

                dbContext.Groups.Add(adminGroup);

                dbContext.Users.Add(new User {UserName = "Administrator", Groups = {adminGroup} });

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
