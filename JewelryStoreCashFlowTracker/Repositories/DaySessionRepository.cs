using JewelryStoreCashFlowTracker.Database;
using JewelryStoreCashFlowTracker.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JewelryStoreCashFlowTracker.Repositories;

/// <summary>
/// Repository implementation for DaySession entity.
/// </summary>
public class DaySessionRepository : IDaySessionRepository
{
    private readonly AppDbContext _context;

    public DaySessionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DaySession?> GetTodayOpenSessionAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        return await _context.DaySessions
            .Include(s => s.Transactions)
            .FirstOrDefaultAsync(s => s.Date == today && !s.IsClosed);
    }

    public async Task<DaySession?> GetByDateAsync(DateOnly date)
    {
        return await _context.DaySessions
            .Include(s => s.Transactions)
            .FirstOrDefaultAsync(s => s.Date == date);
    }

    public async Task<DaySession?> GetByIdAsync(int id)
    {
        return await _context.DaySessions
            .Include(s => s.Transactions)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<bool> HasOpenSessionForDateAsync(DateOnly date)
    {
        return await _context.DaySessions
            .AnyAsync(s => s.Date == date && !s.IsClosed);
    }

    public async Task<bool> HasClosedSessionForDateAsync(DateOnly date)
    {
        return await _context.DaySessions
            .AnyAsync(s => s.Date == date && s.IsClosed);
    }

    public async Task AddAsync(DaySession session)
    {
        await _context.DaySessions.AddAsync(session);
    }

    public Task UpdateAsync(DaySession session)
    {
        _context.DaySessions.Update(session);
        return Task.CompletedTask;
    }

    public Task CloseSessionAsync(DaySession session, decimal countedCash)
    {
        var expectedCash = CalculateExpectedCash(session);

        session.CountedCash = countedCash;
        session.Difference = countedCash - expectedCash;
        session.IsClosed = true;
        session.ClosedAt = DateTime.Now;

        _context.DaySessions.Update(session);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<List<DaySession>> GetAllSessionsAsync()
    {
        return await _context.DaySessions
            .Include(s => s.Transactions)
            .OrderByDescending(s => s.Date)
            .ToListAsync();
    }

    private static decimal CalculateExpectedCash(DaySession session)
    {
        var totalEnter = session.Transactions
            .Where(t => !t.IsDeleted && t.Type == TransactionType.MoneyEnter)
            .Sum(t => t.Amount);

        var totalLeave = session.Transactions
            .Where(t => !t.IsDeleted && t.Type == TransactionType.MoneyLeave)
            .Sum(t => t.Amount);

        return session.InitialCash + totalEnter - totalLeave;
    }
}
