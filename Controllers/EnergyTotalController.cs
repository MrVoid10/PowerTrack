using Microsoft.AspNetCore.Mvc;
using PowerTrack.Data;
using PowerTrack.Models;
using PowerTrack.Services;
using Microsoft.EntityFrameworkCore;

namespace PowerTrack.Controllers
{
  public class EnergyTotalController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly AuditService _audit;

    public EnergyTotalController(
        ApplicationDbContext context,
        AuditService audit)
    {
      _context = context;
      _audit = audit;
    }

    // GET: /EnergyTotal
    public IActionResult Index()
    {
      var userId = HttpContext.Session.GetInt32("UserId");

      if (userId == null)
      {
        _audit.Log(
            "Energy Total Access",
            "FAILED",
            "WARNING",
            "Unauthorized access to EnergyTotal page"
        );

        TempData["Error"] =
            "You must be logged in to view your energy records.";

        return RedirectToAction("Login", "Account");
      }

      try
      {
        var data = _context.EnergyConsumptions
            .Where(e => e.UserId == userId.Value)
            .OrderByDescending(e => e.Year)
            .ThenByDescending(e => e.Month)
            .ToList();

        _audit.Log(
            "Energy Total View",
            "SUCCESS",
            "INFO",
            $"User {userId} viewed energy totals"
        );

        return View(data);
      }
      catch (Exception ex)
      {
        _audit.Log(
            "Energy Total View",
            "FAILED",
            "ERROR",
            ex.Message
        );

        TempData["Error"] = "Error loading energy data";
        return View(new List<EnergyConsumption>());
      }
    }
  }
}