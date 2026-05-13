using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JewelryStoreCashFlowTracker.Models;
using JewelryStoreCashFlowTracker.Repositories;
using JewelryStoreCashFlowTracker.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace JewelryStoreCashFlowTracker.ViewModels;

/// <summary>
/// ViewModel for browsing day session history.
/// </summary>
public partial class DayHistoryViewModel : ViewModelBase
{
    private readonly IDaySessionRepository _daySessionRepository;
    private readonly IBalanceService _balanceService;

    [ObservableProperty]
    private ObservableCollection<DaySession> _sessions = new();

    [ObservableProperty]
    private DaySession? _selectedSession;

    [ObservableProperty]
    private decimal _selectedSessionBalance;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasSessions;

    public DayHistoryViewModel(
        IDaySessionRepository daySessionRepository,
        IBalanceService balanceService)
    {
        _daySessionRepository = daySessionRepository;
        _balanceService = balanceService;
    }

    public async Task InitializeAsync()
    {
        await LoadSessionsAsync();
    }

    [RelayCommand]
    private async Task LoadSessionsAsync()
    {
        try
        {
            IsLoading = true;
            var sessions = await _daySessionRepository.GetAllSessionsAsync();
            
            Sessions.Clear();
            foreach (var session in sessions.OrderByDescending(s => s.Date))
            {
                Sessions.Add(session);
            }
            HasSessions = Sessions.Count > 0;

            if (SelectedSession != null)
            {
                // Refresh the selected session data
                var updatedSession = sessions.FirstOrDefault(s => s.Id == SelectedSession.Id);
                if (updatedSession != null)
                {
                    SelectedSession = updatedSession;
                    SelectedSessionBalance = _balanceService.CalculateCurrentBalance(updatedSession);
                }
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void SelectSession(DaySession session)
    {
        SelectedSession = session;
        SelectedSessionBalance = _balanceService.CalculateCurrentBalance(session);
    }

    [RelayCommand]
    private void BackToDashboard()
    {
        // This will be handled by MainWindowViewModel to switch views
    }
}
