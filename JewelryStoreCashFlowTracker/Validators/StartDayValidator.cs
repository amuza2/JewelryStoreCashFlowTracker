using FluentValidation;
using JewelryStoreCashFlowTracker.ViewModels;

namespace JewelryStoreCashFlowTracker.Validators;

/// <summary>
/// Validator for starting a new day session.
/// </summary>
public class StartDayValidator : AbstractValidator<StartDayDialogViewModel>
{
    public StartDayValidator()
    {
        RuleFor(x => x.InitialCash)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Initial cash must be 0 or greater");
    }
}
