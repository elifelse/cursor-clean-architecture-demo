using System.Text;
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
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

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

    // Configure Swagger with JWT support
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "CursorDemo API",
            Version = "v1",
            Description = "Clean Architecture Demo API with JWT Authentication and FluentValidation"
        });

        // Include XML comments for better Swagger documentation
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }

        // Add JWT authentication to Swagger
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

    // Dependency Injection
    builder.Services.AddScoped<IBookRepository, InMemoryBookRepository>();
    builder.Services.AddScoped<IBookService, BookService>();
    builder.Services.AddScoped<ITokenService, TokenService>();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseRequestLogging();
    app.UseGlobalExceptionMiddleware();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("Application started successfully");
    app.Run();
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

