using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JewelryStoreCashFlowTracker.Models;
using JewelryStoreCashFlowTracker.Repositories;
using JewelryStoreCashFlowTracker.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace JewelryStoreCashFlowTracker.ViewModels;

/// <summary>
/// ViewModel for editing the initial cash amount.
/// </summary>
public partial class EditInitialCashDialogViewModel : ViewModelBase
{
    private readonly IDaySessionRepository _daySessionRepository;
    private DaySession? _currentSession;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private decimal _initialCash;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isBusy;

    public bool CanSave => !IsBusy && InitialCash >= 0 && _currentSession != null;

    /// <summary>
    /// Callback action when save is successful. Set by MainWindowViewModel.
    /// </summary>
    public Action? OnSaveSuccess { get; set; }

    /// <summary>
    /// Callback action when cancelled. Set by MainWindowViewModel.
    /// </summary>
    public Action? OnCancel { get; set; }

    public EditInitialCashDialogViewModel(IDaySessionRepository daySessionRepository)
    {
        _daySessionRepository = daySessionRepository;
    }

    public void Initialize(DaySession session)
    {
        _currentSession = session;
        InitialCash = session.InitialCash;
        ErrorMessage = null;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy || _currentSession == null) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            // Check if there are any transactions
            if (_currentSession.Transactions?.Any(t => !t.IsDeleted) == true)
            {
                ErrorMessage = "Cannot edit initial cash after transactions have been added.";
                return;
            }

            _currentSession.InitialCash = InitialCash;
            await _daySessionRepository.UpdateAsync(_currentSession);
            await _daySessionRepository.SaveChangesAsync();

            // Signal success
            OnSaveSuccess?.Invoke();
        }
        catch (System.Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        OnCancel?.Invoke();
    }
}
