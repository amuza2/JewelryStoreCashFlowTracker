using System;

namespace JewelryStoreCashFlowTracker.Models;

/// <summary>
/// Represents a single cash flow transaction.
/// Transactions are always linked to a DaySession.
/// </summary>
public class Transaction
{
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the parent DaySession.
    /// </summary>
    public int DaySessionId { get; set; }

    /// <summary>
    /// Navigation property to the parent DaySession.
    /// </summary>
    public DaySession DaySession { get; set; } = null!;

    /// <summary>
    /// When the transaction was recorded.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the transaction was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Description of the transaction (e.g., "Gold ring sale", "Supplier payment").
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Type of transaction (Sale, Purchase, Expense).
    /// </summary>
    public TransactionType Type { get; set; }

    /// <summary>
    /// The monetary amount (always positive).
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Optional additional notes.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Soft delete flag for audit safety.
    /// </summary>
    public bool IsDeleted { get; set; }
}
