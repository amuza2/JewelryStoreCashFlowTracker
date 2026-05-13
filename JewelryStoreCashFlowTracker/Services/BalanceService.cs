using JewelryStoreCashFlowTracker.Models;
using System.Linq;

namespace JewelryStoreCashFlowTracker.Services;

/// <summary>
/// Service implementation for balance calculations.
/// </summary>
public class BalanceService : IBalanceService
{
    public decimal CalculateCurrentBalance(DaySession session)
    {
        if (session?.Transactions == null)
            return session?.InitialCash ?? 0;

        var totalEnter = session.Transactions
            .Where(t => !t.IsDeleted && t.Type == TransactionType.MoneyEnter)
            .Sum(t => t.Amount);

        var totalLeave = session.Transactions
            .Where(t => !t.IsDeleted && t.Type == TransactionType.MoneyLeave)
            .Sum(t => t.Amount);

        return session.InitialCash + totalEnter - totalLeave;
    }

    public decimal CalculateExpectedCash(DaySession session)
    {
        // Same calculation as current balance for open sessions
        return CalculateCurrentBalance(session);
    }

    public (decimal totalEnter, decimal totalLeave) GetDailySummary(DaySession session)
    {
        if (session?.Transactions == null)
            return (0, 0);

        var totalEnter = session.Transactions
            .Where(t => !t.IsDeleted && t.Type == TransactionType.MoneyEnter)
            .Sum(t => t.Amount);

        var totalLeave = session.Transactions
            .Where(t => !t.IsDeleted && t.Type == TransactionType.MoneyLeave)
            .Sum(t => t.Amount);

        return (totalEnter, totalLeave);
    }

    public decimal CalculateDifference(decimal countedCash, decimal expectedCash)
    {
        return countedCash - expectedCash;
    }
}
