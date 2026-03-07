using Microsoft.AspNetCore.Mvc;
using PowerTrack.Data;
using PowerTrack.Models;
using PowerTrack.Services; // Add this for AuditService
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace PowerTrack.Controllers
{
  public class DashboardController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly AuditService _audit;

    public DashboardController(
        ApplicationDbContext context,
        AuditService audit)
    {
      _context = context;
      _audit = audit;
    }

    public IActionResult Index()
    {
      // Get current logged-in user from session
      int? userId = HttpContext.Session.GetInt32("UserId");
      if (userId == null)
        return RedirectToAction("Login", "Account");

      var data = _context.EnergyConsumptions
          .Where(c => c.UserId == userId)
          .OrderBy(c => c.Year)
          .ThenBy(c => c.Month)
          .ToList();

      // Audit log: track that the user accessed their dashboard
      _audit.Log(
          "Dashboard Access",
          "SUCCESS",
          "INFO",
          $"User {userId} accessed their dashboard"
      );

      return View(data);
    }
  }
}