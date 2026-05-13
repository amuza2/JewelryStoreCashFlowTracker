namespace JewelryStoreCashFlowTracker.Models;

/// <summary>
/// Represents the type of cash flow transaction.
/// MoneyEnter: Cash entering the store (Green)
/// MoneyLeave: Cash leaving the store (Red)
/// </summary>
public enum TransactionType
{
    MoneyEnter,
    MoneyLeave
}
