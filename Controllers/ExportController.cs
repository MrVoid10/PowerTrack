using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PowerTrack.Data;
using PowerTrack.Models;
using PowerTrack.Services;
using System.Text;

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

      Console.WriteLine($"[DEBUG] Role = {role}");

      return string.Equals(
          role,
          "Admin",
          StringComparison.OrdinalIgnoreCase);
    }

    private int? CurrentUserId()
    {
      var userId = HttpContext.Session.GetInt32("UserId");

      Console.WriteLine($"[DEBUG] CurrentUserId = {userId}");

      return userId;
    }

    // =====================================================
    // VIEW
    // =====================================================

    public IActionResult Index()
    {
      Console.WriteLine("[DEBUG] Export Index loaded");

      ViewBag.IsAdmin = IsAdmin();
      return View("~/Views/ImportExport/Index.cshtml");
    }

    // =====================================================
    // EXPORT CSV
    // =====================================================

    public async Task<IActionResult> ExportCsv(bool allUsers = false)
    {
      try
      {
        Console.WriteLine("[DEBUG] ExportCsv START");

        var isAdmin = IsAdmin();
        var userId = CurrentUserId();

        IQueryable<EnergyConsumption> query =
            _context.EnergyConsumptions.AsNoTracking();

        Console.WriteLine($"[DEBUG] allUsers={allUsers} isAdmin={isAdmin}");

        if (!allUsers || !isAdmin)
        {
          Console.WriteLine($"[DEBUG] Filtering by UserId={userId}");
          query = query.Where(x => x.UserId == userId);
        }

        var data = await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        Console.WriteLine($"[DEBUG] Records fetched = {data.Count}");

        var sb = new StringBuilder();

        if (allUsers && isAdmin)
          sb.AppendLine("Id,UserId,Year,Month,ConsumptionKWh,PricePerKWh,TotalCost,CreatedAt");
        else
          sb.AppendLine("Id,Year,Month,ConsumptionKWh,PricePerKWh,TotalCost,CreatedAt");

        foreach (var item in data)
        {
          Console.WriteLine($"[DEBUG] Processing record {item.Id}");

          if (allUsers && isAdmin)
          {
            sb.AppendLine(
                $"{item.Id},{item.UserId},{item.Year},{item.Month}," +
                $"{item.ConsumptionKWh},{item.PricePerKWh},{item.TotalCost}," +
                $"{item.CreatedAt:yyyy-MM-dd}");
          }
          else
          {
            sb.AppendLine(
                $"{item.Id},{item.Year},{item.Month},{item.ConsumptionKWh}," +
                $"{item.PricePerKWh},{item.TotalCost},{item.CreatedAt:yyyy-MM-dd}");
          }
        }

        await _audit.LogAsync(
            "Export CSV",
            "SUCCESS",
            "INFO",
            $"User {userId} exported CSV"
        );

        Console.WriteLine("[DEBUG] ExportCsv END");

        return File(
            Encoding.UTF8.GetBytes(sb.ToString()),
            "text/csv",
            $"EnergyConsumption_{DateTime.Now:yyyyMMdd}.csv");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"[ERROR] ExportCsv -> {ex.Message}");
        Console.WriteLine(ex.StackTrace);

        await _audit.LogAsync(
            "Export CSV",
            "FAILED",
            "ERROR",
            ex.Message
        );

        return RedirectToAction("Index");
      }
    }

    // =====================================================
    // EXPORT JSON
    // =====================================================

    public async Task<IActionResult> ExportJson(bool allUsers = false)
    {
      try
      {
        Console.WriteLine("[DEBUG] ExportJson START");

        var isAdmin = IsAdmin();
        var userId = CurrentUserId();

        IQueryable<EnergyConsumption> query =
            _context.EnergyConsumptions.AsNoTracking();

        if (!allUsers || !isAdmin)
        {
          Console.WriteLine($"[DEBUG] JSON filter UserId={userId}");
          query = query.Where(x => x.UserId == userId);
        }

        var data = await query.ToListAsync();

        object result;

        if (!allUsers || !isAdmin)
        {
          result = data.Select(x => new
          {
            x.Id,
            x.Year,
            x.Month,
            x.ConsumptionKWh,
            x.PricePerKWh,
            x.TotalCost,
            x.CreatedAt
          });
        }
        else
        {
          result = data;
        }

        await _audit.LogAsync(
            "Export JSON",
            "SUCCESS",
            "INFO",
            $"User {userId} exported JSON"
        );

        return Json(result);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"[ERROR] ExportJson -> {ex.Message}");

        return StatusCode(500, "Export JSON error");
      }
    }

    // =====================================================
    // IMPORT CSV
    // =====================================================

    [HttpPost]
    public async Task<IActionResult> ImportCsv(IFormFile file, bool allUsers = false)
    {
      try
      {
        Console.WriteLine("[DEBUG] ImportCsv START");

        if (file == null || file.Length == 0)
        {
          Console.WriteLine("[DEBUG] ImportCsv file empty");
          return RedirectToAction("Index");
        }

        var isAdmin = IsAdmin();
        var userId = CurrentUserId();

        using var reader = new StreamReader(file.OpenReadStream());

        await reader.ReadLineAsync();

        while (!reader.EndOfStream)
        {
          var line = await reader.ReadLineAsync();

          if (string.IsNullOrWhiteSpace(line))
            continue;

          Console.WriteLine($"[DEBUG] Importing line -> {line}");

          var values = line.Split(',');

          int targetUserId = userId ?? 0;

          if (allUsers && isAdmin)
            targetUserId = int.Parse(values[1]);

          var energy = new EnergyConsumption
          {
            UserId = targetUserId,
            Year = int.Parse(values[allUsers && isAdmin ? 2 : 1]),
            Month = int.Parse(values[allUsers && isAdmin ? 3 : 2]),
            ConsumptionKWh = decimal.Parse(values[allUsers && isAdmin ? 4 : 3]),
            PricePerKWh = decimal.Parse(values[allUsers && isAdmin ? 5 : 4]),
            CreatedAt = DateTime.Now
          };

          _context.EnergyConsumptions.Add(energy);
        }

        await _context.SaveChangesAsync();

        await _audit.LogAsync(
            "Import CSV",
            "SUCCESS",
            "INFO",
            $"User {userId} imported CSV"
        );

        Console.WriteLine("[DEBUG] ImportCsv END");

        return RedirectToAction("Index");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"[ERROR] ImportCsv -> {ex.Message}");
        Console.WriteLine(ex.StackTrace);

        return RedirectToAction("Index");
      }
    }
  }
}