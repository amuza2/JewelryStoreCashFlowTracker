using JewelryStoreCashFlowTracker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JewelryStoreCashFlowTracker.Services;

/// <summary>
/// Service for managing transactions.
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// Add a new transaction to today's open session.
    /// </summary>
    /// <exception cref="InvalidOperationException">If no open session exists.</exception>
    Task<Transaction> AddTransactionAsync(string description, TransactionType type, decimal amount, string? notes);

    /// <summary>
    /// Get all transactions for today.
    /// </summary>
    Task<List<Transaction>> GetTodayTransactionsAsync();

    /// <summary>
    /// Get transactions for a specific session.
    /// </summary>
    Task<List<Transaction>> GetTransactionsBySessionIdAsync(int sessionId);

    /// <summary>
    /// Soft delete a transaction.
    /// </summary>
    Task SoftDeleteTransactionAsync(int transactionId);

    /// <summary>
    /// Update an existing transaction.
    /// </summary>
    Task UpdateTransactionAsync(Transaction transaction);
}
