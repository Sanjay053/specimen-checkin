namespace SpecimenCheckIn.Api.Models;

public class Specimen
{
    public int Id { get; set; }

    public int ManifestId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Patient { get; set; } = string.Empty;

    public string Status { get; set; } = "Pending";

    public Manifest? Manifest { get; set; }
}