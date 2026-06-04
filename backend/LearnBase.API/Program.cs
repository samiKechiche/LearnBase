using LearnBase.API.Data;
using LearnBase.API.Models;
using LearnBase.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);
var jwtSettings = builder.Configuration.GetSection("JwtSettings");

// --- configure multipart/form limits ---
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100_000_000; // 100 MB example - adjust to your needs
    // options.ValueLengthLimit = ...
});

// Configure Kestrel max request body size (global)
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 100_000_000; // 100 MB example
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Key"])),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };

    o.Events = new JwtBearerEvents
    {
        OnMessageReceived = ctx =>
        {
            var authHeader = ctx.Request.Headers["Authorization"].FirstOrDefault();
            Console.WriteLine($"[JwtBearer] OnMessageReceived - Authorization header: {authHeader}");
            return Task.CompletedTask;
        },
        OnTokenValidated = ctx =>
        {
            var loggerFactory = ctx.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("JwtBearerEvents");
            var sub = ctx.Principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            logger.LogInformation("Token validated for user {sub}", sub);
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = ctx =>
        {
            var loggerFactory = ctx.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("JwtBearerEvents");
            logger.LogError(ctx.Exception, "Authentication failed (detailed): {Message}", ctx.Exception?.Message);
            Console.WriteLine("[JwtBearer] Authentication failed: " + ctx.Exception?.ToString());
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// ALL SERVICES REGISTERED HERE
builder.Services.AddScoped<ExerciseService>();
builder.Services.AddScoped<TagService>();
builder.Services.AddScoped<PracticeSetService>();
builder.Services.AddScoped<PracticeSessionService>();
builder.Services.AddScoped<PracticeSessionStatsService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<PasswordHasherService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<LessonService>();
builder.Services.AddScoped<NoteService>();
builder.Services.AddScoped<ImportExportService>();
builder.Services.AddScoped<FileService>();



builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JWTToken_Auth_ API",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer <token>'",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=LearnBase.db"));

builder.Services.AddCors();

var app = builder.Build();

// Auto apply migrations (dev convenience)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Global exception middleware to catch and log early errors
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetService<ILogger<Program>>();
        logger?.LogError(ex, "Unhandled exception in pipeline");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Internal server error");
    }
});

// AppDomain / TaskScheduler global handlers to surface crash-causing exceptions to logs
AppDomain.CurrentDomain.UnhandledException += (s, e) =>
{
    try
    {
        var logger = app.Services.GetService<ILogger<Program>>();
        logger?.LogCritical(e.ExceptionObject as Exception, "UnhandledException");
    }
    catch { }
};

TaskScheduler.UnobservedTaskException += (s, e) =>
{
    try
    {
        var logger = app.Services.GetService<ILogger<Program>>();
        logger?.LogError(e.Exception, "UnobservedTaskException");
    }
    catch { }
};

// CORS - Allow Angular frontend to call the API
app.UseCors(policy => policy
    .WithOrigins("http://localhost:4200")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.MapControllers();

app.Run();