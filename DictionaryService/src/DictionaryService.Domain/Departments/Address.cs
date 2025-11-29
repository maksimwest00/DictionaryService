namespace DictionaryService.Domain.Departments;

public record Address
{
    public string City { get; set; }
    public string Street { get; set; }
    public string Building { get; set; }
    public string RoomNumber { get; set; }

    public static Address Create(
        string city,
        string street,
        string building,
        string roomNumber)
    {
        return new Address
        {
            City = city,
            Street = street,
            Building = building,
            RoomNumber = roomNumber,
        };
    }
}