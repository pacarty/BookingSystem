using BookingSystem.Application.Interfaces;
using BookingSystem.Infrastructure.Persistence;

namespace BookingSystem.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly BookingSystemDbContext _db;

    public UnitOfWork(BookingSystemDbContext db) => _db = db;

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
