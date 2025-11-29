namespace DictionaryService.Domain.Departments;

public record Address
{
    public string City { get; private set; }
    public string Street { get; private set; }
    public string Building { get; private set; }
    public string RoomNumber { get; private set; }

    private Address()
    {
    }

    public static Address Create(
        string city,
        string street,
        string building,
        string roomNumber)
    {
        if (string.IsNullOrEmpty(city))
            throw new ArgumentException("Invalid city");
        if (string.IsNullOrEmpty(street))
            throw new ArgumentException("Invalid street");
        if (string.IsNullOrEmpty(building))
            throw new ArgumentException("Invalid building");
        if (string.IsNullOrEmpty(roomNumber))
            throw new ArgumentException("Invalid room number");

        return new Address
        {
            City = city,
            Street = street,
            Building = building,
            RoomNumber = roomNumber,
        };
    }
}