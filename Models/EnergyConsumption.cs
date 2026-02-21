using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace PowerTrack.Models
{
  public class EnergyConsumption
  {
    public int Id { get; set; }

    public int UserId { get; set; }

    public int Year { get; set; }
    public int Month { get; set; }

    public decimal ConsumptionKWh { get; set; }
    public decimal PricePerKWh { get; set; }

    public decimal TotalCost => ConsumptionKWh * PricePerKWh;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    [ValidateNever]
    public User User { get; set; }
  }
}
