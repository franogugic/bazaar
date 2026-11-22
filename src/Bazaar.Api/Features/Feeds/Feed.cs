using System.Security.Claims;
using Dapper;
using Npgsql;

namespace Bazaar.Api.Features.Feeds;

public static class Feed
{
    private const int FeedPageSize = 8;

    private const string GetFeedsSql = """
                                                SELECT l.id, l.title, l.description, l.price, l.quantity, l.created_at, u.country, u.city, u.username, u.id AS seller_id
                                                FROM Follows f
                                                JOIN Listings l ON f.followed_id = l.seller_id
                                                join users u on u.id = l.seller_id
                                                WHERE f.follower_id = @UserId
                                                    AND l.quantity > 0
                                                    AND (@BeforeId::int IS NULL
                                                        OR (l.created_at, l.id) < (@BeforeCreatedAt::timestamptz, @BeforeId::int))
                                                ORDER BY l.created_at DESC, l.id DESC
                                                LIMIT @PageSize
                                            """;
    
    public static void GetFeeds(this IEndpointRouteBuilder app)
    {
        app.MapGet("/feeds", async (DateTimeOffset? beforeCreatedAt, int? beforeId, NpgsqlDataSource db, ClaimsPrincipal principal) =>
        {
            var subject = principal.FindFirstValue("sub");
            if (!int.TryParse(subject, out var userId))
                return Results.Unauthorized();

            if (beforeCreatedAt.HasValue != beforeId.HasValue)
                return Results.BadRequest("Send both beforeCreatedAt and beforeId, or neither.");

            await using var connection = await db.OpenConnectionAsync();
            var feed = (await connection.QueryAsync(GetFeedsSql, new
            {
                UserId = userId,
                BeforeCreatedAt = beforeCreatedAt?.ToUniversalTime(),
                BeforeId = beforeId,
                PageSize = FeedPageSize
            })).ToList();

            if (feed.Count == 0)
                return Results.NotFound();

            return Results.Ok(feed);
        }).RequireAuthorization();
    }
}