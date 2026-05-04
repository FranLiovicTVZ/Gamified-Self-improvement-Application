using Gamified_Self_Improvement.Repositories;
using GamefiedSelfImprovement;
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
}
