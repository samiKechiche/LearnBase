using LearnBase.API.Data;
using LearnBase.API.Models;
using LearnBase.API.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// ALL FOUR SERVICES REGISTERED HERE
builder.Services.AddScoped<ExerciseService>();
builder.Services.AddScoped<TagService>();
builder.Services.AddScoped<PracticeSetService>();
builder.Services.AddScoped<PracticeSessionService>();  // ← MAKE SURE THIS LINE EXISTS!

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=LearnBase.db"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();