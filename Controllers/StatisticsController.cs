using Microsoft.AspNetCore.Mvc;
using PowerTrack.Data;

namespace PowerTrack.Controllers
{
  public class StatisticsController : Controller
  {
    private readonly ApplicationDbContext _context;

    public StatisticsController(ApplicationDbContext context)
    {
      _context = context;
    }

    public IActionResult Index(int? month, int? year)
    {
      var userId = HttpContext.Session.GetInt32("UserId");
      if (userId == null)
      {
        TempData["Error"] = "You must be logged in to view statistics.";
        return RedirectToAction("Login", "Account");
      }

      var now = DateTime.Now;

      int selectedYear = year ?? now.Year;
      int selectedMonth = month ?? now.Month;

      // 🔒 SAFEGUARD 1: Limit year
      if (selectedYear > now.Year)
        selectedYear = now.Year;

      if (selectedYear < 2000)
        selectedYear = 2000;

      // 🔒 SAFEGUARD 2: If current year, limit month
      if (selectedYear == now.Year && selectedMonth > now.Month)
        selectedMonth = now.Month;

      // 🔒 SAFEGUARD 3: Month bounds
      if (selectedMonth < 1)
        selectedMonth = 1;

      if (selectedMonth > 12)
        selectedMonth = 12;

      var endDate = new DateTime(selectedYear, selectedMonth, 1);
      var startDate = endDate.AddMonths(-12);

      var data = _context.EnergyConsumptions
          .Where(e => e.UserId == userId.Value)
          .ToList()
          .Where(e =>
          {
            var recordDate = new DateTime(e.Year, e.Month, 1);
            return recordDate >= startDate && recordDate <= endDate;
          })
          .OrderBy(e => e.Year)
          .ThenBy(e => e.Month)
          .ToList();

      var labels = data.Select(e => $"{e.Month}/{e.Year}").ToList();
      var consumption = data.Select(e => e.ConsumptionKWh).ToList();

      ViewBag.Labels = labels;
      ViewBag.Consumption = consumption;
      ViewBag.SelectedMonth = selectedMonth;
      ViewBag.SelectedYear = selectedYear;
      ViewBag.CurrentYear = now.Year;
      ViewBag.CurrentMonth = now.Month;

      return View();
    }
  }
}