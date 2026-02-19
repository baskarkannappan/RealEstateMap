using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using RealEstateMap.Api.Options;
using RealEstateMap.Api.Services;
using RealEstateMap.Api.Services.Abstractions;
using RealEstateMap.Api.Services.Caching;
using RealEstateMap.Api.Services.Database;
using RealEstateMap.DAL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var cacheOptions = builder.Configuration.GetSection(CacheOptions.SectionName).Get<CacheOptions>() ?? new CacheOptions();
builder.Services.AddMemoryCache(options =>
{
    if (cacheOptions.MemorySizeLimit > 0)
    {
        options.SizeLimit = cacheOptions.MemorySizeLimit;
    }
});
builder.Services.Configure<DataSourceOptions>(builder.Configuration.GetSection(DataSourceOptions.SectionName));
builder.Services.Configure<CacheOptions>(builder.Configuration.GetSection(CacheOptions.SectionName));

if (cacheOptions.EnableDistributedCache)
{
    if (!string.IsNullOrWhiteSpace(cacheOptions.RedisConnectionString))
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = cacheOptions.RedisConnectionString;
            options.InstanceName = "RealEstateMapApi";
        });
    }
    else
    {
        builder.Services.AddDistributedMemoryCache();
    }
}

builder.Services.AddSingleton<FakeDataService>();
builder.Services.AddScoped<FakeHouseDataService>();
builder.Services.AddSingleton<ICacheService, HybridCacheService>();
builder.Services.AddRealEstateDal(builder.Configuration);
builder.Services.AddScoped<IHouseQueryService, HouseQueryService>();
builder.Services.AddScoped<IHouseDbService, HouseDbService>();
builder.Services.AddScoped<IHouseDataService, SwitchableHouseDataService>();
builder.Services.AddHostedService<HouseCacheWarmupService>();

var jwtSection = builder.Configuration.GetSection("Jwt");
var signingKey = jwtSection["SigningKey"] ?? throw new InvalidOperationException("Missing Jwt:SigningKey configuration.");
var issuer = jwtSection["Issuer"] ?? "RealEstateMap.Api";
var audience = jwtSection["Audience"] ?? "RealEstateMap.Client";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey))
        };
    });

builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5209"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("api", limiterOptions =>
    {
        limiterOptions.PermitLimit = 120;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 20;
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("BlazorClient");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers().RequireRateLimiting("api");

app.Run();
