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

                var executePerm = new Permission
                {
                    Name = "Execute",
                    Description = ""
                };

                var noPerm = new Permission
                {
                    Name = "No permission",
                    Description = ""
                };

                var adminGroup = new Group
                {
                    Name = "Admin",
                    Description = "Role with highest level of permissions"
                    ,
                    Permissions = { readPerm, writePerm, executePerm }
                };

                var managerGroup = new Group
                {
                    Name = "Manager",
                    Description = "Role with second highest level of permissions",
                    Permissions = { readPerm, executePerm }
                };

                var standardUserGroup = new Group
                {
                    Name = "Standard user",
                    Description = "Role with common level of permissions",
                    Permissions = { readPerm }
                };

                var noPermGroup = new Group
                {
                    Name = "No perm user",
                    Description = "Lowest level of permissions",
                    Permissions = {noPerm}
                };

                dbContext.Permissions.Add(readPerm);
                dbContext.Permissions.Add(writePerm);
                dbContext.Permissions.Add(executePerm);
                dbContext.Permissions.Add(noPerm);
                await dbContext.SaveChangesAsync();


                dbContext.Groups.Add(adminGroup);
                dbContext.Groups.Add(managerGroup);
                dbContext.Groups.Add(standardUserGroup);
                dbContext.Groups.Add(noPermGroup);
                await dbContext.SaveChangesAsync();

                dbContext.Users.Add(new User {UserName = "Administrator", Groups = { adminGroup }});
                dbContext.Users.Add(new User { UserName = "Kim Larsen", Groups = { managerGroup }});
                dbContext.Users.Add(new User { UserName = "Simon", Groups = { standardUserGroup }});
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
