using GamefiedSelfImprovement;
using Microsoft.EntityFrameworkCore;

namespace Gamified_Self_Improvement.Repositories;

/// <summary>
/// Entity Framework repository za aktivnosti - koristi realnu bazu podataka
/// </summary>
public class ActivityRepository
{
    private readonly GamefiedSelfImprovementDbContext _context;

    public ActivityRepository(GamefiedSelfImprovementDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Vraća sve aktivnosti
    /// </summary>
    public List<Activity> GetAll()
    {
        return _context.Activities
            .Include(a => a.User)
            .ToList();
    }

    /// <summary>
    /// Dohvata aktivnost po ID-u
    /// </summary>
    public Activity? GetById(int id)
    {
        return _context.Activities
            .Include(a => a.User)
            .FirstOrDefault(a => a.Id == id);
    }

    /// <summary>
    /// Dohvata sve aktivnosti korisnika
    /// </summary>
    public List<Activity> GetByUserId(int userId)
    {
        return _context.Activities
            .Where(a => a.UserId == userId)
            .Include(a => a.User)
            .ToList();
    }

    /// <summary>
    /// Dohvata sve aktivnosti specifičnog tipa
    /// </summary>
    public List<Activity> GetByActivityType(ActivityType type)
    {
        return _context.Activities
            .Where(a => a.ActivityType == type)
            .Include(a => a.User)
            .ToList();
    }

    /// <summary>
    /// Sprema novu aktivnost
    /// </summary>
    public void Add(Activity activity)
    {
        _context.Activities.Add(activity);
        _context.SaveChanges();
    }

    /// <summary>
    /// Ažurira postojeću aktivnost
    /// </summary>
    public void Update(Activity activity)
    {
        _context.Activities.Update(activity);
        _context.SaveChanges();
    }

    /// <summary>
    /// Briše aktivnost po ID-u
    /// </summary>
    public void Delete(int id)
    {
        var activity = _context.Activities.Find(id);
        if (activity != null)
        {
            _context.Activities.Remove(activity);
            _context.SaveChanges();
        }
    }

    /// <summary>
    /// Dohvata sve vježbe
    /// </summary>
    public List<Exercise> GetAllExercises()
    {
        return _context.Exercises
            .Include(e => e.User)
            .ToList();
    }

    /// <summary>
    /// Dohvata sve meditacije
    /// </summary>
    public List<Meditation> GetAllMeditations()
    {
        return _context.Meditations
            .Include(m => m.User)
            .ToList();
    }

    /// <summary>
    /// Dohvata sve duhovne aktivnosti
    /// </summary>
    public List<SpiritualActivity> GetAllSpiritualActivities()
    {
        return _context.SpiritualActivities
            .Include(s => s.User)
            .Include(s => s.Book)
            .ToList();
    }

    /// <summary>
    /// Dohvata sve dnevnike
    /// </summary>
    public List<DailyJournal> GetAllJournals()
    {
        return _context.DailyJournals
            .Include(d => d.User)
            .ToList();
    }
}
