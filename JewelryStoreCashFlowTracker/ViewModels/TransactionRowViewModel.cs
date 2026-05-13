using JewelryStoreCashFlowTracker.Models;
using System;

namespace JewelryStoreCashFlowTracker.ViewModels;

/// <summary>
/// ViewModel for a transaction row in the dashboard grid.
/// Shows Amount Entered, Amount Left, and Sold (net) columns.
/// </summary>
public class TransactionRowViewModel
{
    private readonly Transaction _transaction;

    public TransactionRowViewModel(Transaction transaction)
    {
        _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
    }

    public int Id => _transaction.Id;

    public DateTime CreatedAt => _transaction.CreatedAt;

    public string Description => _transaction.Description;

    /// <summary>
    /// Amount for MoneyEnter transactions, 0 for MoneyLeave.
    /// </summary>
    public decimal AmountEntered => _transaction.Type == TransactionType.MoneyEnter ? _transaction.Amount : 0;

    /// <summary>
    /// Amount for MoneyLeave transactions, 0 for MoneyEnter.
    /// </summary>
    public decimal AmountLeft => _transaction.Type == TransactionType.MoneyLeave ? _transaction.Amount : 0;

    /// <summary>
    /// Net amount: MoneyEnter adds positive, MoneyLeave subtracts negative.
    /// </summary>
    public decimal Sold => _transaction.Type == TransactionType.MoneyEnter ? _transaction.Amount : -_transaction.Amount;

    public TransactionType Type => _transaction.Type;

    public decimal OriginalAmount => _transaction.Amount;

    /// <summary>
    /// The underlying transaction entity for editing.
    /// </summary>
    public Transaction Transaction => _transaction;
}
