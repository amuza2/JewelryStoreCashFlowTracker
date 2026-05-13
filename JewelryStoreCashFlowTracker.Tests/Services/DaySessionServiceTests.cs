using JewelryStoreCashFlowTracker.Models;
using JewelryStoreCashFlowTracker.Repositories;
using JewelryStoreCashFlowTracker.Services;
using Moq;
using Xunit;

namespace JewelryStoreCashFlowTracker.Tests.Services;

public class DaySessionServiceTests
{
    private readonly Mock<IDaySessionRepository> _mockRepo;
    private readonly DaySessionService _service;

    public DaySessionServiceTests()
    {
        _mockRepo = new Mock<IDaySessionRepository>();
        _service = new DaySessionService(_mockRepo.Object);
    }

    [Fact]
    public async Task StartNewDayAsync_WithValidInitialCash_CreatesOpenSession()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Now);
        _mockRepo.Setup(r => r.GetByDateAsync(today)).ReturnsAsync((DaySession?)null);
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<DaySession>())).Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _service.StartNewDayAsync(1000m);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(today, result.Date);
        Assert.Equal(1000m, result.InitialCash);
        Assert.False(result.IsClosed);
        _mockRepo.Verify(r => r.AddAsync(It.Is<DaySession>(s => s.InitialCash == 1000m)), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task StartNewDayAsync_WhenSessionAlreadyExists_ThrowsException()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Now);
        var existingSession = new DaySession { Date = today, IsClosed = false };
        _mockRepo.Setup(r => r.GetByDateAsync(today)).ReturnsAsync(existingSession);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.StartNewDayAsync(1000m));
    }

    [Fact]
    public async Task StartNewDayAsync_WithNegativeInitialCash_ThrowsException()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Now);
        _mockRepo.Setup(r => r.GetByDateAsync(today)).ReturnsAsync((DaySession?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.StartNewDayAsync(-100m));
    }

    [Fact]
    public async Task CloseDayAsync_WithCorrectCount_SetsIsClosed()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Now);
        var openSession = new DaySession
        {
            Date = today,
            IsClosed = false,
            InitialCash = 1000m,
            Transactions = new List<Transaction>()
        };
        _mockRepo.Setup(r => r.GetTodayOpenSessionAsync()).ReturnsAsync(openSession);
        _mockRepo.Setup(r => r.CloseSessionAsync(openSession, 1000m)).Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _service.CloseDayAsync(1000m);

        // Assert
        _mockRepo.Verify(r => r.CloseSessionAsync(openSession, 1000m), Times.Once);
        _mockRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CloseDayAsync_WhenNoOpenSession_ThrowsException()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetTodayOpenSessionAsync()).ReturnsAsync((DaySession?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CloseDayAsync(1000m));
    }

    [Fact]
    public async Task CloseDayAsync_WithNegativeCountedCash_ThrowsException()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Now);
        var openSession = new DaySession { Date = today, IsClosed = false };
        _mockRepo.Setup(r => r.GetTodayOpenSessionAsync()).ReturnsAsync(openSession);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CloseDayAsync(-100m));
    }

    [Fact]
    public async Task CanAddTransactionAsync_WithOpenSession_ReturnsTrue()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Now);
        _mockRepo.Setup(r => r.HasOpenSessionForDateAsync(today)).ReturnsAsync(true);

        // Act
        var result = await _service.CanAddTransactionAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CanAddTransactionAsync_WithNoOpenSession_ReturnsFalse()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Now);
        _mockRepo.Setup(r => r.HasOpenSessionForDateAsync(today)).ReturnsAsync(false);

        // Act
        var result = await _service.CanAddTransactionAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasClosedSessionTodayAsync_WithClosedSession_ReturnsTrue()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Now);
        _mockRepo.Setup(r => r.HasClosedSessionForDateAsync(today)).ReturnsAsync(true);

        // Act
        var result = await _service.HasClosedSessionTodayAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasClosedSessionTodayAsync_WithNoClosedSession_ReturnsFalse()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Now);
        _mockRepo.Setup(r => r.HasClosedSessionForDateAsync(today)).ReturnsAsync(false);

        // Act
        var result = await _service.HasClosedSessionTodayAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetTodaySessionAsync_ReturnsSession()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Now);
        var expectedSession = new DaySession { Date = today, IsClosed = false };
        _mockRepo.Setup(r => r.GetByDateAsync(today)).ReturnsAsync(expectedSession);

        // Act
        var result = await _service.GetTodaySessionAsync();

        // Assert
        Assert.Equal(expectedSession, result);
    }

    [Fact]
    public async Task GetTodaySessionAsync_WhenNoSession_ReturnsNull()
    {
        // Arrange
        var today = DateOnly.FromDateTime(DateTime.Now);
        _mockRepo.Setup(r => r.GetByDateAsync(today)).ReturnsAsync((DaySession?)null);

        // Act
        var result = await _service.GetTodaySessionAsync();

        // Assert
        Assert.Null(result);
    }
}
