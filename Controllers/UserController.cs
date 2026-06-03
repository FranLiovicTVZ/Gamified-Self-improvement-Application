using Gamified_Self_Improvement.Repositories;
using GamefiedSelfImprovement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamified_Self_Improvement.Controllers;

/// <summary>
/// Controller za upravljanje korisnicima - custom routing primjena
/// </summary>
[Route("korisnici")]
public class UserController : Controller
{
    private readonly UserRepository _userRepository;

    public UserController(UserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Lista svih korisnika
    /// URL: /korisnici
    /// </summary>
    [Route("")]
    public IActionResult Index()
    {
        var users = _userRepository.GetAll();
        return View(users);
    }

    /// <summary>
    /// Detalji o specifičnom korisniku
    /// URL: /korisnici/{id} ili /profil/{id}
    /// </summary>
    [Route("{id:int}")]
    [Route("/profil/{id:int}")]
    public IActionResult Details(int id)
    {
        var user = _userRepository.GetById(id);
        if (user == null)
            return NotFound();

        return View(user);
    }

    /// <summary>
    /// Forma za kreiranje novog korisnika - GET
    /// URL: /korisnici/dodaj
    /// </summary>
    [Route("dodaj")]
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Spremi novog korisnika - POST
    /// URL: /korisnici/dodaj
    /// </summary>
    [Route("dodaj")]
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult Create(User user)
    {
        if (!ModelState.IsValid)
            return View(user);

        try
        {
            _userRepository.Add(user);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Greška pri dodavanju korisnika: {ex.Message}");
            return View(user);
        }
    }

    /// <summary>
    /// Forma za uređivanje korisnika - GET
    /// URL: /korisnici/uredi/{id}
    /// </summary>
    [Route("uredi/{id:int}")]
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Edit(int id)
    {
        var user = _userRepository.GetById(id);
        if (user == null)
            return NotFound();

        return View(user);
    }

    /// <summary>
    /// Spremi uređenog korisnika - POST
    /// URL: /korisnici/uredi/{id}
    /// </summary>
    [Route("uredi/{id:int}")]
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ActionName("Edit")]
    public IActionResult EditPost(int id, User user)
    {
        var existingUser = _userRepository.GetById(id);
        if (existingUser == null)
            return NotFound();

        if (!ModelState.IsValid)
            return View(nameof(Edit), existingUser);

        try
        {
            // Ažuriraj samo dopuštena polja
            existingUser.Username = user.Username;
            existingUser.Email = user.Email;
            existingUser.Bio = user.Bio;
            existingUser.PreferredMeditationType = user.PreferredMeditationType;
            existingUser.Level = user.Level;

            _userRepository.Update(existingUser);
            return RedirectToAction("Details", new { id = id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Greška pri ažuriranju korisnika: {ex.Message}");
            return View(nameof(Edit), existingUser);
        }
    }

    /// <summary>
    /// Forma za brisanje korisnika - GET (potvrda)
    /// URL: /korisnici/obrisi/{id}
    /// </summary>
    [Route("obrisi/{id:int}")]
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Delete(int id)
    {
        var user = _userRepository.GetById(id);
        if (user == null)
            return NotFound();

        return View(user);
    }

    /// <summary>
    /// Briše korisnika - POST
    /// URL: /korisnici/obrisi/{id}
    /// </summary>
    [Route("obrisi/{id:int}")]
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ActionName("Delete")]
    public IActionResult DeletePost(int id)
    {
        var user = _userRepository.GetById(id);
        if (user == null)
            return NotFound();

        try
        {
            _userRepository.Delete(id);
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Greška pri brisanju korisnika: {ex.Message}");
            return View(nameof(Delete), user);
        }
    }

    /// <summary>
    /// AJAX pretraga korisnika
    /// URL: /korisnici/pretraga?q=search_term
    /// </summary>
    [Route("pretraga")]
    [HttpGet]
    public IActionResult Search(string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return Json(new List<object>());

        var searchQuery = q.ToLower();
        var results = _userRepository.GetAll()
            .Where(u => u.Username.ToLower().Contains(searchQuery) || 
                        u.Email.ToLower().Contains(searchQuery))
            .Take(10)
            .Select(u => new { 
                id = u.Id, 
                username = u.Username,
                email = u.Email,
                level = u.Level,
                xp = u.TotalXP
            })
            .ToList();

        return Json(results);
    }
}
