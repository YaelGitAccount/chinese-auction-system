using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using server_NET.Configurations;
using server_NET.Data;
using server_NET.Middleware;
using System.Text;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// Register DI for layers
builder.Services.AddScoped<server_NET.Services.IDonorService, server_NET.Services.DonorService>();
builder.Services.AddScoped<server_NET.Repositories.IDonorRepository, server_NET.Repositories.DonorRepository>();
builder.Services.AddScoped<server_NET.Services.IGiftService, server_NET.Services.GiftService>();
builder.Services.AddScoped<server_NET.Repositories.IGiftRepository, server_NET.Repositories.GiftRepository>();
builder.Services.AddScoped<server_NET.Services.IUserService, server_NET.Services.UserService>();
builder.Services.AddScoped<server_NET.Repositories.IUserRepository, server_NET.Repositories.UserRepository>();
builder.Services.AddScoped<server_NET.Services.IPurchaseService, server_NET.Services.PurchaseService>();
builder.Services.AddScoped<server_NET.Repositories.IPurchaseRepository, server_NET.Repositories.PurchaseRepository>();
builder.Services.AddScoped<server_NET.Services.ICategoryService, server_NET.Services.CategoryService>();
builder.Services.AddScoped<server_NET.Repositories.ICategoryRepository, server_NET.Repositories.CategoryRepository>();
builder.Services.AddScoped<server_NET.Services.ISystemStateService, server_NET.Services.SystemStateService>();
builder.Services.AddScoped<server_NET.Services.ILotteryService, server_NET.Services.LotteryService>();
builder.Services.AddScoped<server_NET.Repositories.ILotteryRepository, server_NET.Repositories.LotteryRepository>();
builder.Services.AddScoped<server_NET.Services.DatabaseInitializerService>();
builder.Services.AddScoped<server_NET.Helpers.EmailHelper>();
builder.Services.AddScoped<server_NET.Services.IStripeService, server_NET.Services.StripeService>();
builder.Services.AddAuthorization();

// Add EF Core DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT config
var jwtConfig = builder.Configuration.GetSection("JwtConfig").Get<JwtConfig>();
var key = Encoding.ASCII.GetBytes(jwtConfig?.Secret ?? "default-secret-key");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtConfig?.Issuer,
        ValidAudience = jwtConfig?.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Chinese Auction API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement{
        {
            new OpenApiSecurityScheme{
                Reference = new OpenApiReference{
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Initialize database with seed data
using (var scope = app.Services.CreateScope())
{
    var databaseInitializer = scope.ServiceProvider.GetRequiredService<server_NET.Services.DatabaseInitializerService>();
    await databaseInitializer.InitializeAsync();
}

// Configure the HTTP request pipeline.

// Add custom middleware for request logging and error handling
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
