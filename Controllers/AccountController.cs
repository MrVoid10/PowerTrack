using Microsoft.AspNetCore.Mvc;
using PowerTrack.Data;
using PowerTrack.Models;
using PowerTrack.Services;
using System.Security.Cryptography;
using System.Text;

namespace PowerTrack.Controllers
{
  public class AccountController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly AuditService _audit;

    public AccountController(
        ApplicationDbContext context,
        AuditService audit)
    {
      _context = context;
      _audit = audit;
    }

    // =====================================================
    // LOGIN
    // =====================================================

    [HttpGet]
    public IActionResult Login()
    {
      return View();
    }

    [HttpPost]
    public IActionResult Login(User model)
    {
      if (string.IsNullOrEmpty(model.Email) ||
          string.IsNullOrEmpty(model.PasswordHash))
      {
        ViewBag.Error = "Email and password required";

        _audit.Log(
            "Login Attempt",
            "FAILED",
            "WARNING",
            "Empty login credentials"
        );

        return View();
      }

      var hashedInput = HashPassword(model.PasswordHash);

      var user = _context.Users
          .FirstOrDefault(u =>
              u.Email == model.Email &&
              u.PasswordHash == hashedInput);

      if (user != null)
      {
        // Session storage
        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserName", user.Name);
        HttpContext.Session.SetString("UserEmail", user.Email);
        HttpContext.Session.SetString("Role", user.Role);

        _audit.Log(
            "Login",
            "SUCCESS",
            "INFO",
            $"User {user.Email} logged in"
        );

        return RedirectToAction("Index", "Home");
      }

      _audit.Log(
          "Login",
          "FAILED",
          "ERROR",
          $"Invalid login for {model.Email}"
      );

      ViewBag.Error = "Invalid credentials";
      return View();
    }

    // =====================================================
    // SIGNUP
    // =====================================================

    [HttpGet]
    public IActionResult Signup()
    {
      return View();
    }

    [HttpPost]
    public IActionResult Signup(User model)
    {
      if (ModelState.IsValid)
      {
        model.PasswordHash = HashPassword(model.PasswordHash);

        // Default role if not set
        if (string.IsNullOrEmpty(model.Role))
          model.Role = "User";

        _context.Users.Add(model);
        _context.SaveChanges();

        _audit.Log(
            "User Registration",
            "SUCCESS",
            "INFO",
            $"New user registered: {model.Email}"
        );

        return RedirectToAction("Login");
      }

      _audit.Log(
          "User Registration",
          "FAILED",
          "ERROR",
          "Signup validation failed"
      );

      return View();
    }

    // =====================================================
    // LOGOUT
    // =====================================================

    public IActionResult Logout()
    {
      var email = HttpContext.Session.GetString("UserEmail");

      _audit.Log(
          "Logout",
          "SUCCESS",
          "INFO",
          $"User {email} logged out"
      );

      HttpContext.Session.Clear();

      return RedirectToAction("Login");
    }

    // =====================================================
    // PASSWORD HASH
    // =====================================================

    private string HashPassword(string password)
    {
      using var sha256 = SHA256.Create();

      var bytes = Encoding.UTF8.GetBytes(password);
      var hash = sha256.ComputeHash(bytes);

      return Convert.ToBase64String(hash);
    }
  }
}