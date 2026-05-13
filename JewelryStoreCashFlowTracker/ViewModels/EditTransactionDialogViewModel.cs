using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JewelryStoreCashFlowTracker.Models;
using JewelryStoreCashFlowTracker.Services;
using Serilog;
using System;
using System.Threading.Tasks;

namespace JewelryStoreCashFlowTracker.ViewModels;

/// <summary>
/// ViewModel for editing an existing transaction.
/// </summary>
public partial class EditTransactionDialogViewModel : ViewModelBase
{
    private readonly ITransactionService _transactionService;
    private Transaction? _transaction;

    [ObservableProperty]
    private string _title = "Edit Transaction";

    [ObservableProperty]
    private string _description = "";

    [ObservableProperty]
    private decimal _amount;

    [ObservableProperty]
    private string _errorMessage = "";

    [ObservableProperty]
    private bool _isBusy;

    /// <summary>
    /// Callback when save is successful.
    /// </summary>
    public Action? OnSaveSuccess { get; set; }

    /// <summary>
    /// Callback when dialog is cancelled.
    /// </summary>
    public Action? OnCancel { get; set; }

    public EditTransactionDialogViewModel(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    /// <summary>
    /// Determines if the transaction can be saved.
    /// </summary>
    public bool CanSave => !IsBusy && Amount > 0 && !string.IsNullOrWhiteSpace(Description);

    /// <summary>
    /// Initialize the ViewModel with an existing transaction.
    /// </summary>
    public void Initialize(Transaction transaction)
    {
        _transaction = transaction;
        Description = transaction.Description;
        Amount = transaction.Amount;
        Title = $"Edit Transaction - {transaction.Type}";
        ErrorMessage = "";
        Log.Information("Initialized EditTransactionDialog for transaction {Id}", transaction.Id);
    }

    partial void OnDescriptionChanged(string value) => Validate();
    partial void OnAmountChanged(decimal value) => Validate();

    private void Validate()
    {
        ErrorMessage = "";
        if (string.IsNullOrWhiteSpace(Description))
        {
            ErrorMessage = "Description is required.";
        }
        else if (Amount <= 0)
        {
            ErrorMessage = "Amount must be greater than 0.";
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_transaction == null || !CanSave) return;

        try
        {
            IsBusy = true;
            ErrorMessage = "";

            // Update the transaction
            _transaction.Description = Description.Trim();
            _transaction.Amount = Amount;
            _transaction.UpdatedAt = DateTime.UtcNow;

            await _transactionService.UpdateTransactionAsync(_transaction);

            Log.Information("Transaction updated: {Id} - {Description} - {Amount:C}",
                _transaction.Id, _transaction.Description, _transaction.Amount);

            OnSaveSuccess?.Invoke();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to update transaction: {ex.Message}";
            Log.Error(ex, "Failed to update transaction {Id}", _transaction?.Id);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        Log.Information("Edit transaction cancelled");
        OnCancel?.Invoke();
    }
}
