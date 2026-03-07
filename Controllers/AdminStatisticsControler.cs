using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PowerTrack.Data;
using System.Globalization;

namespace PowerTrack.Controllers
{
  public class AdminStatisticsController : Controller
  {
    private readonly ApplicationDbContext _context;

    public AdminStatisticsController(ApplicationDbContext context)
    {
      _context = context;
    }

    public IActionResult Index()
    {
      var role = HttpContext.Session.GetString("Role");

      if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        return RedirectToAction("Index", "Home");

      var now = DateTime.Now;

      var allEnergy = _context.EnergyConsumptions.ToList();

      // ===== USERS =====
      ViewBag.TotalUsers = _context.Users.Count();

      // ===== ENERGY RECORDS =====
      ViewBag.TotalRecords = allEnergy.Count;

      // ===== CONSUMPTION TOTAL =====
      ViewBag.TotalConsumption = allEnergy.Sum(x => x.ConsumptionKWh);

      // ===== MONEY TOTAL =====
      ViewBag.TotalMoneySpent = allEnergy.Sum(x => x.TotalCost);

      // ===== MONTHLY CHART =====
      var monthly = allEnergy
          .Where(x => x.Year == now.Year)
          .GroupBy(x => x.Month)
          .Select(g => new
          {
            Month = CultureInfo.CurrentCulture
                  .DateTimeFormat.GetMonthName(g.Key),

            Value = g.Sum(x => x.ConsumptionKWh)
          })
          .OrderBy(x => x.Month)
          .ToList();

      ViewBag.MonthLabels = monthly.Select(x => x.Month).ToList();
      ViewBag.MonthValues = monthly.Select(x => x.Value).ToList();

      return View();
    }
  }
}