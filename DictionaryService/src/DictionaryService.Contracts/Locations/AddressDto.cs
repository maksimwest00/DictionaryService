namespace DictionaryService.Contracts.Locations;

public record AddressDto(
    string City,
    string Street,
    string Building,
    string RoomNumber);