using GamefiedSelfImprovement;

namespace Gamified_Self_Improvement.Repositories;

/// <summary>
/// Mock repository za aktivnosti
/// </summary>
public class ActivityMockRepository
{
    private readonly UserMockRepository _userRepository;
    private readonly List<Activity> _activities;

    public ActivityMockRepository(UserMockRepository userRepository)
    {
        _userRepository = userRepository;
        _activities = InitializeMockActivities();
    }

    /// <summary>
    /// Vraća sve aktivnosti
    /// </summary>
    public List<Activity> GetAll()
    {
        return _activities.ToList();
    }

    /// <summary>
    /// Vraća aktivnosti za određenog korisnika
    /// </summary>
    public List<Activity> GetByUserId(int userId)
    {
        return _activities.Where(a => a.UserId == userId).ToList();
    }

    /// <summary>
    /// Dohvata aktivnost po ID-u
    /// </summary>
    public Activity? GetById(int id)
    {
        return _activities.FirstOrDefault(a => a.Id == id);
    }

    /// <summary>
    /// Vraća sve aktivnosti određenog tipa
    /// </summary>
    public List<Activity> GetByType(ActivityType type)
    {
        return _activities.Where(a => a.ActivityType == type).ToList();
    }

    private List<Activity> InitializeMockActivities()
    {
        var activities = new List<Activity>();
        var users = _userRepository.GetAll();

        // Prikupi sve aktivnosti iz svих korisnika
        foreach (var user in users)
        {
            foreach (var activity in user.Activities)
            {
                activities.Add(activity);
            }
        }

        return activities;
    }
}
