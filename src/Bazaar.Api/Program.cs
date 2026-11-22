using System.Text;
using Bazaar.Api.Features.Listings;
using Bazaar.Api.Features.Users;
using DbUp;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

var connectionString = builder.Configuration.GetConnectionString("Default") ??
                       throw new InvalidOperationException("Please set connection string in appsettings.json");

const int JwtKeyMinLength = 32;

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < JwtKeyMinLength)
    throw new InvalidOperationException($"Jwt:Key must be set and at least {JwtKeyMinLength} characters long.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuerSigningKey = true,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddNpgsqlDataSource(connectionString);

var app = builder.Build();

app.UseExceptionHandler();

var upgrader = DeployChanges.To
    .PostgresqlDatabase(connectionString)
    .WithScriptsEmbeddedInAssembly(typeof(Program).Assembly)
    .LogToConsole()
    .Build();

var result = upgrader.PerformUpgrade();
if (!result.Successful)
    throw result.Error;


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//auth
app.MapSignUp();
app.MapLogin();
app.MapMe();

//listings
app.MapCreateListing();
app.MapGetListing();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();


app.Run();


