using JewelryStoreCashFlowTracker.Models;
using JewelryStoreCashFlowTracker.Repositories;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JewelryStoreCashFlowTracker.Services;

/// <summary>
/// Service implementation for day session management.
/// </summary>
public class DaySessionService : IDaySessionService
{
    private readonly IDaySessionRepository _daySessionRepository;

    public DaySessionService(IDaySessionRepository daySessionRepository)
    {
        _daySessionRepository = daySessionRepository;
    }

    public async Task<DaySession?> GetTodaySessionAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        return await _daySessionRepository.GetByDateAsync(today);
    }

    public async Task<DaySession> StartNewDayAsync(decimal initialCash)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);

        // Check if session already exists
        var existingSession = await _daySessionRepository.GetByDateAsync(today);
        if (existingSession != null)
        {
            throw new InvalidOperationException("A session already exists for today.");
        }

        if (initialCash < 0)
        {
            throw new ArgumentException("Initial cash cannot be negative.", nameof(initialCash));
        }

        var session = new DaySession
        {
            Date = today,
            InitialCash = initialCash,
            IsClosed = false,
            OpenedAt = DateTime.Now,
            Transactions = new List<Transaction>()
        };

        await _daySessionRepository.AddAsync(session);
        await _daySessionRepository.SaveChangesAsync();

        Log.Information("Day session opened for {Date} with initial cash {InitialCash:C}", today, initialCash);

        return session;
    }

    public async Task CloseDayAsync(decimal countedCash)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var session = await _daySessionRepository.GetTodayOpenSessionAsync();

        if (session == null)
        {
            throw new InvalidOperationException("No open session found for today.");
        }

        if (countedCash < 0)
        {
            throw new ArgumentException("Counted cash cannot be negative.", nameof(countedCash));
        }

        await _daySessionRepository.CloseSessionAsync(session, countedCash);
        await _daySessionRepository.SaveChangesAsync();

        Log.Information("Day session closed for {Date}. Counted: {Counted:C}, Difference: {Difference:C}",
            today, countedCash, session.Difference);
    }

    public async Task<bool> CanAddTransactionAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        return await _daySessionRepository.HasOpenSessionForDateAsync(today);
    }

    public async Task<bool> HasClosedSessionTodayAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        return await _daySessionRepository.HasClosedSessionForDateAsync(today);
    }
}
