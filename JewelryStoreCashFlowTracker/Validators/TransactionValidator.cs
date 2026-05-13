using FluentValidation;
using JewelryStoreCashFlowTracker.ViewModels;

namespace JewelryStoreCashFlowTracker.Validators;

/// <summary>
/// Validator for adding a transaction.
/// </summary>
public class TransactionValidator : AbstractValidator<AddTransactionDialogViewModel>
{
    public TransactionValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(200)
            .WithMessage("Description cannot exceed 200 characters");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
