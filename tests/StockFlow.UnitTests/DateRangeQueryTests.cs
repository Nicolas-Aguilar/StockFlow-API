using System.ComponentModel.DataAnnotations;
using StockFlow.Application.Common;

namespace StockFlow.UnitTests;

public sealed class DateRangeQueryTests
{
    [Fact]
    public void InvalidDateRange_ReturnsValidationError()
    {
        var query = new DateRangeQuery
        {
            From = new DateOnly(2026, 2, 1),
            To = new DateOnly(2026, 1, 1)
        };

        var validationResults = query.Validate(new ValidationContext(query)).ToArray();

        Assert.Single(validationResults);
    }
}
