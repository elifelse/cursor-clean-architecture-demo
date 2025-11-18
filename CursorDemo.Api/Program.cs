using System.Text;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using CursorDemo.Api.Configuration;
using System.Reflection;
using CursorDemo.Api.Extensions;
using CursorDemo.Api.Filters;
using CursorDemo.Api.Middleware;
using CursorDemo.Api.Models;
using CursorDemo.Application.Configuration;
using CursorDemo.Application.Interfaces;
using CursorDemo.Application.Services;
using CursorDemo.Domain.Interfaces;
using CursorDemo.Infrastructure.Repositories;
using CursorDemo.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .Build())
    .CreateLogger();

try
{
    Log.Information("Starting web application");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for logging
    builder.Host.UseSerilog();

    // Add API Versioning
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = ApiVersionReader.Combine(
            new UrlSegmentApiVersionReader()
        );
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
    });

    // Add services to the container
    builder.Services.AddControllers(options =>
    {
        // Add validation filter to format FluentValidation errors
        options.Filters.Add<ValidationFilter>();
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Disable automatic model state validation to use FluentValidation instead
        options.SuppressModelStateInvalidFilter = true;
    });

    builder.Services.AddEndpointsApiExplorer();

    // Configure JWT Settings
    var jwtSettings = new JwtSettings();
    builder.Configuration.GetSection("JwtSettings").Bind(jwtSettings);
    builder.Services.AddSingleton(jwtSettings);

    // Configure JWT Authentication
    var key = Encoding.UTF8.GetBytes(jwtSettings.Key);
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
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

    // Configure Swagger with JWT support and API versioning
    builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
    builder.Services.AddSwaggerGen(options =>
    {
        // Filter endpoints by API version - use ApiExplorer's GroupName
        options.DocInclusionPredicate((docName, apiDesc) =>
        {
            // ApiExplorer automatically sets GroupName based on API version
            // Match the document name with the GroupName
            return apiDesc.GroupName == docName;
        });

        // Include XML comments for better Swagger documentation
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }

        // Add JWT authentication to Swagger
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // FluentValidation
    builder.Services.AddFluentValidationServices();
    builder.Services.AddFluentValidationAutoValidation(config =>
    {
        // Configure FluentValidation to run automatically
        config.DisableDataAnnotationsValidation = true;
    });

    // Caching
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

    // Dependency Injection
    builder.Services.AddScoped<IBookRepository, InMemoryBookRepository>();
    builder.Services.AddScoped<IBookService, BookService>();
    builder.Services.AddScoped<ITokenService, TokenService>();

    // Background Services
    builder.Services.AddHostedService<BackgroundBookRefresherService>();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
            foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions.Reverse())
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    $"CursorDemo API {description.GroupName.ToUpperInvariant()}");
            }
        });
    }

    app.UseHttpsRedirection();
    
    // Enable static files for frontend
    app.UseStaticFiles();
    
    app.UseRequestLogging();
    app.UseGlobalExceptionMiddleware();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    
    // Default route to index.html
    app.MapFallbackToFile("index.html");

    Log.Information("Application started successfully");
    
    // Don't run the app in test environment (WebApplicationFactory handles this)
    if (app.Environment.EnvironmentName != "Testing")
    {
        app.Run();
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for WebApplicationFactory in integration tests
public partial class Program { }

