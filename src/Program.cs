using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using RmsDemo.Data;
using RmsDemo.Services;

var builder = WebApplication.CreateBuilder(args);

// App Insights
var aiConn = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
if (!string.IsNullOrWhiteSpace(aiConn))
{
    builder.Services.AddApplicationInsightsTelemetry();
}

// Controllers + Swagger
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RMS Demo ESRI API", Version = "v1" });
});

// CORS for frontend dev
builder.Services.AddCors(o =>
{
    o.AddPolicy("frontend", p =>
        p.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins(
            builder.Configuration["Frontend:BaseUrl"] ?? "http://localhost:5173"));
});

// EF Core + Postgres + PostGIS (NTS)
var cs = builder.Configuration.GetConnectionString("DefaultConnection")
         ?? builder.Configuration["ConnectionStrings__DefaultConnection"];
builder.Services.AddDbContext<RmsDbContext>(opts =>
    opts.UseNpgsql(cs, o => o.UseNetTopologySuite()));

// Redis cache
var redisConn = builder.Configuration["Redis:ConnectionString"] ?? builder.Configuration["Redis__ConnectionString"];
if (!string.IsNullOrEmpty(redisConn))
{
    builder.Services.AddStackExchangeRedisCache(o => o.Configuration = redisConn);
}

// ArcGIS service
builder.Services.AddHttpClient<ArcGisService>();
builder.Services.Configure<ArcGisOptions>(builder.Configuration.GetSection("ArcGIS"));

// Auth (optional in Dev)
var authority = builder.Configuration["OAuth:Authority"] ?? builder.Configuration["OAuth__Authority"];
var clientId = builder.Configuration["OAuth:ClientId"] ?? builder.Configuration["OAuth__ClientId"];
if (!string.IsNullOrWhiteSpace(authority))
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = authority;
            options.Audience = clientId;
            options.RequireHttpsMetadata = true;
        });
}
builder.Services.AddAuthorization();

builder.Services.AddHealthChecks();

var app = builder.Build();

// DB init (safe in dev; for prod prefer migrations)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<RmsDbContext>();
        // EnsureCreated lets us avoid writing migrations for the sample
        db.Database.EnsureCreated();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Database initialization failed");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("frontend");
app.UseRouting();

if (!string.IsNullOrWhiteSpace(authority))
{
    app.UseAuthentication();
}
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

// Redirect base path to Swagger in development to avoid 404s on /
app.MapGet("/", (IWebHostEnvironment env) =>
{
    if (env.IsDevelopment())
    {
        return Results.Redirect("/swagger", false);
    }
    return Results.Ok(new { name = "RMS Demo ESRI API", docs = "/swagger" });
});

app.Run();

public partial class Program { }
