using System;
using System.Collections.Generic;

namespace JewelryStoreCashFlowTracker.Models;

/// <summary>
/// Represents a single business day session.
/// Each day has one session that tracks the opening cash, all transactions,
/// and the closing verification.
/// </summary>
public class DaySession
{
    public int Id { get; set; }

    /// <summary>
    /// The date of this business day.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// The cash amount at the start of the day.
    /// </summary>
    public decimal InitialCash { get; set; }

    /// <summary>
    /// Whether this day has been closed and verified.
    /// </summary>
    public bool IsClosed { get; set; }

    /// <summary>
    /// The physically counted cash at end of day (set when closing).
    /// </summary>
    public decimal? CountedCash { get; set; }

    /// <summary>
    /// The difference between counted cash and expected cash (set when closing).
    /// Positive = surplus, Negative = shortage.
    /// </summary>
    public decimal? Difference { get; set; }

    /// <summary>
    /// When the day session was opened.
    /// </summary>
    public DateTime OpenedAt { get; set; }

    /// <summary>
    /// When the day session was closed (null if still open).
    /// </summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>
    /// All transactions for this day.
    /// </summary>
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
