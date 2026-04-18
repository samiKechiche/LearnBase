using LearnBase.API.Data;
using LearnBase.API.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ════════════════════════════════════════════════════════════
// REGISTER ALL SERVICES (Exercise + Tag + PracticeSet)
// ════════════════════════════════════════════════════════════

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Prevents circular reference errors when fetching related data
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Service Registrations (ALL THREE)
builder.Services.AddScoped<ExerciseService>();      // From Exercise Management feature
builder.Services.AddScoped<TagService>();           // From Tag Management feature
builder.Services.AddScoped<PracticeSetService>();   // From Practice Set Management feature

// Swagger/OpenAPI configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database Configuration - SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=LearnBase.db");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();