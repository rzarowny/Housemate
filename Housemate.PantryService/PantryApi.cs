using Housemate.PantryDb;

namespace Housemate.PantryService;

public static class PantryApi
{
    public static RouteGroupBuilder MapPantryApi(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/pantry");

        group.WithTags("Pantry");

        group.MapGet("items/location/all", (PantryDbContext pantryContext, int? before, int? after, int pageSize = 8)
            => GetPantryItems(null, pantryContext, before, after, pageSize))
            .Produces(StatusCodes.Status400BadRequest)
            .Produces<PantryItemsPage>();

        group.MapGet("items/location/all/category/{pantryCategoryId:int}", (int pantryCategoryId, PantryDbContext pantryContext, int? before, int? after, int pageSize = 8)
            => GetPantryItems(pantryCategoryId, pantryContext, before, after, pageSize))
            .Produces(StatusCodes.Status400BadRequest)
            .Produces<PantryItemsPage>();

        static async Task<IResult> GetPantryItems(int? pantryCategoryId, PantryDbContext pantryContext, int? before, int? after, int pageSize)
        {
            if (before is > 0 && after is > 0)
            {
                return TypedResults.BadRequest($"Invalid paging parameters. Only one of {nameof(before)} or {nameof(after)} can be specified, not both.");
            }

            var itemsOnPage = await pantryContext.GetPantryItemsCompiledAsync(pantryCategoryId, before, after, pageSize);

            var (firstId, nextId) = itemsOnPage switch
            {
            [] => (0, 0),
            [var only] => (only.Id, only.Id),
            [var first, .., var last] => (first.Id, last.Id)
            };

            return Results.Ok(new PantryItemsPage(
                firstId,
                nextId,
                itemsOnPage.Count < pageSize,
                itemsOnPage.Take(pageSize)));
        }

        return group;
    }
}

public record PantryItemsPage(int FirstId, int NextId, bool IsLastPage, IEnumerable<PantryItem> Data);
