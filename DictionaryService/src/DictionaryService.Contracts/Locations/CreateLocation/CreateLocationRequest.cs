namespace DictionaryService.Contracts.Locations.CreateLocation;

public record CreateLocationRequest(
    string Name,
    AddressDto Address,
    string Timezone);