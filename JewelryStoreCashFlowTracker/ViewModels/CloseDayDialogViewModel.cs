using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JewelryStoreCashFlowTracker.Services;
using Serilog;
using System.Threading.Tasks;

namespace JewelryStoreCashFlowTracker.ViewModels;

/// <summary>
/// ViewModel for closing a day session.
/// </summary>
public partial class CloseDayDialogViewModel : ViewModelBase
{
    private readonly IDaySessionService _daySessionService;
    private readonly IBalanceService _balanceService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanConfirm))]
    private decimal _expectedCash;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Difference))]
    [NotifyPropertyChangedFor(nameof(CanConfirm))]
    private decimal _countedCash;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isBusy;

    public decimal Difference => _balanceService.CalculateDifference(CountedCash, ExpectedCash);

    public bool CanConfirm => !IsBusy && CountedCash >= 0;

    public CloseDayDialogViewModel(
        IDaySessionService daySessionService,
        IBalanceService balanceService)
    {
        _daySessionService = daySessionService;
        _balanceService = balanceService;
    }

    public void SetExpectedCash(decimal expected)
    {
        ExpectedCash = expected;
    }

    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private async Task ConfirmAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            await _daySessionService.CloseDayAsync(CountedCash);

            Log.Information("Day closed. Expected: {Expected:C}, Counted: {Counted:C}, Difference: {Difference:C}",
                ExpectedCash, CountedCash, Difference);
        }
        catch (System.Exception ex)
        {
            ErrorMessage = ex.Message;
            Log.Warning(ex, "Failed to close day");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
