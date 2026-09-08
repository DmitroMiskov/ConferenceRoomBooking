namespace ConferenceRoomBooking.Domain;

public class Service
{
    public Guid Id { get; init; }
    public string Name { get; private set; } = null!;
    public decimal Price { get; private set; }

    public Service(string name, decimal price)
    {
        Id = Guid.NewGuid();
        if(string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Service name cannot be null or empty.", nameof(name));
        }
        Name = name;
        if(price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Service price cannot be negative.");
        }
        Price = price;
    }

    private Service() { } 
}
