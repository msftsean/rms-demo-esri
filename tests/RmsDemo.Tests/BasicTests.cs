using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;

namespace RmsDemo.Tests;

public class BasicTests
{
    [Fact]
    public async Task Health_Returns_Ok()
    {
        await using var app = new WebApplicationFactory<Program>();
        var client = app.CreateClient();
        var resp = await client.GetAsync("/health");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
