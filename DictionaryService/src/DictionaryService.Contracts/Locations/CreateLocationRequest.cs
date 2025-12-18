namespace DictionaryService.Contracts.Locations;

public record CreateLocationRequest(
    string Name,
    AddressDto Address,
    string Timezone);