namespace DictionaryService.Contracts.Locations;

public record CreateLocationDto(
    string Name,
    AddressDto Address,
    string Timezone);