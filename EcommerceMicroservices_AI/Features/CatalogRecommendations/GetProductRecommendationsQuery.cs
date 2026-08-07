using MediatR;

namespace EcommerceMicroservices.Ai.Features.CatalogRecommendations;

public record GetProductRecommendationsQuery(string UserPrompt, int Limit = 3) : IRequest<RecommendationResponse>;

public record RecommendationResponse(string AiSummary, List<ProductDto> Products);

public record ProductDto(Guid Id, string Name, string Description, double Price);
