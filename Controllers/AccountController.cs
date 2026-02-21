using Microsoft.AspNetCore.Mvc;
using PowerTrack.Data;
using PowerTrack.Models;
using System.Security.Cryptography;
using System.Text;

namespace PowerTrack.Controllers
{
  public class AccountController : Controller
  {
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
      _context = context;
    }

    // GET: /Account/Login
    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public IActionResult Login(User model)
    {
      if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.PasswordHash))
      {
        ViewBag.Error = "Email and password required";
        return View();
      }

      // hash the password entered to compare
      var hashedInput = HashPassword(model.PasswordHash);

      var user = _context.Users
          .FirstOrDefault(u => u.Email == model.Email && u.PasswordHash == hashedInput);

      if (user != null)
      {
        // store session variables
        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserName", user.Name);   // <-- Name for navbar
        HttpContext.Session.SetString("UserEmail", user.Email); // <-- Email for tooltip

        return RedirectToAction("Index", "Home");
      }

      ViewBag.Error = "Invalid credentials";
      return View();
    }

    // GET: /Account/Signup
    [HttpGet]
    public IActionResult Signup() => View();

    [HttpPost]
    public IActionResult Signup(User model)
    {
      if (ModelState.IsValid)
      {
        // hash the password before storing
        model.PasswordHash = HashPassword(model.PasswordHash);

        _context.Users.Add(model);
        _context.SaveChanges();
        return RedirectToAction("Login");
      }

      return View();
    }

    // GET: /Account/Logout
    public IActionResult Logout()
    {
      // clear all session data
      HttpContext.Session.Remove("UserId");
      HttpContext.Session.Remove("UserName");
      HttpContext.Session.Remove("UserEmail");

      return RedirectToAction("Login");
    }

    // Simple SHA256 hash function
    private string HashPassword(string password)
    {
      using var sha256 = SHA256.Create();
      var bytes = Encoding.UTF8.GetBytes(password);
      var hash = sha256.ComputeHash(bytes);
      return Convert.ToBase64String(hash);
    }
  }
}
