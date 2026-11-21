using System.Security.Claims;
using Dapper;
using Npgsql;

namespace Bazaar.Api.Features.Users;

public static class Me
{
    private const string SelectUserByIdSql = """
        SELECT id, username, email, country, city, created_at AS CreatedAt
        FROM users
        WHERE id = @Id
        """;

    public record MeResponse(
        int Id,
        string Username,
        string Email,
        string? Country,
        string? City,
        DateTime CreatedAt
    );

    public static void MapMe(this IEndpointRouteBuilder app)
    {
        app.MapGet("/me", async (ClaimsPrincipal principal, NpgsqlDataSource db) =>
        {
            var subject = principal.FindFirstValue("sub");
            if (!int.TryParse(subject, out var userId))
                return Results.Unauthorized();

            await using var connection = await db.OpenConnectionAsync();

            var user = await connection.QuerySingleOrDefaultAsync<MeResponse>(SelectUserByIdSql, new { Id = userId });

            return user is null ? Results.Unauthorized() : Results.Ok(user);
        }).RequireAuthorization();
    }
}
