using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JewelryStoreCashFlowTracker.Models;
using JewelryStoreCashFlowTracker.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace JewelryStoreCashFlowTracker.ViewModels;

/// <summary>
/// ViewModel for the main dashboard displaying daily metrics.
/// </summary>
public partial class DashboardViewModel : ViewModelBase
{
    private readonly IDaySessionService _daySessionService;
    private readonly ITransactionService _transactionService;
    private readonly IBalanceService _balanceService;

    [ObservableProperty]
    private DaySession? _currentSession;

    [ObservableProperty]
    private decimal _initialCash;

    [ObservableProperty]
    private decimal _totalMoneyEnter;

    [ObservableProperty]
    private decimal _totalMoneyLeave;

    [ObservableProperty]
    private decimal _currentBalance;

    [ObservableProperty]
    private bool _isDayOpen;

    [ObservableProperty]
    private bool _isDayClosed;

    [ObservableProperty]
    private bool _isLoading;

    /// <summary>
    /// Total sold (net): Total Money Enter - Total Money Leave
    /// </summary>
    [ObservableProperty]
    private decimal _totalSold;

    /// <summary>
    /// Event raised when user requests to edit initial cash.
    /// MainWindowViewModel subscribes to this.
    /// </summary>
    public event EventHandler? EditInitialCashRequested;

    /// <summary>
    /// Event raised when user requests to add Money Enter transaction.
    /// MainWindowViewModel subscribes to this.
    /// </summary>
    public event EventHandler? AddMoneyEnterRequested;

    /// <summary>
    /// Event raised when user requests to add Money Leave transaction.
    /// MainWindowViewModel subscribes to this.
    /// </summary>
    public event EventHandler? AddMoneyLeaveRequested;

    /// <summary>
    /// Event raised when user requests to close the day.
    /// MainWindowViewModel subscribes to this.
    /// </summary>
    public event EventHandler? CloseDayRequested;

    /// <summary>
    /// Event raised when user requests to edit a transaction.
    /// MainWindowViewModel subscribes to this.
    /// </summary>
    public event EventHandler<TransactionRowViewModel>? EditTransactionRequested;

    [ObservableProperty]
    private ObservableCollection<TransactionRowViewModel> _transactionRows = new();

    [ObservableProperty]
    private TransactionRowViewModel? _selectedTransaction;

    public DashboardViewModel(
        IDaySessionService daySessionService,
        ITransactionService transactionService,
        IBalanceService balanceService)
    {
        _daySessionService = daySessionService;
        _transactionService = transactionService;
        _balanceService = balanceService;
    }

    public async Task InitializeAsync()
    {
        await RefreshDashboardAsync();
    }

    [RelayCommand]
    private async Task RefreshDashboardAsync()
    {
        Log.Information("=== REFRESH DASHBOARD STARTED ===");
        try
        {
            IsLoading = true;

            CurrentSession = await _daySessionService.GetTodaySessionAsync();
            Log.Information("Current session: {SessionId}, Date: {Date}", 
                CurrentSession?.Id ?? 0, CurrentSession?.Date.ToString() ?? "none");
            IsDayOpen = CurrentSession != null && !CurrentSession.IsClosed;
            IsDayClosed = CurrentSession?.IsClosed ?? false;

            if (CurrentSession != null)
            {
                InitialCash = CurrentSession.InitialCash;

                var summary = _balanceService.GetDailySummary(CurrentSession);
                TotalMoneyEnter = summary.totalEnter;
                TotalMoneyLeave = summary.totalLeave;
                TotalSold = TotalMoneyEnter - TotalMoneyLeave;

                CurrentBalance = _balanceService.CalculateCurrentBalance(CurrentSession);

                var transactions = await _transactionService.GetTodayTransactionsAsync();
                Log.Information("Loaded {Count} transactions from database", transactions.Count);
                
                TransactionRows.Clear();
                foreach (var transaction in transactions)
                {
                    Log.Debug("Adding transaction: {Desc} - {Type} - {Amount:C}", 
                        transaction.Description, transaction.Type, transaction.Amount);
                    TransactionRows.Add(new TransactionRowViewModel(transaction));
                }
                
                Log.Information("TransactionRows now has {Count} items", TransactionRows.Count);
            }
            else
            {
                InitialCash = 0;
                TotalMoneyEnter = 0;
                TotalMoneyLeave = 0;
                TotalSold = 0;
                CurrentBalance = 0;
                TransactionRows.Clear();
                Log.Information("No active session - cleared TransactionRows");
            }

            Log.Debug("Dashboard refreshed. Balance: {Balance:C}, Transactions: {Count}, TotalSold: {Sold:C}",
                CurrentBalance, TransactionRows.Count, TotalSold);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void AddMoneyEnter()
    {
        Log.Debug("AddMoneyEnter requested - raising event");
        AddMoneyEnterRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void AddMoneyLeave()
    {
        Log.Debug("AddMoneyLeave requested - raising event");
        AddMoneyLeaveRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void CloseDay()
    {
        Log.Debug("CloseDay requested - raising event");
        CloseDayRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void EditInitialCash()
    {
        Log.Debug("EditInitialCash requested - raising event");
        EditInitialCashRequested?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void EditTransaction()
    {
        if (SelectedTransaction != null)
        {
            Log.Debug("EditTransaction requested - raising event for transaction {Id}", SelectedTransaction.Id);
            EditTransactionRequested?.Invoke(this, SelectedTransaction);
        }
    }
}
