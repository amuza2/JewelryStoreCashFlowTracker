using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JewelryStoreCashFlowTracker.Models;
using JewelryStoreCashFlowTracker.Services;
using JewelryStoreCashFlowTracker.Views;
using Avalonia.Controls;
using Serilog;
using System.Threading.Tasks;

namespace JewelryStoreCashFlowTracker.ViewModels;

/// <summary>
/// Main window view model - coordinates the application startup flow.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IDaySessionService _daySessionService;
    private readonly StartDayDialogViewModel _startDayViewModel;
    private readonly EditInitialCashDialogViewModel _editInitialCashViewModel;
    private readonly AddTransactionDialogViewModel _addTransactionViewModel;
    private readonly EditTransactionDialogViewModel _editTransactionViewModel;
    private Control _startDayView;
    private Control _dashboardView;
    private Control _editInitialCashView;
    private Control _addTransactionView;
    private Control _editTransactionView;

    [ObservableProperty]
    private Control _currentView;

    /// <summary>
    /// Exposed for button command bindings in MainWindow.
    /// </summary>
    public DashboardViewModel DashboardViewModel { get; }

    [ObservableProperty]
    private string _title = "Jewelry Cash Flow Tracker";

    [ObservableProperty]
    private bool _isStartDayVisible;

    [ObservableProperty]
    private bool _isDashboardVisible;


    public MainWindowViewModel(
        IDaySessionService daySessionService,
        DashboardViewModel dashboardViewModel,
        StartDayDialogViewModel startDayViewModel,
        EditInitialCashDialogViewModel editInitialCashViewModel,
        AddTransactionDialogViewModel addTransactionViewModel,
        EditTransactionDialogViewModel editTransactionViewModel)
    {
        _daySessionService = daySessionService;
        DashboardViewModel = dashboardViewModel;
        _startDayViewModel = startDayViewModel;
        _editInitialCashViewModel = editInitialCashViewModel;
        _addTransactionViewModel = addTransactionViewModel;
        _editTransactionViewModel = editTransactionViewModel;

        // Subscribe to StartDayDialog events
        _startDayViewModel.OnSaveSuccess = async () => await StartDayCompleteAsync();

        // Subscribe to DashboardViewModel events
        DashboardViewModel.EditInitialCashRequested += async (s, e) => await ShowEditInitialCashAsync();
        DashboardViewModel.AddMoneyEnterRequested += (s, e) => ShowAddTransaction(TransactionType.MoneyEnter);
        DashboardViewModel.AddMoneyLeaveRequested += (s, e) => ShowAddTransaction(TransactionType.MoneyLeave);
        DashboardViewModel.CloseDayRequested += async (s, e) => await ShowCloseDayAsync();
        DashboardViewModel.EditTransactionRequested += (s, e) => ShowEditTransaction(e);

        // Create views explicitly
        var startDayView = new StartDayDialog { DataContext = _startDayViewModel };
        var dashboardView = new DashboardView { DataContext = DashboardViewModel };
        var editInitialCashView = new EditInitialCashDialog { DataContext = _editInitialCashViewModel };
        var addTransactionView = new AddTransactionDialog { DataContext = _addTransactionViewModel };
        var editTransactionView = new EditTransactionDialog { DataContext = _editTransactionViewModel };

        // Store views for later use
        _startDayView = startDayView;
        _dashboardView = dashboardView;
        _editInitialCashView = editInitialCashView;
        _addTransactionView = addTransactionView;
        _editTransactionView = editTransactionView;

        // Set initial view
        _currentView = startDayView;

        _ = InitializeAsync(startDayView, dashboardView);
    }

    private async Task InitializeAsync(Control startDayView, Control dashboardView)
    {
        var session = await _daySessionService.GetTodaySessionAsync();

        if (session == null)
        {
            // No session - show start day dialog
            CurrentView = startDayView;
            IsStartDayVisible = true;
            IsDashboardVisible = false;
            Log.Information("No session for today - showing StartDay dialog");
        }
        else if (session.IsClosed)
        {
            // Day is closed - could show report or allow starting next day
            // For now, show dashboard with closed state
            await DashboardViewModel.InitializeAsync();
            CurrentView = dashboardView;
            IsStartDayVisible = false;
            IsDashboardVisible = true;
            Log.Information("Day is closed - showing dashboard");
        }
        else
        {
            // Open session - show dashboard
            await DashboardViewModel.InitializeAsync();
            CurrentView = dashboardView;
            IsStartDayVisible = false;
            IsDashboardVisible = true;
            Log.Information("Open session found - showing dashboard");
        }
    }

    [RelayCommand]
    private async Task StartDayCompleteAsync()
    {
        // Called after day is started - switch to dashboard
        await DashboardViewModel.InitializeAsync();
        CurrentView = _dashboardView;
        IsStartDayVisible = false;
        IsDashboardVisible = true;
    }

    private async Task BackToDashboardAsync()
    {
        await DashboardViewModel.InitializeAsync();
        CurrentView = _dashboardView;
        IsStartDayVisible = false;
        IsDashboardVisible = true;
        Log.Information("Back to dashboard");
    }

    [RelayCommand]
    private async Task ShowEditInitialCashAsync()
    {
        var session = await _daySessionService.GetTodaySessionAsync();
        if (session == null)
        {
            Log.Warning("Cannot edit initial cash - no active session");
            return;
        }

        // Set up callbacks
        _editInitialCashViewModel.OnSaveSuccess = async () =>
        {
            await EditInitialCashCompleteAsync();
        };
        _editInitialCashViewModel.OnCancel = async () =>
        {
            await BackToDashboardAsync();
        };

        _editInitialCashViewModel.Initialize(session);
        CurrentView = _editInitialCashView;
        IsStartDayVisible = false;
        IsDashboardVisible = false;
        Log.Information("Showing edit initial cash dialog");
    }

    [RelayCommand]
    private async Task EditInitialCashCompleteAsync()
    {
        // After editing initial cash, refresh dashboard and show it
        await DashboardViewModel.InitializeAsync();
        CurrentView = _dashboardView;
        IsStartDayVisible = false;
        IsDashboardVisible = true;
        Log.Information("Edit initial cash completed");
    }

    private void ShowAddTransaction(TransactionType type)
    {
        // Set the transaction type and show dialog
        _addTransactionViewModel.Type = type;

        // Set up callbacks
        _addTransactionViewModel.OnSaveSuccess = async () =>
        {
            await AddTransactionCompleteAsync();
        };
        _addTransactionViewModel.OnCancel = async () =>
        {
            await BackToDashboardAsync();
        };

        CurrentView = _addTransactionView;
        IsStartDayVisible = false;
        IsDashboardVisible = false;
        Log.Information("Showing add transaction dialog with type: {Type}", type);
    }

    private async Task ShowCloseDayAsync()
    {
        // Check if day is already closed
        var session = await _daySessionService.GetTodaySessionAsync();
        if (session == null || session.IsClosed)
        {
            Log.Warning("Cannot close day - no active session or already closed");
            return;
        }

        // Set up callbacks for CloseDayDialog if needed
        // For now, just switch back to dashboard after
        Log.Information("Showing close day dialog");
    }

    private async Task AddTransactionCompleteAsync()
    {
        // After adding transaction, refresh dashboard and show it
        await DashboardViewModel.InitializeAsync();
        CurrentView = _dashboardView;
        IsStartDayVisible = false;
        IsDashboardVisible = true;
        Log.Information("Add transaction completed");
    }

    private void ShowEditTransaction(TransactionRowViewModel transactionRow)
    {
        if (transactionRow == null) return;

        // Get the actual transaction from the row
        var transaction = transactionRow.Transaction;
        if (transaction == null) return;

        // Initialize the view model with the transaction
        _editTransactionViewModel.Initialize(transaction);

        // Set up callbacks
        _editTransactionViewModel.OnSaveSuccess = async () =>
        {
            await EditTransactionCompleteAsync();
        };
        _editTransactionViewModel.OnCancel = async () =>
        {
            await BackToDashboardAsync();
        };

        CurrentView = _editTransactionView;
        IsStartDayVisible = false;
        IsDashboardVisible = false;
        Log.Information("Showing edit transaction dialog for transaction {Id}", transaction.Id);
    }

    private async Task EditTransactionCompleteAsync()
    {
        // After editing transaction, refresh dashboard and show it
        await DashboardViewModel.InitializeAsync();
        CurrentView = _dashboardView;
        IsStartDayVisible = false;
        IsDashboardVisible = true;
        Log.Information("Edit transaction completed");
    }
}