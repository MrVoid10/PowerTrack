using Microsoft.AspNetCore.Mvc;
using PowerTrack.Data;
using PowerTrack.Models;
using Microsoft.AspNetCore.Http;

namespace PowerTrack.Controllers
{
  public class DashboardController : Controller
  {
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
      _context = context;
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

      return View(data);
    }
  }
}
