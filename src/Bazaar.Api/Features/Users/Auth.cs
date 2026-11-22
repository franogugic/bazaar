using System.Security.Claims;
using System.Text;
using Dapper;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using JwtRegisteredClaimNames = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

namespace Bazaar.Api.Features.Users;

public static class Auth
{
    private const int UsernameMinLength = 4;
    private const int UsernameMaxLength = 30;
    private const int EmailMinLength = 3;
    private const int EmailMaxLength = 254;
    private const int PasswordMinLength = 8;
    private const int PasswordMaxLength = 50;
    private const int PlaceMinLength = 3;
    private const int PlaceMaxLength = 50;

    private const string InsertUserSql = """
        INSERT INTO users (username, email, password_hash, country, city)
        VALUES (@Username, @Email, @PasswordHash, @Country, @City)
        RETURNING id, username, email, country, city, created_at AS CreatedAt
        """;

    private const string LoginUserSql = """
                                        SELECT id, username, email, password_hash AS PasswordHash, country, city, created_at AS CreatedAt
                                        FROM users
                                        WHERE lower(email) = lower(@Email)
                                        """;

    private const int TokenLifetimeHours = 1;
    
    private record UserRow(
        int Id,
        string Username,
        string Email,
        string PasswordHash,
        string? Country,
        string? City,
        DateTime CreatedAt
    );
    
    public record SignUpRequest(
        string Username,
        string Email,
        string Password,
        string ConfirmPassword,
        string? Country,
        string? City
        );
    
    public record LoginRequest(
        string Email,
        string Password
    );

    public record SignUpResponse(
        int Id,
        string Username,
        string Email,
        string? Country,
        string? City,
        DateTime CreatedAt
        );
    
    public record LoginResponse(
        string Username, 
        string? Country, 
        string? City,
        string Token
        );

    private static string CreateToken(UserRow user, string key)
    {
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            ]),
            Expires = DateTime.UtcNow.AddHours(TokenLifetimeHours),
            SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    public static void MapSignUp(this IEndpointRouteBuilder app)
    {
        app.MapPost("/signup", async (SignUpRequest req, NpgsqlDataSource db) =>
        {
            if(string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Email) ||
               string.IsNullOrWhiteSpace(req.Password) || string.IsNullOrWhiteSpace(req.ConfirmPassword))
                return Results.BadRequest("Username, email and password are required.");

            if (req.Username.Length < UsernameMinLength || req.Username.Length > UsernameMaxLength)
                return Results.BadRequest($"Username must be between {UsernameMinLength} and {UsernameMaxLength} characters long.");

            if(req.Email.Length < EmailMinLength || req.Email.Length > EmailMaxLength || req.Email.IndexOf('@') < 1)
                return Results.BadRequest("Invalid email format.");

            if(req.Password.Length < PasswordMinLength || req.Password.Length > PasswordMaxLength ||
               !req.Password.Any(char.IsUpper) || !req.Password.Any(char.IsDigit))
                return Results.BadRequest("Invalid password.");

            if(req.Password != req.ConfirmPassword)
                return Results.BadRequest("Passwords do not match.");

            if(req.Country!=null && (req.Country.Length < PlaceMinLength || req.Country.Length > PlaceMaxLength))
                return Results.BadRequest("Invalid country.");

            if(req.City!=null && (req.City.Length < PlaceMinLength || req.City.Length > PlaceMaxLength))
                return Results.BadRequest("Invalid city.");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(req.Password);
            
            SignUpResponse created;
            await using var connection = await db.OpenConnectionAsync();
            try
            {
                 created = await connection.QuerySingleAsync<SignUpResponse>(InsertUserSql,
                    new { req.Username, req.Email, PasswordHash = passwordHash, req.Country, req.City });
            }
            catch (PostgresException e) 
            {
                if(e.ConstraintName == "user_username_unique") 
                    return Results.Conflict("Username already taken.");
                
                if(e.ConstraintName == "user_email_unique")
                    return Results.Conflict("Could not create that account.");

                throw;
            }
            
            return Results.Created($"/users/{created.Id}", created);
        });
    }

    public static void MapLogin(this IEndpointRouteBuilder app)
    {
        app.MapPost("/login", async (LoginRequest req, NpgsqlDataSource db, IConfiguration config) =>
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return Results.BadRequest("Email and password are required.");

            if (req.Email.Length < EmailMinLength || req.Email.Length > EmailMaxLength || req.Email.IndexOf('@') < 1)
                return Results.BadRequest("Invalid email format.");

            if (req.Password.Length < PasswordMinLength || req.Password.Length > PasswordMaxLength ||
                !req.Password.Any(char.IsUpper) || !req.Password.Any(char.IsDigit))
                return Results.BadRequest("Invalid password.");

            await using var connection = await db.OpenConnectionAsync();
            
            var user = await connection.QuerySingleOrDefaultAsync<UserRow>(LoginUserSql,
                    new { req.Email });
            
            if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user?.PasswordHash))
            {
                return Results.Json("Invalid email or password.", statusCode: StatusCodes.Status401Unauthorized);
            }

            var token = CreateToken(user, config["Jwt:Key"]);
            
            var loggedUser = new LoginResponse(
                user.Username,
                user.Country,
                user.City,
                token
            );
            
            return Results.Ok(loggedUser);
        });
    }
}