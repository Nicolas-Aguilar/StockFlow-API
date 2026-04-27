using System.ComponentModel.DataAnnotations;
using StockFlow.Application.Common;

namespace StockFlow.UnitTests;

public sealed class PaginationQueryTests
{
    [Fact]
    public void DefaultValues_AreApplied()
    {
        var query = new PaginationQuery();

        Assert.Equal(1, query.Page);
        Assert.Equal(20, query.PageSize);
        Assert.Equal(0, query.Skip);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public void InvalidValues_ReturnValidationErrors(int page, int pageSize)
    {
        var query = new PaginationQuery
        {
            Page = page,
            PageSize = pageSize
        };

        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(query, new ValidationContext(query), validationResults, true);

        Assert.False(isValid);
        Assert.NotEmpty(validationResults);
    }
}
