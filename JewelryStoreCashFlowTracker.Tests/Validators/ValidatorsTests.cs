using FluentValidation.TestHelper;
using JewelryStoreCashFlowTracker.Models;
using JewelryStoreCashFlowTracker.Validators;
using JewelryStoreCashFlowTracker.ViewModels;
using Xunit;

namespace JewelryStoreCashFlowTracker.Tests.Validators;

public class ValidatorsTests
{
    public class StartDayValidatorTests
    {
        private readonly StartDayValidator _validator = new();

        [Theory]
        [InlineData(0)]
        [InlineData(100)]
        [InlineData(999999.99)]
        public void ValidateInitialCash_ZeroOrPositive_Passes(decimal initialCash)
        {
            // Arrange
            var model = new StartDayDialogViewModel(null!) { InitialCash = initialCash };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.InitialCash);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(-0.01)]
        public void ValidateInitialCash_Negative_Fails(decimal initialCash)
        {
            // Arrange
            var model = new StartDayDialogViewModel(null!) { InitialCash = initialCash };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.InitialCash);
        }
    }

    public class TransactionValidatorTests
    {
        private readonly TransactionValidator _validator = new();

        [Theory]
        [InlineData(0.01)]
        [InlineData(100)]
        [InlineData(999999.99)]
        public void ValidateAmount_Positive_Passes(decimal amount)
        {
            // Arrange
            var model = new AddTransactionDialogViewModel(null!)
            {
                Description = "Test",
                Amount = amount
            };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Amount);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void ValidateAmount_ZeroOrNegative_Fails(decimal amount)
        {
            // Arrange
            var model = new AddTransactionDialogViewModel(null!)
            {
                Description = "Test",
                Amount = amount
            };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Amount);
        }

        [Theory]
        [InlineData("Valid Description")]
        [InlineData("A")]
        [InlineData("Sale of gold ring")]
        public void ValidateDescription_NonEmpty_Passes(string description)
        {
            // Arrange
            var model = new AddTransactionDialogViewModel(null!)
            {
                Description = description,
                Amount = 100
            };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Description);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void ValidateDescription_Empty_Fails(string? description)
        {
            // Arrange
            var model = new AddTransactionDialogViewModel(null!)
            {
                Description = description!,
                Amount = 100
            };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void ValidateDescription_TooLong_Fails()
        {
            // Arrange
            var model = new AddTransactionDialogViewModel(null!)
            {
                Description = new string('A', 201), // 201 characters
                Amount = 100
            };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("Short note")]
        public void ValidateNotes_NullOrShort_Passes(string? notes)
        {
            // Arrange
            var model = new AddTransactionDialogViewModel(null!)
            {
                Description = "Test",
                Amount = 100,
                Notes = notes
            };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Notes);
        }

        [Fact]
        public void ValidateNotes_TooLong_Fails()
        {
            // Arrange
            var model = new AddTransactionDialogViewModel(null!)
            {
                Description = "Test",
                Amount = 100,
                Notes = new string('N', 501) // 501 characters
            };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Notes);
        }
    }

    public class CloseDayValidatorTests
    {
        private readonly CloseDayValidator _validator = new();

        [Theory]
        [InlineData(0)]
        [InlineData(100)]
        [InlineData(999999.99)]
        public void ValidateCountedCash_ZeroOrPositive_Passes(decimal countedCash)
        {
            // Arrange
            var model = new CloseDayDialogViewModel(null!, null!) { CountedCash = countedCash };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.CountedCash);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(-0.01)]
        public void ValidateCountedCash_Negative_Fails(decimal countedCash)
        {
            // Arrange
            var model = new CloseDayDialogViewModel(null!, null!) { CountedCash = countedCash };

            // Act
            var result = _validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CountedCash);
        }
    }
}
