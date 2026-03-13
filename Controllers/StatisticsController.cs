using Microsoft.AspNetCore.Mvc;
using PowerTrack.Data;
using PowerTrack.Services;
using System.Globalization;

namespace PowerTrack.Controllers
{
  public class StatisticsController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly AuditService _audit;

    public StatisticsController(
        ApplicationDbContext context,
        AuditService audit)
    {
      _context = context;
      _audit = audit;
    }

    public IActionResult Index(int? month, int? year)
    {
      var userId = HttpContext.Session.GetInt32("UserId");

      if (userId == null)
      {
        _audit.Log(
            "Statistics Access",
            "FAILED",
            "WARNING",
            "Unauthorized access to statistics page"
        );

        TempData["Error"] =
            "You must be logged in to view statistics.";

        return RedirectToAction("Login", "Account");
      }

      try
      {
        var now = DateTime.Now;

        int selectedYear = year ?? now.Year;
        int selectedMonth = month ?? now.Month;

        selectedYear = Math.Clamp(selectedYear, 2000, now.Year);
        selectedMonth = Math.Clamp(selectedMonth, 1, 12);

        if (selectedYear == now.Year && selectedMonth > now.Month)
          selectedMonth = now.Month;

        var endDate = new DateTime(selectedYear, selectedMonth, 1);
        var startDate = endDate.AddMonths(-12);

        int startYear = startDate.Year;
        int startMonth = startDate.Month;

        int endYear = endDate.Year;
        int endMonth = endDate.Month;

        var data = _context.EnergyConsumptions
            .Where(e => e.UserId == userId.Value)
            .Where(e =>
                (e.Year > startYear || (e.Year == startYear && e.Month >= startMonth)) &&
                (e.Year < endYear || (e.Year == endYear && e.Month <= endMonth))
            )
            .OrderBy(e => e.Year)
            .ThenBy(e => e.Month)
            .ToList();

        var labels = data.Select(e =>
            $"{CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(e.Month)} {e.Year}"
        ).ToList();

        var consumption = data
            .Select(e => e.ConsumptionKWh)
            .ToList();

        _audit.Log(
            "Statistics View",
            "SUCCESS",
            "INFO",
            $"User {userId} viewed statistics for {selectedMonth}/{selectedYear}"
        );

        ViewBag.Labels = labels;
        ViewBag.Consumption = consumption;
        ViewBag.SelectedMonth = selectedMonth;
        ViewBag.SelectedYear = selectedYear;
        ViewBag.CurrentYear = now.Year;
        ViewBag.CurrentMonth = now.Month;

        return View();
      }
      catch (Exception ex)
      {
        _audit.Log(
            "Statistics View",
            "FAILED",
            "ERROR",
            ex.Message
        );

        TempData["Error"] = "Error loading statistics data";
        return RedirectToAction("Index", "Home");
      }
    }
  }
}