using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PowerTrack.Data;
using PowerTrack.Models;
using PowerTrack.Services;
using ClosedXML.Excel;

namespace PowerTrack.Controllers
{
  public class ExportController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly AuditService _audit;

    public ExportController(
        ApplicationDbContext context,
        AuditService audit)
    {
      _context = context;
      _audit = audit;
    }

    // =====================================================
    // Helpers
    // =====================================================

    private bool IsAdmin()
    {
      var role = HttpContext.Session.GetString("Role");
      return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
    }

    private int? CurrentUserId()
    {
      return HttpContext.Session.GetInt32("UserId");
    }

    // =====================================================
    // VIEW
    // =====================================================

    public IActionResult Index()
    {
      ViewBag.IsAdmin = IsAdmin();
      return View("~/Views/Export/Index.cshtml");
    }

    // =====================================================
    // EXPORT EXCEL
    // =====================================================

    public async Task<IActionResult> ExportExcel(bool allUsers = false)
    {
      try
      {
        var isAdmin = IsAdmin();
        var userId = CurrentUserId();

        IQueryable<EnergyConsumption> query =
            _context.EnergyConsumptions
            .Include(e => e.User)
            .AsNoTracking();

        if (!allUsers || !isAdmin)
          query = query.Where(x => x.UserId == userId);

        var data = await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("EnergyConsumption");

        int col = 1;

        ws.Cell(1, col++).Value = "Id";

        if (allUsers && isAdmin)
        {
          ws.Cell(1, col++).Value = "UserId";
          ws.Cell(1, col++).Value = "UserName";
        }

        ws.Cell(1, col++).Value = "Year";
        ws.Cell(1, col++).Value = "Month";
        ws.Cell(1, col++).Value = "ConsumptionKWh";
        ws.Cell(1, col++).Value = "PricePerKWh";
        ws.Cell(1, col++).Value = "TotalCost";
        ws.Cell(1, col++).Value = "CreatedAt";

        int row = 2;

        foreach (var item in data)
        {
          col = 1;

          ws.Cell(row, col++).Value = item.Id;

          if (allUsers && isAdmin)
          {
            ws.Cell(row, col++).Value = item.UserId;
            ws.Cell(row, col++).Value = item.User?.Name ?? "";
          }

          ws.Cell(row, col++).Value = item.Year;
          ws.Cell(row, col++).Value = item.Month;
          ws.Cell(row, col++).Value = item.ConsumptionKWh;
          ws.Cell(row, col++).Value = item.PricePerKWh;
          ws.Cell(row, col++).Value = item.TotalCost;
          ws.Cell(row, col++).Value = item.CreatedAt;

          row++;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);

        await _audit.LogAsync(
            "Export Excel",
            "SUCCESS",
            "INFO",
            $"User {userId} exported Excel"
        );

        return File(
            ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"EnergyConsumption_{DateTime.Now:yyyyMMdd}.xlsx");
      }
      catch (Exception ex)
      {
        await _audit.LogAsync(
            "Export Excel",
            "FAILED",
            "ERROR",
            ex.Message
        );

        return RedirectToAction("Index");
      }
    }

    // =====================================================
    // IMPORT EXCEL
    // =====================================================

    [HttpPost]
    public async Task<IActionResult> ImportExcel(IFormFile file, bool allUsers = false)
    {
      try
      {
        if (file == null || file.Length == 0)
          return RedirectToAction("Index");

        var isAdmin = IsAdmin();
        var userId = CurrentUserId();

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);

        using var workbook = new XLWorkbook(stream);
        var ws = workbook.Worksheet(1);

        var rows = ws.RangeUsed().RowsUsed().Skip(1);

        foreach (var row in rows)
        {
          int col = 1;

          int id = row.Cell(col++).GetValue<int>();

          int targetUserId = userId ?? 0;

          if (allUsers && isAdmin)
          {
            targetUserId = row.Cell(col++).GetValue<int>();
            col++; // skip username
          }

          int year = row.Cell(col++).GetValue<int>();
          int month = row.Cell(col++).GetValue<int>();
          decimal consumption = row.Cell(col++).GetValue<decimal>();
          decimal price = row.Cell(col++).GetValue<decimal>();

          var energy = new EnergyConsumption
          {
            UserId = targetUserId,
            Year = year,
            Month = month,
            ConsumptionKWh = consumption,
            PricePerKWh = price,
            CreatedAt = DateTime.Now
          };

          _context.EnergyConsumptions.Add(energy);
        }

        await _context.SaveChangesAsync();

        await _audit.LogAsync(
            "Import Excel",
            "SUCCESS",
            "INFO",
            $"User {userId} imported Excel"
        );

        return RedirectToAction("Index");
      }
      catch (Exception ex)
      {
        await _audit.LogAsync(
            "Import Excel",
            "FAILED",
            "ERROR",
            ex.Message
        );

        return RedirectToAction("Index");
      }
    }
  }
}