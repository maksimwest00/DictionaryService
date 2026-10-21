using CSharpFunctionalExtensions;
using DictionaryService.Domain.Shared;

namespace DictionaryService.Domain.Locations;

public record Address
{
    private Address(
        string city,
        string street,
        string building,
        string roomNumber)
    {
        City = city;
        Street = street;
        Building = building;
        RoomNumber = roomNumber;
    }

    public string City { get; }
    public string Street { get; }
    public string Building { get; }
    public string RoomNumber { get; }

    public static Result<Address, Error> Create(
        string city,
        string street,
        string building,
        string roomNumber)
    {
        if (string.IsNullOrEmpty(city))
        {
            return GeneralErrors.ValueIsRequired("Address City");
        }

        if (string.IsNullOrEmpty(street))
        {
            return GeneralErrors.ValueIsRequired("Address Street");
        }

        if (string.IsNullOrEmpty(building))
        {
            return GeneralErrors.ValueIsRequired("Address Building");
        }

        if (string.IsNullOrEmpty(roomNumber))
        {
            return GeneralErrors.ValueIsRequired("Address RoomNumber");
        }

        return new Address(city, street, building, roomNumber);
    }
}