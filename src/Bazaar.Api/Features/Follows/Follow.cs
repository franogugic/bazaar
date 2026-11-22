using System.Security.Claims;
using Dapper;
using Npgsql;

namespace Bazaar.Api.Features.Follows;

public static class Follow
{
    private const string followSql = """
                                     INSERT INTO follows (follower_id, followed_id)
                                     VALUES (@FollowerId, @FollowedId)
                                     ON CONFLICT DO NOTHING
                                     """;

    public record FollowRequest(
        int Id
        );

    public static void MapFollowUser(this IEndpointRouteBuilder app)
    {
        app.MapPost("/users/{id}/follow", async ([AsParameters]FollowRequest request, NpgsqlDataSource db, ClaimsPrincipal principal) =>
        {
            var subject = principal.FindFirstValue("sub");
            if(!int.TryParse(subject, out var userId))
                return Results.Unauthorized();

            if (request.Id == userId)
                return Results.BadRequest("You cannot follow yourself.");

            await using var connection = await db.OpenConnectionAsync();
            try
            {
                await connection.ExecuteAsync(followSql, new { FollowerId = userId, FollowedId = request.Id });
            }
            catch (PostgresException e) when (e.ConstraintName == "follows_followed_fk")
            {
                return Results.NotFound();
            }

            return Results.NoContent();
        }).RequireAuthorization();
    }

}
