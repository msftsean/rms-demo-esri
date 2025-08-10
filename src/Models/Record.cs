using NetTopologySuite.Geometries;

namespace RmsDemo.Models;

public class Record
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Point? Location { get; set; } // SRID 4326 (lon/lat)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
