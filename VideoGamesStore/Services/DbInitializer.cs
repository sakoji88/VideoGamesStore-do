using Microsoft.EntityFrameworkCore;
using VideoGamesStore.Models;

namespace VideoGamesStore.Services;

public static class DbInitializer
{
    public static async Task SeedAsync(VideoGamesStoreContext context, IPasswordHasher hasher)
    {
        await context.Database.EnsureCreatedAsync();

        await context.Database.ExecuteSqlRawAsync(@"
IF OBJECT_ID(N'dbo.Reviews', N'U') IS NOT NULL
AND COL_LENGTH(N'dbo.Reviews', N'IsApproved') IS NULL
BEGIN
    ALTER TABLE dbo.Reviews
    ADD IsApproved bit NOT NULL
        CONSTRAINT DF_Reviews_IsApproved DEFAULT(0);
END");

        await context.Database.ExecuteSqlRawAsync(@"
IF OBJECT_ID(N'dbo.Reviews', N'U') IS NOT NULL
BEGIN
    DECLARE @sql nvarchar(max) = N'';

    SELECT @sql = @sql + N'ALTER TABLE dbo.Reviews DROP CONSTRAINT [' + cc.name + N'];'
    FROM sys.check_constraints cc
    WHERE cc.parent_object_id = OBJECT_ID(N'dbo.Reviews')
      AND (
           cc.name LIKE N'CK__Reviews__Rating%'
           OR cc.name = N'CK_Reviews_Rating'
           OR cc.definition LIKE N'%Rating%'
      );

    IF LEN(@sql) > 0
    BEGIN
        EXEC sp_executesql @sql;
    END

    ALTER TABLE dbo.Reviews
    ADD CONSTRAINT CK_Reviews_Rating CHECK ([Rating] >= 1 AND [Rating] <= 10);
END");

        var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
        if (userRole is null)
        {
            userRole = new Role { Name = "User" };
            context.Roles.Add(userRole);
        }

        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole is null)
        {
            adminRole = new Role { Name = "Admin" };
            context.Roles.Add(adminRole);
        }

        await context.SaveChangesAsync();

        var hasAdmin = await context.Users.AnyAsync(u => u.RoleId == adminRole.Id);
        if (!hasAdmin)
        {
            context.Users.Add(new User
            {
                Username = "admin",
                Email = "admin@videogamestore.local",
                PasswordHash = hasher.HashPassword("Admin123!"),
                RoleId = adminRole.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }
    }
}
