using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PowerTrack.Data;
using PowerTrack.Models;

namespace PowerTrack.Controllers
{
  public class AdminAuditController : Controller
  {
    private readonly ApplicationDbContext _context;

    public AdminAuditController(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<IActionResult> Index(
        int page = 1,
        string? logType = null,
        string? actionFilter = null,
        string? search = null,
        int? userId = null,
        string? timeRange = "month",
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
      const int pageSize = 50;

      var query = _context.AuditLogs.AsQueryable();

      // Log type filter
      if (!string.IsNullOrEmpty(logType))
        query = query.Where(x => x.LogType == logType);

      // Action filter
      if (!string.IsNullOrEmpty(actionFilter))
        query = query.Where(x => x.Action.Contains(actionFilter));

      // User filter
      if (userId.HasValue)
        query = query.Where(x => x.UserId == userId);

      // Search filter
      if (!string.IsNullOrEmpty(search))
      {
        query = query.Where(x =>
            x.Action.Contains(search) ||
            x.Details.Contains(search) ||
            x.Status.Contains(search));
      }

      // Time range filter
      var now = DateTime.Now;

      if (timeRange == "month")
      {
        var start = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
        var end = new DateTime(now.Year, now.Month, 1);

        query = query.Where(x =>
            x.Timestamp >= start &&
            x.Timestamp < end);
      }
      else if (timeRange == "year")
      {
        var start = new DateTime(now.Year - 1, 1, 1);
        var end = new DateTime(now.Year, 1, 1);

        query = query.Where(x =>
            x.Timestamp >= start &&
            x.Timestamp < end);
      }
      else if (timeRange == "custom")
      {
        if (fromDate.HasValue)
          query = query.Where(x => x.Timestamp >= fromDate.Value);

        if (toDate.HasValue)
          query = query.Where(x => x.Timestamp <= toDate.Value);
      }

      // Count total rows
      var total = await query.CountAsync();

      // Pagination
      var logs = await query
          .OrderByDescending(x => x.Timestamp)
          .Skip((page - 1) * pageSize)
          .Take(pageSize)
          .ToListAsync();

      // ViewBag values
      ViewBag.Page = page;
      ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);

      ViewBag.LogType = logType;
      ViewBag.ActionFilter = actionFilter;
      ViewBag.Search = search;
      ViewBag.UserId = userId;

      ViewBag.TimeRange = timeRange;
      ViewBag.FromDate = fromDate;
      ViewBag.ToDate = toDate;

      return View(logs);
    }
  }
}