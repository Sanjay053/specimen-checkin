namespace SpecimenCheckIn.Api.Models;

public class Manifest
{
    public int Id { get; set; }

    public int LabId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Status { get; set; } = "Open";

    public DateTime SentAt { get; set; }

    public Lab? Lab { get; set; }

    public ICollection<Specimen> Specimens { get; set; }
        = new List<Specimen>();

    public ICollection<Discrepancy> Discrepancies { get; set; }
        = new List<Discrepancy>();
}