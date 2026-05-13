using JewelryStoreCashFlowTracker.Models;
using System.Threading.Tasks;

namespace JewelryStoreCashFlowTracker.Services;

/// <summary>
/// Service for managing day session lifecycle.
/// </summary>
public interface IDaySessionService
{
    /// <summary>
    /// Get today's session if one exists.
    /// </summary>
    Task<DaySession?> GetTodaySessionAsync();

    /// <summary>
    /// Start a new day with the given initial cash amount.
    /// </summary>
    /// <exception cref="InvalidOperationException">If a session already exists for today.</exception>
    Task<DaySession> StartNewDayAsync(decimal initialCash);

    /// <summary>
    /// Close the current day with the counted cash amount.
    /// </summary>
    /// <exception cref="InvalidOperationException">If no open session exists.</exception>
    Task CloseDayAsync(decimal countedCash);

    /// <summary>
    /// Check if transactions can be added (has open session).
    /// </summary>
    Task<bool> CanAddTransactionAsync();

    /// <summary>
    /// Check if today has a closed session.
    /// </summary>
    Task<bool> HasClosedSessionTodayAsync();
}
