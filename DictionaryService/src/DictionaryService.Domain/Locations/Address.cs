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

    public static Address Create(
        string city,
        string street,
        string building,
        string roomNumber)
    {
        if (string.IsNullOrEmpty(city))
        {
            throw new ArgumentException("Invalid city");
        }

        if (string.IsNullOrEmpty(street))
        {
            throw new ArgumentException("Invalid street");
        }

        if (string.IsNullOrEmpty(building))
        {
            throw new ArgumentException("Invalid building");
        }

        if (string.IsNullOrEmpty(roomNumber))
        {
            throw new ArgumentException("Invalid room number");
        }

        return new Address(city, street, building, roomNumber);
    }
}