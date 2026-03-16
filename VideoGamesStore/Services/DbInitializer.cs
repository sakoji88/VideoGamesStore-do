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
    DECLARE @ConstraintName nvarchar(128);

    SELECT TOP(1) @ConstraintName = cc.name
    FROM sys.check_constraints cc
    JOIN sys.tables t ON cc.parent_object_id = t.object_id
    WHERE t.name = N'Reviews'
      AND cc.definition LIKE N'%[[]Rating[]]%';

    IF @ConstraintName IS NOT NULL
    BEGIN
        EXEC(N'ALTER TABLE dbo.Reviews DROP CONSTRAINT [' + @ConstraintName + N']');
    END

    IF NOT EXISTS (
        SELECT 1
        FROM sys.check_constraints cc
        JOIN sys.tables t ON cc.parent_object_id = t.object_id
        WHERE t.name = N'Reviews'
          AND cc.name = N'CK_Reviews_Rating'
    )
    BEGIN
        ALTER TABLE dbo.Reviews
        ADD CONSTRAINT CK_Reviews_Rating CHECK ([Rating] >= 1 AND [Rating] <= 10);
    END
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
