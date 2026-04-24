using System.ComponentModel.DataAnnotations;

namespace StockFlow.Application.Common;

public sealed class DateRangeQuery : IValidatableObject
{
    [Required]
    public DateOnly From { get; init; }

    [Required]
    public DateOnly To { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (From > To)
        {
            yield return new ValidationResult("The from date must be less than or equal to the to date.", new[] { nameof(From), nameof(To) });
        }
    }
}
