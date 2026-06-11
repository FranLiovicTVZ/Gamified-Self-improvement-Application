using GamefiedSelfImprovement;

namespace Gamified_Self_Improvement.Services;

/// <summary>
/// Izračun stupnja bedža za svaku vrstu aktivnosti.
/// Efektivni bodovi = activityXP * 1.05^streakDays (eksponencijalni bonus za redovitost).
/// Jedan propušteni dan resetira streak (handled u Streak entity).
/// </summary>
public static class BadgeCalculator
{
    private const double StreakBase = 1.05;

    public static BadgeTier Calculate(int activityXP, int streakDays)
    {
        if (activityXP <= 0) return BadgeTier.None;
        double effective = activityXP * Math.Pow(StreakBase, streakDays);
        return effective switch
        {
            >= 4500 => BadgeTier.Champion,
            >= 1800 => BadgeTier.Diamond,
            >= 750  => BadgeTier.Gold,
            >= 300  => BadgeTier.Silver,
            _       => BadgeTier.Bronze
        };
    }

    public static string GetEmoji(BadgeTier tier) => tier switch
    {
        BadgeTier.Bronze   => "🥉",
        BadgeTier.Silver   => "🥈",
        BadgeTier.Gold     => "🥇",
        BadgeTier.Diamond  => "💎",
        BadgeTier.Champion => "🏆",
        _                  => ""
    };

    public static string GetLabel(BadgeTier tier) => tier switch
    {
        BadgeTier.Bronze   => "Bronze",
        BadgeTier.Silver   => "Silver",
        BadgeTier.Gold     => "Gold",
        BadgeTier.Diamond  => "Diamond",
        BadgeTier.Champion => "Champion",
        _                  => "Nema bedža"
    };

    public static string GetColor(BadgeTier tier) => tier switch
    {
        BadgeTier.Bronze   => "#CD7F32",
        BadgeTier.Silver   => "#A8A9AD",
        BadgeTier.Gold     => "#FFD700",
        BadgeTier.Diamond  => "#60D0F0",
        BadgeTier.Champion => "#FF6B35",
        _                  => "#888"
    };
}
