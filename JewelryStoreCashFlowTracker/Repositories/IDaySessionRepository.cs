using JewelryStoreCashFlowTracker.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JewelryStoreCashFlowTracker.Repositories;

/// <summary>
/// Repository interface for DaySession entity operations.
/// </summary>
public interface IDaySessionRepository
{
    /// <summary>
    /// Get the open session for today, or null if none exists.
    /// </summary>
    Task<DaySession?> GetTodayOpenSessionAsync();

    /// <summary>
    /// Get session by specific date.
    /// </summary>
    Task<DaySession?> GetByDateAsync(DateOnly date);

    /// <summary>
    /// Get session by ID.
    /// </summary>
    Task<DaySession?> GetByIdAsync(int id);

    /// <summary>
    /// Check if there's an open session for today.
    /// </summary>
    Task<bool> HasOpenSessionForDateAsync(DateOnly date);

    /// <summary>
    /// Check if there's a closed session for today.
    /// </summary>
    Task<bool> HasClosedSessionForDateAsync(DateOnly date);

    /// <summary>
    /// Add a new day session.
    /// </summary>
    Task AddAsync(DaySession session);

    /// <summary>
    /// Update an existing day session.
    /// </summary>
    Task UpdateAsync(DaySession session);

    /// <summary>
    /// Close a session with the counted cash amount.
    /// </summary>
    Task CloseSessionAsync(DaySession session, decimal countedCash);

    /// <summary>
    /// Save changes to the database.
    /// </summary>
    Task SaveChangesAsync();

    /// <summary>
    /// Get all day sessions ordered by date descending.
    /// </summary>
    Task<List<DaySession>> GetAllSessionsAsync();
}
