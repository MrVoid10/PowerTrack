namespace PowerTrack.Models
{
  public class AuditLog
  {
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string Action { get; set; } = "";

    public string Status { get; set; } = "";

    public string LogType { get; set; } = "INFO"; // INFO | WARNING | ERROR

    public string Details { get; set; } = "";

    public DateTime Timestamp { get; set; } = DateTime.Now;
  }
}