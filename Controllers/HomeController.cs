using Microsoft.AspNetCore.Mvc;
using PowerTrack.Data;
using PowerTrack.Services;
using Microsoft.EntityFrameworkCore;

namespace PowerTrack.Controllers
{
  public class HomeController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly AuditService _audit;

    public HomeController(
        ApplicationDbContext context,
        AuditService audit)
    {
      _context = context;
      _audit = audit;
    }

    public IActionResult Index()
    {
      var userId = HttpContext.Session.GetInt32("UserId");

      if (userId == null)
        return RedirectToAction("Login", "Account");

      try
      {
        var now = DateTime.Now;

        int year = now.Year;
        int month = now.Month;

        // ⭐ Try current month data
        var data = _context.EnergyConsumptions
            .Where(x => x.UserId == userId.Value &&
                        x.Year == year &&
                        x.Month == month)
            .AsNoTracking()
            .ToList();

        // ⭐ If no data → fallback to previous month
        if (!data.Any())
        {
          var prev = now.AddMonths(-1);

          year = prev.Year;
          month = prev.Month;

          data = _context.EnergyConsumptions
              .Where(x => x.UserId == userId.Value &&
                          x.Year == year &&
                          x.Month == month)
              .AsNoTracking()
              .ToList();
        }

        // ⭐ LOCAL CALCULATION VARIABLES
        decimal totalConsumption = data.Sum(x => x.ConsumptionKWh);

        decimal totalCost = data.Any()
            ? data.Sum(x => x.TotalCost)
            : 0m;

        // ⭐ Format cu 2 zecimale
        ViewBag.TotalCost = Math.Round(totalCost, 2);

        ViewBag.Year = year;
        ViewBag.Month = month;
        ViewBag.TotalConsumption = totalConsumption;
        ViewBag.TotalCost = totalCost;

        _audit.Log(
            "Home Dashboard View",
            "SUCCESS",
            "INFO",
            $"User {userId} viewed dashboard"
        );

        return View();
      }
      catch (Exception ex)
      {
        _audit.Log(
            "Home Dashboard View",
            "FAILED",
            "ERROR",
            ex.Message
        );

        return View();
      }
    }
  }
}