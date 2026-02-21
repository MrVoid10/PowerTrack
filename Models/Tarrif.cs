namespace PowerTrack.Models
{
  public class Tariff
  {
    public int Id { get; set; }

    public decimal PricePerKWh { get; set; }

    public DateTime EffectiveFrom { get; set; }
  }
}
