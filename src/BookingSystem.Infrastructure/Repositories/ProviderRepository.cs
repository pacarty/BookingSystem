using BookingSystem.Application.Interfaces;
using BookingSystem.Domain.Entities;
using BookingSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Repositories;

public class ProviderRepository : IProviderRepository
{
    private readonly BookingSystemDbContext _db;

    public ProviderRepository(BookingSystemDbContext db) => _db = db;

    public Task<Provider?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Providers.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<List<Provider>> GetActiveAsync(CancellationToken ct = default) =>
        _db.Providers.Where(p => p.IsActive).ToListAsync(ct);

    public Task<List<Availability>> GetAvailabilityAsync(Guid providerId, CancellationToken ct = default) =>
        _db.Availabilities.Where(a => a.ProviderId == providerId).ToListAsync(ct);

    public Task<List<Provider>> GetByServiceIdAsync(Guid serviceId, CancellationToken ct = default) =>
        _db.ProviderServices
            .Where(ps => ps.ServiceId == serviceId && ps.Provider.IsActive)
            .Select(ps => ps.Provider)
            .ToListAsync(ct);
}

public class ServiceRepository : IServiceRepository
{
    private readonly BookingSystemDbContext _db;

    public ServiceRepository(BookingSystemDbContext db) => _db = db;

    public Task<Service?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Services.FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<List<Service>> GetActiveAsync(CancellationToken ct = default) =>
        _db.Services.Where(s => s.IsActive).ToListAsync(ct);
}
