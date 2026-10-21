namespace DictionaryService.Contracts.Locations.GetTop;

public record AddressDto(
    string City,
    string Street,
    string Building,
    string RoomNumber);