using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JewelryStoreCashFlowTracker.Models;
using JewelryStoreCashFlowTracker.Services;
using Serilog;
using System.Threading.Tasks;

namespace JewelryStoreCashFlowTracker.ViewModels;

/// <summary>
/// ViewModel for adding a transaction (MoneyEnter, MoneyLeave).
/// </summary>
public partial class AddTransactionDialogViewModel : ViewModelBase
{
    private readonly ITransactionService _transactionService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    [NotifyPropertyChangedFor(nameof(Title))]
    private TransactionType _type = TransactionType.MoneyEnter;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private string _description = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSave))]
    private decimal _amount;

    [ObservableProperty]
    private string? _notes;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isBusy;

    public bool CanSave => !IsBusy && Amount > 0 && !string.IsNullOrWhiteSpace(Description);

    /// <summary>
    /// Title shown in dialog header based on transaction type.
    /// </summary>
    public string Title => Type == TransactionType.MoneyEnter ? "Add Money Enter" : "Add Money Leave";

    /// <summary>
    /// Callback action when save is successful. Set by MainWindowViewModel.
    /// </summary>
    public System.Action? OnSaveSuccess { get; set; }

    /// <summary>
    /// Callback action when cancelled. Set by MainWindowViewModel.
    /// </summary>
    public System.Action? OnCancel { get; set; }

    public AddTransactionDialogViewModel(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy || !CanSave) return;

        try
        {
            IsBusy = true;
            ErrorMessage = null;

            await _transactionService.AddTransactionAsync(
                Description.Trim(),
                Type,
                Amount,
                Notes?.Trim());

            Log.Information("Transaction saved: {Type} - {Description} - {Amount:C}", Type, Description, Amount);

            // Signal success
            OnSaveSuccess?.Invoke();
        }
        catch (System.Exception ex)
        {
            ErrorMessage = ex.Message;
            Log.Warning(ex, "Failed to add transaction");
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
