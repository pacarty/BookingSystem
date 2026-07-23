namespace BookingSystem.Domain.Common;

// Every entity gets a Guid key and audit timestamps. Using Guid (not int identity)
// means IDs can be generated client-side before hitting the database, which
// keeps EF Core inserts simple and avoids exposing sequential integer IDs
// over the public API.
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedUtc { get; set; }
}
