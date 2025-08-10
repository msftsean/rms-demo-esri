using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using RmsDemo.Data;
using RmsDemo.Models;
using RmsDemo.Services;

namespace RmsDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecordsController(RmsDbContext db, ArcGisService arcgis, ILogger<RecordsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Record>>> Get([FromQuery] double? minLon, [FromQuery] double? minLat,
        [FromQuery] double? maxLon, [FromQuery] double? maxLat, CancellationToken ct)
    {
        var query = db.Records.AsQueryable();

        if (minLon.HasValue && minLat.HasValue && maxLon.HasValue && maxLat.HasValue)
        {
            var poly = new Polygon(new LinearRing(new[]
            {
                new Coordinate(minLon.Value, minLat.Value),
                new Coordinate(maxLon.Value, minLat.Value),
                new Coordinate(maxLon.Value, maxLat.Value),
                new Coordinate(minLon.Value, maxLat.Value),
                new Coordinate(minLon.Value, minLat.Value)
            }))
            { SRID = 4326 };

            query = query.Where(r => r.Location != null && poly.Contains(r.Location));
        }

        var results = await query.OrderByDescending(r => r.CreatedAt).Take(500).ToListAsync(ct);
        return Ok(results);
    }

    public record CreateRecordRequest(string Title, string? Description, double? Latitude, double? Longitude);

    [HttpPost]
    public async Task<ActionResult<Record>> Create([FromBody] CreateRecordRequest req, CancellationToken ct)
    {
        var rec = new Record
        {
            Title = req.Title,
            Description = req.Description
        };

        if (req.Latitude is not null && req.Longitude is not null)
        {
            rec.Location = new Point(req.Longitude.Value, req.Latitude.Value) { SRID = 4326 };
        }

        db.Records.Add(rec);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = rec.Id }, rec);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Record>> GetById(Guid id, CancellationToken ct)
    {
        var rec = await db.Records.FindAsync([id], ct);
        return rec is null ? NotFound() : Ok(rec);
    }

    [HttpGet("geocode")]
    public async Task<ActionResult<object>> Geocode([FromQuery] string address, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(address)) return BadRequest("address is required");
        var result = await arcgis.GeocodeAsync(address, ct);
        return Ok(result);
    }
}
