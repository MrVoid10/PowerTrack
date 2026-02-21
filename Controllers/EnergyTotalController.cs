using Microsoft.AspNetCore.Mvc;
using PowerTrack.Data;
using PowerTrack.Models;
using Microsoft.EntityFrameworkCore;

namespace PowerTrack.Controllers
{
  public class EnergyTotalController : Controller
  {
    private readonly ApplicationDbContext _context;

    public EnergyTotalController(ApplicationDbContext context)
    {
      _context = context;
    }

    // GET: /EnergyTotal
    public IActionResult Index()
    {
      // Get current logged-in user
      var userId = HttpContext.Session.GetInt32("UserId");
      if (userId == null)
      {
        // Not logged in, show empty or redirect to login
        TempData["Error"] = "You must be logged in to view your energy records.";
        return RedirectToAction("Login", "Account");
      }

      // Filter by current user only
      var data = _context.EnergyConsumptions
          .Where(e => e.UserId == userId.Value)
          .OrderByDescending(e => e.Year)
          .ThenByDescending(e => e.Month)
          .ToList();

      return View(data);
    }
  }
}
