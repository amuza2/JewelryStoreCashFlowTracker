using JewelryStoreCashFlowTracker.Models;
using JewelryStoreCashFlowTracker.Services;
using Xunit;

namespace JewelryStoreCashFlowTracker.Tests.Services;

public class BalanceServiceTests
{
    private readonly BalanceService _service = new();

    [Fact]
    public void CalculateCurrentBalance_WithOnlyInitialCash_ReturnsInitialCash()
    {
        // Arrange
        var session = new DaySession
        {
            InitialCash = 1000m,
            Transactions = new List<Transaction>()
        };

        // Act
        var result = _service.CalculateCurrentBalance(session);

        // Assert
        Assert.Equal(1000m, result);
    }

    [Fact]
    public void CalculateCurrentBalance_WithMoneyEnter_IncreasesBalance()
    {
        // Arrange
        var session = new DaySession
        {
            InitialCash = 1000m,
            Transactions = new List<Transaction>
            {
                new() { Type = TransactionType.MoneyEnter, Amount = 500m, IsDeleted = false },
                new() { Type = TransactionType.MoneyEnter, Amount = 300m, IsDeleted = false }
            }
        };

        // Act
        var result = _service.CalculateCurrentBalance(session);

        // Assert
        Assert.Equal(1800m, result); // 1000 + 500 + 300
    }

    [Fact]
    public void CalculateCurrentBalance_WithMoneyLeave_DecreasesBalance()
    {
        // Arrange
        var session = new DaySession
        {
            InitialCash = 1000m,
            Transactions = new List<Transaction>
            {
                new() { Type = TransactionType.MoneyLeave, Amount = 200m, IsDeleted = false },
                new() { Type = TransactionType.MoneyLeave, Amount = 100m, IsDeleted = false }
            }
        };

        // Act
        var result = _service.CalculateCurrentBalance(session);

        // Assert
        Assert.Equal(700m, result); // 1000 - 200 - 100
    }

    [Fact]
    public void CalculateCurrentBalance_WithMixedTransactions_CalculatesCorrectly()
    {
        // Arrange
        var session = new DaySession
        {
            InitialCash = 1000m,
            Transactions = new List<Transaction>
            {
                new() { Type = TransactionType.MoneyEnter, Amount = 500m, IsDeleted = false },
                new() { Type = TransactionType.MoneyLeave, Amount = 200m, IsDeleted = false },
                new() { Type = TransactionType.MoneyLeave, Amount = 50m, IsDeleted = false },
                new() { Type = TransactionType.MoneyEnter, Amount = 300m, IsDeleted = false }
            }
        };

        // Act
        var result = _service.CalculateCurrentBalance(session);

        // Assert
        Assert.Equal(1550m, result); // 1000 + 500 - 200 - 50 + 300
    }

    [Fact]
    public void CalculateCurrentBalance_WithDeletedTransactions_IgnoresDeleted()
    {
        // Arrange
        var session = new DaySession
        {
            InitialCash = 1000m,
            Transactions = new List<Transaction>
            {
                new() { Type = TransactionType.MoneyEnter, Amount = 500m, IsDeleted = false },
                new() { Type = TransactionType.MoneyEnter, Amount = 300m, IsDeleted = true }, // Deleted
                new() { Type = TransactionType.MoneyLeave, Amount = 200m, IsDeleted = false }
            }
        };

        // Act
        var result = _service.CalculateCurrentBalance(session);

        // Assert
        Assert.Equal(1300m, result); // 1000 + 500 - 200 (deleted 300 ignored)
    }

    [Fact]
    public void GetDailySummary_ReturnsCorrectTotals()
    {
        // Arrange
        var session = new DaySession
        {
            InitialCash = 1000m,
            Transactions = new List<Transaction>
            {
                new() { Type = TransactionType.MoneyEnter, Amount = 500m, IsDeleted = false },
                new() { Type = TransactionType.MoneyEnter, Amount = 300m, IsDeleted = false },
                new() { Type = TransactionType.MoneyLeave, Amount = 200m, IsDeleted = false },
                new() { Type = TransactionType.MoneyLeave, Amount = 50m, IsDeleted = false },
                new() { Type = TransactionType.MoneyLeave, Amount = 25m, IsDeleted = true } // Deleted
            }
        };

        // Act
        var (totalEnter, totalLeave) = _service.GetDailySummary(session);

        // Assert
        Assert.Equal(800m, totalEnter);   // 500 + 300
        Assert.Equal(250m, totalLeave);   // 200 + 50 (deleted 25 ignored)
    }

    [Fact]
    public void CalculateDifference_ReturnsCorrectValue()
    {
        // Arrange
        decimal countedCash = 1100m;
        decimal expectedCash = 1000m;

        // Act
        var result = _service.CalculateDifference(countedCash, expectedCash);

        // Assert
        Assert.Equal(100m, result); // 1100 - 1000 = 100 (surplus)
    }

    [Fact]
    public void CalculateDifference_WithShortage_ReturnsNegative()
    {
        // Arrange
        decimal countedCash = 900m;
        decimal expectedCash = 1000m;

        // Act
        var result = _service.CalculateDifference(countedCash, expectedCash);

        // Assert
        Assert.Equal(-100m, result); // 900 - 1000 = -100 (shortage)
    }

    [Fact]
    public void CalculateExpectedCash_SameAsCurrentBalanceForOpenSession()
    {
        // Arrange
        var session = new DaySession
        {
            InitialCash = 1000m,
            Transactions = new List<Transaction>
            {
                new() { Type = TransactionType.MoneyEnter, Amount = 500m, IsDeleted = false }
            }
        };

        // Act
        var expected = _service.CalculateExpectedCash(session);
        var current = _service.CalculateCurrentBalance(session);

        // Assert
        Assert.Equal(current, expected);
    }
}
