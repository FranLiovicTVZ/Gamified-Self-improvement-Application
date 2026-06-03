using GamefiedSelfImprovement;
using Microsoft.EntityFrameworkCore;

namespace Gamified_Self_Improvement.Services;

/// <summary>
/// Sinkronizira AppUser (Identity) s legacy Users tablicom radi kompatibilnosti.
/// </summary>
public class UserSyncService
{
    private readonly GamefiedSelfImprovementDbContext _dbContext;

    public UserSyncService(GamefiedSelfImprovementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User> SyncFromAppUserAsync(AppUser appUser)
    {
        var legacyUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == appUser.Email);

        if (legacyUser == null)
        {
            legacyUser = new User
            {
                Username = appUser.UserName ?? appUser.Email ?? "user",
                Email = appUser.Email ?? string.Empty,
                CreatedDate = appUser.CreatedDate,
                TotalXP = appUser.TotalXP,
                Level = appUser.Level,
                Bio = appUser.Bio,
                ProfileImagePath = appUser.ProfileImagePath,
                PreferredMeditationType = appUser.PreferredMeditationType,
                FavoriteBooks = appUser.FavoriteBooks,
                StreakDays = appUser.StreakDays,
                LastActiveDate = appUser.LastActiveDate
            };

            _dbContext.Users.Add(legacyUser);
        }
        else
        {
            legacyUser.Username = appUser.UserName ?? legacyUser.Username;
            legacyUser.TotalXP = appUser.TotalXP;
            legacyUser.Level = appUser.Level;
            legacyUser.Bio = appUser.Bio;
            legacyUser.ProfileImagePath = appUser.ProfileImagePath;
            legacyUser.PreferredMeditationType = appUser.PreferredMeditationType;
            legacyUser.FavoriteBooks = appUser.FavoriteBooks;
            legacyUser.StreakDays = appUser.StreakDays;
            legacyUser.LastActiveDate = appUser.LastActiveDate;
            _dbContext.Users.Update(legacyUser);
        }

        await _dbContext.SaveChangesAsync();
        return legacyUser;
    }
}
