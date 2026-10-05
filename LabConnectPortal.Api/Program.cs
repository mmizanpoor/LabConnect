using LabConnectPortal.Api.Extensions;
using LabConnectPortal.Api.Infrastructure.Context;
using LabConnectPortal.Api.Infrastructure.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LabConnectPortal.Api.Infrastructure.Middleware;
using Microsoft.OpenApi.Models;
using Syncfusion.Licensing;

var builder = WebApplication.CreateBuilder(args);

var syncfusionLicenseKey = builder.Configuration["Syncfusion:LicenseKey"];
if (!string.IsNullOrWhiteSpace(syncfusionLicenseKey))
{
    SyncfusionLicenseProvider.RegisterLicense(syncfusionLicenseKey);
}

builder.Services.AddLabConnectPortal(builder.Configuration);
builder.Services.AddScoped<RequireLabPermissionFilter>();
builder.Services.AddScoped<RequireSitePermissionFilter>();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<RequireSystemEntityFilter>();
    options.Filters.AddService<RequireLabPermissionFilter>();
    options.Filters.AddService<RequireSitePermissionFilter>();
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    // Must run before JsonStringEnumConverter so "" maps to null instead of HTTP 400.
    options.JsonSerializerOptions.Converters.Add(
        new LabConnectPortal.Api.Infrastructure.Json.EmptyStringNullableEnumConverterFactory());
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.MapType<FileContentResult>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary",
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "ابتدا از Auth/LoginWithPassword توکن بگیرید و اینجا paste کنید",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                },
            },
            Array.Empty<string>()
        },
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LabConnectDbContext>();
    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    await DbSeeder.SeedAsync(db, env.ContentRootPath);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<SystemEntityMiddleware>();
app.UseMiddleware<UserActivityMiddleware>();
app.UseAuthorization();
app.MapControllers();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");
app.Run();
