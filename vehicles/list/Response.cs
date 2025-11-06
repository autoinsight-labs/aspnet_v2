using AutoInsight.Common.Response;
namespace AutoInsight.Vehicles.List;

public record Response(
    IReadOnlyCollection<ResponseItem> Data,
    PageInfo PageInfo,
    int Count
) : PagedResponse<ResponseItem>(Data, PageInfo, Count);

public record ResponseItem(
    Guid Id,
    string Plate,
    string Model,
    string Status,
    DateTime EnteredAt,
    DateTime? LeftAt,
    Guid? AssigneeId
);
