using System.Text;
using Flashcards.Api.Persistence;
using Flashcards.Api.Common.Auth;
using Flashcards.Api.Features.Categories;
using Flashcards.Api.Features.Questions;
using Flashcards.Api.Features.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

/*
 * Services
 */

// Scans for minimal API endpoints and produces metadata
builder.Services.AddEndpointsApiExplorer(); 

// Generator/config for Swagger docs
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Flashcards API",
        Description = "An ASP.NET Core Minimal API for Flashcards.",
    });
}); 

// Register DB Context
builder.Services.AddDbContext<ApplicationDbContext>(
    options => options
        .UseSqlite(builder.Configuration
            .GetConnectionString("DefaultConnection")
        ));

// Register PasswordService
builder.Services.AddSingleton<IPasswordService, PasswordService>();

// Register Token service
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

// Token config
var jwtKey = builder.Configuration["Jwt:Key"] 
             ?? throw new InvalidOperationException("Jwt:Key not configured");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "Flashcards.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "Flashcards.Client";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

// Register QuestionService
builder.Services.AddScoped<IQuestionService, QuestionService>();

builder.Services.AddAuthorization();

var app = builder.Build();

/*
 * Middleware
 */
if (app.Environment.IsDevelopment())
{
    // Enable Swagger endpoint
    app.UseSwagger(); 
    // Renders Swagger document
    app.UseSwaggerUI(options => {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Flashcards API");
        if (app.Environment.IsDevelopment())
{
    // Enable Swagger endpoint
    app.UseSwagger(); 
    // Renders Swagger document
    app.UseSwaggerUI(options => {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Flashcards API"); // JSON endpoint
        options.RoutePrefix = "docs"; // UI endpoint (http://localhost:5000/docs)
    }); 
}
    }); 
}

app.UseAuthentication();
app.UseAuthorization();

// app.UseHttpsRedirection();

// Register endpoints
app.MapGet("/api/health", () => Results.Ok()).WithTags("Health");
app.MapUserEndpoints();
app.MapCategoryEndpoints();
app.MapQuestionEndpoints();


app.Run();

// This allows the testApiFactory to find the Web App to inherit from in tests/TestApiFactory.cs
public partial class Program { }


