using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpecimenCheckIn.Api.Data;
using SpecimenCheckIn.Api.Models;

namespace SpecimenCheckIn.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ManifestsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ManifestsController(AppDbContext context)
    {
        _context = context;
    }

    // Get current tenant from request header
    private bool TryGetLabId(out int labId)
    {
        labId = 0;

        return Request.Headers.TryGetValue("X-Lab-Id", out var value)
            && int.TryParse(value, out labId)
            && labId > 0;
    }

    // GET: api/manifests
    [HttpGet]
    public async Task<IActionResult> GetManifests()
    {
        if (!TryGetLabId(out int labId))
            return BadRequest(new { error = "Valid X-Lab-Id header is required." });

        var manifests = await _context.Manifests
            .Where(m => m.LabId == labId)
            .Select(m => new
            {
                m.Id,
                m.Code,
                m.Status,
                m.SentAt,

                Expected = m.Specimens.Count(),

                Received = m.Specimens.Count(
                    s => s.Status == "Received"),

                Pending = m.Specimens.Count(
                    s => s.Status == "Pending"),

                Discrepancies = m.Discrepancies.Count(
                    d => d.Status == "Open")
            })
            .ToListAsync();

        return Ok(manifests);
    }

    // GET: api/manifests/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetManifest(int id)
    {
        if (!TryGetLabId(out int labId))
            return BadRequest(new { error = "Valid X-Lab-Id header is required." });

        var manifest = await _context.Manifests
            .Where(m => m.Id == id && m.LabId == labId)
            .Select(m => new
            {
                m.Id,
                m.Code,
                m.Status,
                m.SentAt,

                Expected = m.Specimens.Count(),

                Received = m.Specimens.Count(
                    s => s.Status == "Received"),

                Pending = m.Specimens.Count(
                    s => s.Status == "Pending"),

                Discrepancies = m.Discrepancies.Count(
                    d => d.Status == "Open"),

                Specimens = m.Specimens.Select(s => new
                {
                    s.Id,
                    s.Code,
                    s.Patient,
                    s.Status
                }),

                DiscrepancyDetails = m.Discrepancies.Select(d => new
                {
                    d.Id,
                    d.SpecimenId,
                    d.Type,
                    d.Status
                })
            })
            .FirstOrDefaultAsync();

        if (manifest == null)
            return NotFound(new { error = "Manifest not found." });

        return Ok(manifest);
    }

    // POST: api/manifests/{id}/specimens/{sid}/receive
    [HttpPost("{id}/specimens/{sid}/receive")]
    public async Task<IActionResult> ReceiveSpecimen(
        int id,
        int sid)
    {
        if (!TryGetLabId(out int labId))
            return BadRequest(new { error = "Valid X-Lab-Id header is required." });

        var manifest = await _context.Manifests
            .FirstOrDefaultAsync(m =>
                m.Id == id && m.LabId == labId);

        if (manifest == null)
            return NotFound(new { error = "Manifest not found." });

        if (manifest.Status != "Open")
            return BadRequest(new { error = "Manifest is already closed." });

        var specimen = await _context.Specimens
            .FirstOrDefaultAsync(s =>
                s.Id == sid && s.ManifestId == id);

        if (specimen == null)
            return NotFound(new { error = "Specimen not found." });

        // Idempotency
        if (specimen.Status == "Received")
        {
            return Ok(new
            {
                message = "Specimen received successfully.",
                specimenId = specimen.Id,
                status = specimen.Status
            });
        }

        if (specimen.Status == "Flagged")
        {
            return BadRequest(new
            {
                error = "Flagged specimen cannot be received."
            });
        }

        specimen.Status = "Received";

        await _context.SaveChangesAsync();

        // Return a DTO-style response, not the EF Core entity.
        return Ok(new
        {
            message = "Specimen received successfully.",
            specimenId = specimen.Id,
            status = specimen.Status
        });

    }

    // POST: api/manifests/{id}/specimens/{sid}/flag
    [HttpPost("{id}/specimens/{sid}/flag")]
    public async Task<IActionResult> FlagSpecimen(
        int id,
        int sid)
    {
        if (!TryGetLabId(out int labId))
            return BadRequest(new { error = "Valid X-Lab-Id header is required." });

        var manifest = await _context.Manifests
            .FirstOrDefaultAsync(m =>
                m.Id == id && m.LabId == labId);

        if (manifest == null)
            return NotFound(new { error = "Manifest not found." });

        if (manifest.Status != "Open")
            return BadRequest(new { error = "Manifest is already closed." });

        var specimen = await _context.Specimens
            .FirstOrDefaultAsync(s =>
                s.Id == sid && s.ManifestId == id);

        if (specimen == null)
            return NotFound(new { error = "Specimen not found." });

        if (specimen.Status == "Received")
            return BadRequest(new { error = "Received specimen cannot be flagged." });

        if (specimen.Status == "Flagged")
            return Ok(new { message = "Specimen already flagged." });

        specimen.Status = "Flagged";

        var discrepancy = new Discrepancy
        {
            ManifestId = id,
            SpecimenId = sid,
            Type = "Missing",
            Status = "Open"
        };

        _context.Discrepancies.Add(discrepancy);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Specimen flagged and discrepancy created.",
            specimenId = specimen.Id,
            specimenStatus = specimen.Status,
            discrepancyId = discrepancy.Id,
            discrepancyStatus = discrepancy.Status
        });
    }

    // POST: api/manifests/{id}/discrepancies/{did}/resolve
    [HttpPost("{id}/discrepancies/{did}/resolve")]
    public async Task<IActionResult> ResolveDiscrepancy(
        int id,
        int did)
    {
        if (!TryGetLabId(out int labId))
            return BadRequest(new { error = "Valid X-Lab-Id header is required." });

        var manifest = await _context.Manifests
            .FirstOrDefaultAsync(m =>
                m.Id == id && m.LabId == labId);

        if (manifest == null)
            return NotFound(new { error = "Manifest not found." });

        if (manifest.Status != "Open")
            return BadRequest(new { error = "Manifest is already closed." });

        var discrepancy = await _context.Discrepancies
            .FirstOrDefaultAsync(d =>
                d.Id == did &&
                d.ManifestId == id &&
                d.Status == "Open");

        if (discrepancy == null)
            return NotFound(new { error = "Open discrepancy not found." });

        discrepancy.Status = "Resolved";

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Discrepancy resolved successfully."
        });
    }

    // POST: api/manifests/{id}/close
    [HttpPost("{id}/close")]
    public async Task<IActionResult> CloseManifest(int id)
    {
        if (!TryGetLabId(out int labId))
            return BadRequest(new { error = "Valid X-Lab-Id header is required." });

        var manifest = await _context.Manifests
            .Include(m => m.Specimens)
            .Include(m => m.Discrepancies)
            .FirstOrDefaultAsync(m =>
                m.Id == id && m.LabId == labId);

        if (manifest == null)
            return NotFound(new { error = "Manifest not found." });

        if (manifest.Status != "Open")
            return BadRequest(new { error = "Manifest is already closed." });

        bool hasPending = manifest.Specimens
            .Any(s => s.Status == "Pending");

        bool hasOpenDiscrepancies = manifest.Discrepancies
            .Any(d => d.Status == "Open");

        if (hasPending || hasOpenDiscrepancies)
        {
            return BadRequest(new
            {
                error = "Manifest cannot be closed.",
                pendingSpecimens = manifest.Specimens
                    .Count(s => s.Status == "Pending"),
                openDiscrepancies = manifest.Discrepancies
                    .Count(d => d.Status == "Open")
            });
        }

        manifest.Status = "Closed";

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Manifest closed successfully."
        });
    }
}