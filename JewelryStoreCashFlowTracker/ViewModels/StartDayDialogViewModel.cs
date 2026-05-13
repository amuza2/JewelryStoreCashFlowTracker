using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JewelryStoreCashFlowTracker.Services;
using Serilog;
using System.Threading.Tasks;

namespace JewelryStoreCashFlowTracker.ViewModels;

/// <summary>
/// ViewModel for the Start Day dialog.
/// </summary>
public partial class StartDayDialogViewModel : ViewModelBase
{
    private readonly IDaySessionService _daySessionService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private decimal _initialCash;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isBusy;

    public bool CanSave => !IsBusy && InitialCash >= 0;

    public System.Action? OnSaveSuccess { get; set; }

    public StartDayDialogViewModel(IDaySessionService daySessionService)
    {
        _daySessionService = daySessionService;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            await _daySessionService.StartNewDayAsync(InitialCash);
            Log.Information("Day started with initial cash: {InitialCash:C}", InitialCash);
            OnSaveSuccess?.Invoke();
        }
        catch (System.Exception ex)
        {
            ErrorMessage = ex.Message;
            Log.Warning(ex, "Failed to start day");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
