namespace PowerTrack.Models
{
  public class User
  {
    public int Id { get; set; }                // PK
    public string Name { get; set; } = "";     // Name or nickname
    public string Email { get; set; } = "";    // Unique
    public string PasswordHash { get; set; } = ""; // Store hashed password
    public string Role { get; set; } = "User"; // Default role
  }
}
