namespace SpecimenCheckIn.Api.Models;

public class Discrepancy
{
    public int Id { get; set; }

    public int ManifestId { get; set; }

    public int SpecimenId { get; set; }

    public string Type { get; set; } = "Missing";

    public string Status { get; set; } = "Open";

    public Manifest? Manifest { get; set; }

    public Specimen? Specimen { get; set; }
}