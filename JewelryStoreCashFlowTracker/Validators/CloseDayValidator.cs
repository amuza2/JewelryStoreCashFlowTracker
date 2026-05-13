using FluentValidation;
using JewelryStoreCashFlowTracker.ViewModels;

namespace JewelryStoreCashFlowTracker.Validators;

/// <summary>
/// Validator for closing a day session.
/// </summary>
public class CloseDayValidator : AbstractValidator<CloseDayDialogViewModel>
{
    public CloseDayValidator()
    {
        RuleFor(x => x.CountedCash)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Counted cash must be 0 or greater");
    }
}
