namespace DictionaryService.Contracts.Locations.GetLocationsByFilters;

public record LocationListItemResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public DateTime CreatedAt { get; init; }
    public int DepartmentCount { get; init; }
    public AddressDto Address { get; set; }
}

public record AddressDto
{
    public string City { get; init; }
    public string Street { get; init; }
    public string Building { get; init; }
    public string RoomNumber { get; init; }
}