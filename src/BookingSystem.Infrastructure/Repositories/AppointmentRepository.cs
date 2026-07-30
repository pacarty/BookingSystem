using BookingSystem.Application.Interfaces;
using BookingSystem.Domain.Entities;
using BookingSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly BookingSystemDbContext _db;

    public AppointmentRepository(BookingSystemDbContext db) => _db = db;

    public Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _db.Appointments
            .Include(a => a.Provider)
            .Include(a => a.Client)
            .Include(a => a.Service)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public Task<bool> HasOverlapAsync(Guid providerId, DateTime startUtc, DateTime endUtc, CancellationToken ct = default) =>
        _db.Appointments.AnyAsync(a =>
            a.ProviderId == providerId &&
            a.Status != Domain.Enums.AppointmentStatus.Cancelled &&
            startUtc < a.EndUtc && a.StartUtc < endUtc, ct);

    public Task<List<Appointment>> GetForProviderBetweenAsync(
    Guid providerId, DateTime fromUtc, DateTime toUtc, CancellationToken ct = default) =>
    _db.Appointments
        .Include(a => a.Provider)
        .Include(a => a.Client)
        .Include(a => a.Service)
        .Where(a => a.ProviderId == providerId &&
                    a.Status != Domain.Enums.AppointmentStatus.Cancelled &&
                    a.StartUtc < toUtc && fromUtc < a.EndUtc)
        .ToListAsync(ct);

    public Task<List<Appointment>> GetAllBetweenAsync(DateTime fromUtc, DateTime toUtc, CancellationToken ct = default) =>
    _db.Appointments
        .Include(a => a.Provider)
        .Include(a => a.Client)
        .Include(a => a.Service)
        .Where(a => a.Status != Domain.Enums.AppointmentStatus.Cancelled &&
                    a.StartUtc < toUtc && fromUtc < a.EndUtc)
        .OrderBy(a => a.StartUtc)
        .ToListAsync(ct);

    public async Task AddAsync(Appointment appointment, CancellationToken ct = default) =>
        await _db.Appointments.AddAsync(appointment, ct);
}
