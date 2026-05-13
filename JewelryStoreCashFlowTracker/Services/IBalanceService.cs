using JewelryStoreCashFlowTracker.Models;

namespace JewelryStoreCashFlowTracker.Services;

/// <summary>
/// Service for calculating balances and daily summaries.
/// </summary>
public interface IBalanceService
{
    /// <summary>
    /// Calculate the current balance for an open session.
    /// </summary>
    decimal CalculateCurrentBalance(DaySession session);

    /// <summary>
    /// Calculate the expected cash amount at end of day.
    /// </summary>
    decimal CalculateExpectedCash(DaySession session);

    /// <summary>
    /// Get daily summary metrics.
    /// </summary>
    (decimal totalEnter, decimal totalLeave) GetDailySummary(DaySession session);

    /// <summary>
    /// Calculate the difference between counted and expected cash.
    /// </summary>
    decimal CalculateDifference(decimal countedCash, decimal expectedCash);
}
