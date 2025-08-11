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
    public record RecordDto(Guid Id, string Title, string? Description, double? Latitude, double? Longitude, DateTime CreatedAt);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RecordDto>>> Get([FromQuery] double? minLon, [FromQuery] double? minLat,
        [FromQuery] double? maxLon, [FromQuery] double? maxLat, CancellationToken ct)
    {
        logger.LogInformation("Getting records with bounds: minLon={MinLon}, minLat={MinLat}, maxLon={MaxLon}, maxLat={MaxLat}", 
            minLon, minLat, maxLon, maxLat);

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
            logger.LogDebug("Applied spatial filter for bounding box");
        }

        var results = await query
            .OrderByDescending(r => r.CreatedAt)
            .Take(500)
            .Select(r => new RecordDto(
                r.Id,
                r.Title,
                r.Description,
                r.Location != null ? (double?)r.Location.Y : null,
                r.Location != null ? (double?)r.Location.X : null,
                r.CreatedAt))
            .ToListAsync(ct);

        logger.LogInformation("Retrieved {Count} records", results.Count);
        return Ok(results);
    }

    public record CreateRecordRequest(string Title, string? Description, double? Latitude, double? Longitude);

    [HttpPost]
    public async Task<ActionResult<RecordDto>> Create([FromBody] CreateRecordRequest req, CancellationToken ct)
    {
        logger.LogInformation("Creating new record: {Title}", req.Title);

        var rec = new Record
        {
            Title = req.Title,
            Description = req.Description
        };

        if (req.Latitude is not null && req.Longitude is not null)
        {
            rec.Location = new Point(req.Longitude.Value, req.Latitude.Value) { SRID = 4326 };
            logger.LogDebug("Record location set to: {Latitude}, {Longitude}", req.Latitude, req.Longitude);
        }

        db.Records.Add(rec);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Successfully created record with ID: {RecordId}", rec.Id);

        var dto = new RecordDto(
            rec.Id,
            rec.Title,
            rec.Description,
            rec.Location != null ? (double?)rec.Location.Y : null,
            rec.Location != null ? (double?)rec.Location.X : null,
            rec.CreatedAt);
        return CreatedAtAction(nameof(GetById), new { id = rec.Id }, dto);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RecordDto>> GetById(Guid id, CancellationToken ct)
    {
        logger.LogDebug("Getting record by ID: {RecordId}", id);

        var rec = await db.Records.FindAsync([id], ct);
        if (rec is null) 
        {
            logger.LogWarning("Record not found: {RecordId}", id);
            return NotFound();
        }

        var dto = new RecordDto(
            rec.Id,
            rec.Title,
            rec.Description,
            rec.Location != null ? (double?)rec.Location.Y : null,
            rec.Location != null ? (double?)rec.Location.X : null,
            rec.CreatedAt);
        return Ok(dto);
    }

    [HttpGet("geocode")]
    public async Task<ActionResult<object>> Geocode([FromQuery] string address, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(address)) 
        {
            logger.LogWarning("Geocode request with empty address");
            return BadRequest("address is required");
        }

        logger.LogInformation("Geocoding address: {Address}", address);
        
        try
        {
            var result = await arcgis.GeocodeAsync(address, ct);
            logger.LogInformation("Successfully geocoded address: {Address}", address);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error geocoding address: {Address}", address);
            throw;
        }
    }
}
