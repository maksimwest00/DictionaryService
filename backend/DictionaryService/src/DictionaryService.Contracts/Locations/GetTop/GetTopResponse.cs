namespace DictionaryService.Contracts.Locations.GetTop;

public record GetTopResponse
{
    public List<LocationDto> Locations { get; init; } = null!;
}

public record LocationDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public AddressDto Address { get; init; } = null!;
    public int DepartmentCount { get; init; }
}

