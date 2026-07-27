using BookingSystem.Application.Interfaces;
using BookingSystem.Domain.Entities;
using BookingSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookingSystem.Infrastructure.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly BookingSystemDbContext _db;

    public ClientRepository(BookingSystemDbContext db) => _db = db;

    public Task<Client?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        _db.Clients.FirstOrDefaultAsync(c => c.Email == email, ct);

    public async Task AddAsync(Client client, CancellationToken ct = default) =>
        await _db.Clients.AddAsync(client, ct);
}
