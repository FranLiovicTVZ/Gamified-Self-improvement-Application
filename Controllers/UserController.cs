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
}
