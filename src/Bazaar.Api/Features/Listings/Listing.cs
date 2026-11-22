using System.Security.Claims;
using Dapper;
using Npgsql;

namespace Bazaar.Api.Features.Listings;

public static class Listing
{
    private const int TitleMinLength = 3;
    private const int TitleMaxLength = 100;
    private const int DescriptionMaxLength = 5000;
    private const int PriceMaxDecimals = 2;
    private const decimal PriceLimit = 10_000_000_000m; 
    private const int QuantityMax = 100_000;

    private const string createListingSql = """
                                            INSERT INTO Listings(seller_id, title, description, price, quantity)
                                            VALUES (@SellerId, @Title, @Description, @Price, @Quantity)
                                            RETURNING  id, title, description, price, quantity
                                            """;

    private const string getListingSql = """
                                         SELECT l.id, l.title, l.description, l.price, l.quantity, l.seller_id, u.username, u.country, u.city
                                         FROM Listings l
                                         JOIN Users u ON l.seller_id = u.id
                                         WHERE l.id = @Id
                                         """;
    
    public record CreateListingsRequest(
        string Title,
        string Description,
        decimal Price,
        int Quantity
    );

    public record GetListingRequest(
        int Id
    );

    public record CreateListingResponse(
        int Id,
        string Title,
        string Description,
        decimal Price,
        int Quantity
    );





    public static void MapCreateListing(this IEndpointRouteBuilder app)
    {
        app.MapPost("/listings", async (CreateListingsRequest request, NpgsqlDataSource db, ClaimsPrincipal principal) =>
        {
            var subject = principal.FindFirstValue("sub");
            if(!int.TryParse(subject, out var userId))
                return Results.Unauthorized();

            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description))
                return Results.BadRequest("Title and description are required.");

            if (request.Title.Length < TitleMinLength || request.Title.Length > TitleMaxLength)
                return Results.BadRequest($"Title must be between {TitleMinLength} and {TitleMaxLength} characters long.");

            if (request.Description.Length > DescriptionMaxLength)
                return Results.BadRequest($"Description must be at most {DescriptionMaxLength} characters long.");

            if (request.Price <= 0 || request.Price >= PriceLimit)
                return Results.BadRequest("Invalid price.");

            if (decimal.Round(request.Price, PriceMaxDecimals) != request.Price)
                return Results.BadRequest($"Price can have at most {PriceMaxDecimals} decimal places.");

            if (request.Quantity < 1 || request.Quantity > QuantityMax)
                return Results.BadRequest($"Quantity must be between 1 and {QuantityMax}.");
            
            await using var connection = await db.OpenConnectionAsync();
            var newListing = await connection.QuerySingleAsync<CreateListingResponse>(createListingSql, 
                new {SellerId = userId, request.Title, request.Description, request.Price, request.Quantity});
            
            return Results.Created($"/listings/{newListing.Id}", newListing);
        }).RequireAuthorization();
    }

    public static void MapGetListing(this IEndpointRouteBuilder app)
    {
        app.MapGet("/listings/{Id}", async ([AsParameters]GetListingRequest request, NpgsqlDataSource db) =>
        {
            if (request.Id <= 0)
                return Results.NotFound();

            await using var connection = await db.OpenConnectionAsync();
            var result = await connection.QuerySingleOrDefaultAsync(getListingSql, new {Id = request.Id});
            
            if(result == null)
                return Results.NotFound();
            
            return Results.Ok(result);
        });
    }
}