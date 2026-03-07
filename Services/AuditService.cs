using Microsoft.EntityFrameworkCore;
using PowerTrack.Data;
using PowerTrack.Models;

namespace PowerTrack.Services
{
  public class AuditService
  {
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContext;

    public AuditService(
        ApplicationDbContext context,
        IHttpContextAccessor httpContext)
    {
      _context = context;
      _httpContext = httpContext;
    }

    // =====================================================
    // ⭐ SAFE USER SESSION GETTER
    // =====================================================

    private int GetSafeUserId()
    {
      try
      {
        return _httpContext.HttpContext?
            .Session.GetInt32("UserId") ?? 0;
      }
      catch
      {
        return 0; // System / anonymous user fallback
      }
    }

    // =====================================================
    // 🚀 ASYNC LOGGER (PRO VERSION)
    // =====================================================

    public async Task LogAsync(
        string action,
        string status = "SUCCESS",
        string logType = "INFO",
        string details = "")
    {
      try
      {
        int userId = GetSafeUserId();

        var log = new AuditLog
        {
          UserId = userId,
          Action = action ?? "UNKNOWN_ACTION",
          Status = status?.ToUpper() ?? "SUCCESS",
          LogType = logType?.ToUpper() ?? "INFO",
          Details = details ?? "",
          Timestamp = DateTime.UtcNow
        };

        await _context.AuditLogs.AddAsync(log);
        await _context.SaveChangesAsync();
      }
      catch (Exception ex)
      {
        // 🚀 DEBUG PRINT (Very useful during development)
        Console.WriteLine("========== AUDIT SERVICE ERROR ==========");
        Console.WriteLine($"Message: {ex.Message}");
        Console.WriteLine($"StackTrace: {ex.StackTrace}");
        Console.WriteLine("=========================================");

        try
        {
          // Optional: fallback debug log
          var inner = ex.InnerException;
          if (inner != null)
          {
            Console.WriteLine($"Inner Exception: {inner.Message}");
          }
        }
        catch { }

        // Never throw exception upward
      }
    }

    // =====================================================
    // ⭐ SYNC VERSION (Backward compatibility)
    // =====================================================

    public void Log(
        string action,
        string status = "SUCCESS",
        string logType = "INFO",
        string details = "")
    {
      try
      {
        var task = LogAsync(action, status, logType, details);
        task.Wait();
      }
      catch (Exception ex)
      {
        Console.WriteLine("[AUDIT SYNC ERROR]");
        Console.WriteLine(ex.Message);
      }
    }
  }
}