using GamefiedSelfImprovement;
using Microsoft.EntityFrameworkCore;

namespace Gamified_Self_Improvement.Repositories;

/// <summary>
/// Entity Framework repository za korisnike - koristi realnu bazu podataka
/// </summary>
public class UserRepository
{
    private readonly GamefiedSelfImprovementDbContext _context;

    public UserRepository(GamefiedSelfImprovementDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Vraća sve korisnike s njihovim aktivnostima
    /// </summary>
    public List<User> GetAll()
    {
        return _context.Users
            .Include(u => u.Activities)
            .Include(u => u.Journals)
            .ToList();
    }

    /// <summary>
    /// Dohvata korisnika po ID-u
    /// </summary>
    public User? GetById(int id)
    {
        return _context.Users
            .Include(u => u.Activities)
            .Include(u => u.Journals)
            .FirstOrDefault(u => u.Id == id);
    }

    /// <summary>
    /// Dohvata korisnika po username-u
    /// </summary>
    public User? GetByUsername(string username)
    {
        return _context.Users
            .Include(u => u.Activities)
            .Include(u => u.Journals)
            .FirstOrDefault(u => u.Username == username);
    }

    /// <summary>
    /// Sprema novog korisnika
    /// </summary>
    public void Add(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
    }

    /// <summary>
    /// Ažurira postojećeg korisnika
    /// </summary>
    public void Update(User user)
    {
        _context.Users.Update(user);
        _context.SaveChanges();
    }

    /// <summary>
    /// Briše korisnika po ID-u
    /// </summary>
    public void Delete(int id)
    {
        var user = _context.Users.Find(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            _context.SaveChanges();
        }
    }
}
