using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StockFlow.Application.Interfaces;
using StockFlow.Infrastructure.Bootstrap;
using StockFlow.Infrastructure.Extensions;
using StockFlow.Infrastructure.Services;
using StockFlow.Api.Extensions;
using StockFlow.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, HttpUserContext>();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var problemDetails = context.HttpContext.CreateValidationProblemDetails(context.ModelState, "The request payload is invalid.");
            return new BadRequestObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json" }
            };
        };
    });
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "StockFlow API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Use a valid JWT token."
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

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
EnsureSecretConfigured("Jwt:Key", jwtOptions.Key);

var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();

                if (context.Response.HasStarted)
                {
                    return;
                }

                var problemDetails = context.HttpContext.CreateProblemDetails(
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    "Authentication is required to access this resource.",
                    "unauthorized");

                await context.HttpContext.WriteProblemDetailsAsync(problemDetails);
            },
            OnForbidden = context =>
            {
                var problemDetails = context.HttpContext.CreateProblemDetails(
                    StatusCodes.Status403Forbidden,
                    "Forbidden",
                    "You do not have permission to access this resource.",
                    "forbidden");

                return context.HttpContext.WriteProblemDetailsAsync(problemDetails);
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase))
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

await app.Services.ApplyBootstrapAsync();

static void EnsureSecretConfigured(string settingName, string? value)
{
    if (!string.IsNullOrWhiteSpace(value) && !value.Contains("__SET_IN_USER_SECRETS_OR_ENV__", StringComparison.Ordinal))
    {
        return;
    }

    throw new InvalidOperationException(
        $"Configuration '{settingName}' is not set. Configure local secrets with dotnet user-secrets or environment variables before starting StockFlow.");
}

app.Run();

public partial class Program;
