using JewelryStoreCashFlowTracker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JewelryStoreCashFlowTracker.Repositories;

/// <summary>
/// Repository interface for Transaction entity operations.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// Add a new transaction.
    /// </summary>
    Task AddAsync(Transaction transaction);

    /// <summary>
    /// Get all transactions for a specific day session.
    /// </summary>
    Task<List<Transaction>> GetBySessionIdAsync(int sessionId);

    /// <summary>
    /// Get a transaction by its ID.
    /// </summary>
    Task<Transaction?> GetByIdAsync(int id);

    /// <summary>
    /// Update an existing transaction.
    /// </summary>
    Task UpdateAsync(Transaction transaction);

    /// <summary>
    /// Soft delete a transaction.
    /// </summary>
    Task SoftDeleteAsync(int id);

    /// <summary>
    /// Save changes to the database.
    /// </summary>
    Task SaveChangesAsync();
}
