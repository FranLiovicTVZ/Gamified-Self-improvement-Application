namespace GamefiedSelfImprovement;

/// <summary>
/// Tipovi aktivnosti koje korisnik može obavljati
/// </summary>
public enum ActivityType
{
    Exercise,           // Tjelovježba
    Spiritual,          // Duhovni napredak
    Meditation,         // Meditacija
    Journal            // Dnevnik
}

/// <summary>
/// Tipovi vježbanja
/// </summary>
public enum ExerciseType
{
    Strength,          // Trening snage / teretana
    Cardio,            // Kardio (trčanje, biciklizam, itd)
    Flexibility,       // Rastezanje
    Sports            // Sport
}

/// <summary>
/// Tipovi duhovnih tekstova
/// </summary>
public enum SpiritualBookType
{
    Bible,
    Quran,
    Torah,
    BuddhистicScriptures,
    Hindu
}

/// <summary>
/// Tipovi meditacije
/// </summary>
public enum MeditationType
{
    Guided,            // Vođena meditacija
    Breathing,         // Disanje
    Mantras,           // Mantras
    Mindfulness,       // Svjesnost
    Visualization      // Vizualizacija
}

/// <summary>
/// Težina vježbe za ocjenjivanje
/// </summary>
public enum DifficultyLevel
{
    Easy,
    Medium,
    Hard,
    Extreme
}

/// <summary>
/// Stupnjevi bedževa za svaku vrstu aktivnosti
/// </summary>
public enum BadgeTier
{
    None,
    Bronze,
    Silver,
    Gold,
    Diamond,
    Champion
}
