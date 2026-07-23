using BookingSystem.Domain.Entities;

namespace BookingSystem.Application.Interfaces;

// Kept intentionally narrow (no generic IRepository<T>) - each method here
// exists because a real use case in the API needs it. Generic repositories
// tend to either leak IQueryable everywhere or hide the query patterns that
// actually matter (like "find overlapping appointments").
public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<bool> HasOverlapAsync(
        Guid providerId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default);

    Task<List<Appointment>> GetForProviderBetweenAsync(
        Guid providerId, DateTime fromUtc, DateTime toUtc, CancellationToken ct = default);

    Task AddAsync(Appointment appointment, CancellationToken ct = default);
}

public interface IProviderRepository
{
    Task<Provider?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Provider>> GetActiveAsync(CancellationToken ct = default);
    Task<List<Availability>> GetAvailabilityAsync(Guid providerId, CancellationToken ct = default);
}

public interface IServiceRepository
{
    Task<Service?> GetByIdAsync(Guid id, CancellationToken ct = default);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
