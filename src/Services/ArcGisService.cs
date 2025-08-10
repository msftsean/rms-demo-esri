namespace RmsDemo.Services;

public class ArcGisOptions
{
    public string? ApiKey { get; set; }
    public string? ClientId { get; set; }
}

public class ArcGisService(HttpClient http, IConfiguration config, ILogger<ArcGisService> logger)
{
    public async Task<object> GeocodeAsync(string address, CancellationToken ct)
    {
        var apiKey = config["ArcGIS:ApiKey"] ?? config["ArcGIS__ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            // Return predictable payload for demo if not configured
            return new { address, status = "ArcGIS API key not configured" };
        }

        // Simple call to ArcGIS World Geocoding service (forward geocode)
        var url = $"https://geocode-api.arcgis.com/arcgis/rest/services/World/Geocoding/geocodeAddresses?f=json&token={Uri.EscapeDataString(apiKey)}";
        var content = new StringContent($$"""
        {
          "records": [
            {
              "attributes": { "OBJECTID": 1, "SingleLine": "{address.Replace("\"", "")}" }
            }
          ]
        }
        """, System.Text.Encoding.UTF8, "application/json");

        try
        {
            using var resp = await http.PostAsync(url, content, ct);
            var json = await resp.Content.ReadAsStringAsync(ct);
            return new { ok = resp.IsSuccessStatusCode, json };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "ArcGIS geocode failed");
            return new { error = ex.Message };
        }
    }
}
