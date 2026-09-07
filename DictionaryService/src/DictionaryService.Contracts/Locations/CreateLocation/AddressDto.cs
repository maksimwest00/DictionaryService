namespace DictionaryService.Contracts.Locations.CreateLocation;

public record AddressDto(
    string City,
    string Street,
    string Building,
    string RoomNumber);