namespace SpecimenCheckIn.Api.Models;

public class Lab
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<Manifest> Manifests { get; set; }
        = new List<Manifest>();
}