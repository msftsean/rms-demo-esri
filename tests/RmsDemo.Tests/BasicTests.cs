using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using FluentAssertions;

namespace RmsDemo.Tests;

public class BasicTests
{
    [Fact]
    public async Task Health_Returns_Ok()
    {
        await using var app = new CustomWebApplicationFactory();
        var client = app.CreateClient();
        var resp = await client.GetAsync("/health");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_And_Get_Record_Works()
    {
        await using var app = new CustomWebApplicationFactory();
        var client = app.CreateClient();

        var create = await client.PostAsJsonAsync("/api/records", new { title = "Test", description = "D", latitude = 47.6, longitude = -122.3 });
        create.EnsureSuccessStatusCode();

        var created = await create.Content.ReadFromJsonAsync<JsonElement>();
        created.ValueKind.Should().Be(JsonValueKind.Object);
        var id = created.GetProperty("id").GetGuid();

        var get = await client.GetAsync($"/api/records/{id}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Geocode_Returns_Predictable_When_No_Key()
    {
        await using var app = new CustomWebApplicationFactory();
        var client = app.CreateClient();
        var resp = await client.GetFromJsonAsync<JsonElement>("/api/records/geocode?address=1600%20Pennsylvania%20Ave");
        resp.GetProperty("status").GetString().Should().Contain("not configured");
    }
}
