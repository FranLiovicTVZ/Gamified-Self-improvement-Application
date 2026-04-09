using Gamified_Self_Improvement.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Gamified_Self_Improvement.Controllers;

public class UserController : Controller
{
    private readonly UserMockRepository _userRepository;

    public UserController(UserMockRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Lista svih korisnika
    /// </summary>
    public IActionResult Index()
    {
        var users = _userRepository.GetAll();
        return View(users);
    }

    /// <summary>
    /// Detalji o specifičnom korisniku
    /// </summary>
    public IActionResult Details(int id)
    {
        var user = _userRepository.GetById(id);
        if (user == null)
            return NotFound();

        return View(user);
    }
}
