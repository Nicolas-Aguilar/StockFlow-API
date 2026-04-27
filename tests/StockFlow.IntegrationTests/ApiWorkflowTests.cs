using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using StockFlow.Application.Common;
using StockFlow.Application.DTOs.Auth;
using StockFlow.Application.DTOs.Categories;
using StockFlow.Application.DTOs.Inventory;
using StockFlow.Application.DTOs.Products;
using StockFlow.Application.DTOs.Sales;

namespace StockFlow.IntegrationTests;

public sealed class ApiWorkflowTests : IClassFixture<StockFlowApiFactory>, IAsyncLifetime
{
    private readonly StockFlowApiFactory _factory;
    private HttpClient _client = null!;

    public ApiWorkflowTests(StockFlowApiFactory factory)
    {
        _factory = factory;
    }

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        _client = _factory.CreateClient();
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task PrivateEndpoint_RejectsAnonymousRequests()
    {
        var response = await _client.GetAsync("/api/businesses/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_ThenMe_ReturnsUserAndBusinessWithoutPasswordHash()
    {
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FullName = "Owner One",
            Email = "owner1@example.com",
            Password = "Password123!",
            BusinessName = "Owner One Store",
            BusinessDescription = "Main branch"
        });

        registerResponse.EnsureSuccessStatusCode();
        var auth = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(auth);
        Assert.False(string.IsNullOrWhiteSpace(auth!.Token));

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        var meResponse = await _client.GetAsync("/api/auth/me");
        meResponse.EnsureSuccessStatusCode();
        var payload = await meResponse.Content.ReadAsStringAsync();

        Assert.Contains("owner1@example.com", payload);
        Assert.DoesNotContain("PasswordHash", payload, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SaleCreation_DiscountsStockAndCreatesInventoryHistory()
    {
        await RegisterAndAuthenticateAsync("sales@example.com", "Sales Store");

        var categoryResponse = await _client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest
        {
            Name = "Medicine",
            Description = "Shelf A"
        });
        categoryResponse.EnsureSuccessStatusCode();
        var category = await categoryResponse.Content.ReadFromJsonAsync<CategoryResponse>();

        var productResponse = await _client.PostAsJsonAsync("/api/products", new CreateProductRequest
        {
            CategoryId = category!.Id,
            Name = "Paracetamol",
            InternalCode = "PARA-001",
            PurchasePrice = 2m,
            SalePrice = 3.5m,
            CurrentStock = 10,
            MinimumStock = 4,
            ExpirationDate = DateTime.UtcNow.Date.AddDays(20)
        });
        productResponse.EnsureSuccessStatusCode();
        var product = await productResponse.Content.ReadFromJsonAsync<ProductResponse>();

        var saleResponse = await _client.PostAsJsonAsync("/api/sales", new CreateSaleRequest
        {
            PaymentMethod = StockFlow.Domain.Enums.PaymentMethod.Cash,
            Items = new[]
            {
                new CreateSaleItemRequest
                {
                    ProductId = product!.Id,
                    Quantity = 3
                }
            }
        });

        saleResponse.EnsureSuccessStatusCode();
        var updatedProduct = await _client.GetFromJsonAsync<ProductResponse>($"/api/products/{product!.Id}");
        Assert.Equal(7, updatedProduct!.CurrentStock);

        var history = await _client.GetFromJsonAsync<PagedResponse<InventoryMovementResponse>>($"/api/inventory/products/{product.Id}/history");
        Assert.NotNull(history);
        Assert.Contains(history!.Items, movement => movement.MovementType == StockFlow.Domain.Enums.InventoryMovementType.Sale && movement.Quantity == 3);
    }

    [Fact]
    public async Task ProductQuery_IsIsolatedByBusinessId()
    {
        await RegisterAndAuthenticateAsync("tenant-a@example.com", "Tenant A");
        var categoryResponse = await _client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest { Name = "Food" });
        var category = await categoryResponse.Content.ReadFromJsonAsync<CategoryResponse>();
        var productResponse = await _client.PostAsJsonAsync("/api/products", new CreateProductRequest
        {
            CategoryId = category!.Id,
            Name = "Rice",
            InternalCode = "RICE-001",
            PurchasePrice = 1m,
            SalePrice = 2m,
            CurrentStock = 5,
            MinimumStock = 1
        });
        var product = await productResponse.Content.ReadFromJsonAsync<ProductResponse>();

        await RegisterAndAuthenticateAsync("tenant-b@example.com", "Tenant B");
        var response = await _client.GetAsync($"/api/products/{product!.Id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SaleCreation_WithInsufficientStock_DoesNotPersistSaleOrMovement()
    {
        await RegisterAndAuthenticateAsync($"stock-check-{Guid.NewGuid():N}@example.com", "Stock Check Store");
        var category = await CreateCategoryAsync("Supplements");
        var product = await CreateProductAsync(category.Id, "Vitamin C", "VIT-C", 2);

        var saleResponse = await _client.PostAsJsonAsync("/api/sales", new CreateSaleRequest
        {
            PaymentMethod = StockFlow.Domain.Enums.PaymentMethod.Cash,
            Items = new[]
            {
                new CreateSaleItemRequest
                {
                    ProductId = product.Id,
                    Quantity = 3
                }
            }
        });

        Assert.Equal(HttpStatusCode.Conflict, saleResponse.StatusCode);

        var updatedProduct = await _client.GetFromJsonAsync<ProductResponse>($"/api/products/{product.Id}");
        var sales = await _client.GetFromJsonAsync<PagedResponse<SaleResponse>>("/api/sales");
        var history = await _client.GetFromJsonAsync<PagedResponse<InventoryMovementResponse>>($"/api/inventory/products/{product.Id}/history");

        Assert.Equal(2, updatedProduct!.CurrentStock);
        Assert.Empty(sales!.Items);
        Assert.Empty(history!.Items);
    }

    [Fact]
    public async Task SaleCreation_WithMissingProduct_FailsWithoutPersistingChanges()
    {
        await RegisterAndAuthenticateAsync($"missing-product-{Guid.NewGuid():N}@example.com", "Missing Product Store");
        var category = await CreateCategoryAsync("Personal Care");
        var product = await CreateProductAsync(category.Id, "Shampoo", "SHAMPOO", 4);

        var saleResponse = await _client.PostAsJsonAsync("/api/sales", new CreateSaleRequest
        {
            PaymentMethod = StockFlow.Domain.Enums.PaymentMethod.Cash,
            Items = new[]
            {
                new CreateSaleItemRequest
                {
                    ProductId = product.Id,
                    Quantity = 1
                },
                new CreateSaleItemRequest
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 1
                }
            }
        });

        Assert.Equal(HttpStatusCode.NotFound, saleResponse.StatusCode);

        var updatedProduct = await _client.GetFromJsonAsync<ProductResponse>($"/api/products/{product.Id}");
        var sales = await _client.GetFromJsonAsync<PagedResponse<SaleResponse>>("/api/sales");
        var history = await _client.GetFromJsonAsync<PagedResponse<InventoryMovementResponse>>($"/api/inventory/products/{product.Id}/history");

        Assert.Equal(4, updatedProduct!.CurrentStock);
        Assert.Empty(sales!.Items);
        Assert.Empty(history!.Items);
    }

    [Fact]
    public async Task SaleCreation_WithProductFromAnotherBusiness_FailsWithoutPersistingChanges()
    {
        var ownerA = await RegisterAndAuthenticateAsync($"tenant-a-sale-{Guid.NewGuid():N}@example.com", "Tenant Sale A");
        var categoryA = await CreateCategoryAsync("Beverages");
        var productA = await CreateProductAsync(categoryA.Id, "Orange Juice", "JUICE", 5);

        await RegisterAndAuthenticateAsync($"tenant-b-sale-{Guid.NewGuid():N}@example.com", "Tenant Sale B");

        var saleResponse = await _client.PostAsJsonAsync("/api/sales", new CreateSaleRequest
        {
            PaymentMethod = StockFlow.Domain.Enums.PaymentMethod.Cash,
            Items = new[]
            {
                new CreateSaleItemRequest
                {
                    ProductId = productA.Id,
                    Quantity = 1
                }
            }
        });

        Assert.Equal(HttpStatusCode.NotFound, saleResponse.StatusCode);

        var businessBSales = await _client.GetFromJsonAsync<PagedResponse<SaleResponse>>("/api/sales");
        Assert.Empty(businessBSales!.Items);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ownerA.Token);

        var updatedProduct = await _client.GetFromJsonAsync<ProductResponse>($"/api/products/{productA.Id}");
        var history = await _client.GetFromJsonAsync<PagedResponse<InventoryMovementResponse>>($"/api/inventory/products/{productA.Id}/history");

        Assert.Equal(5, updatedProduct!.CurrentStock);
        Assert.Empty(history!.Items);
    }

    [Fact]
    public async Task ConcurrentSaleCreation_AllowsSingleWinnerAndPreventsOversell()
    {
        var auth = await RegisterAndAuthenticateAsync($"concurrency-{Guid.NewGuid():N}@example.com", "Concurrency Store");
        var category = await CreateCategoryAsync("Snacks");
        var product = await CreateProductAsync(category.Id, "Cookies", "COOKIE", 1);

        using var firstClient = _factory.CreateClient();
        using var secondClient = _factory.CreateClient();
        firstClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        secondClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        var request = new CreateSaleRequest
        {
            PaymentMethod = StockFlow.Domain.Enums.PaymentMethod.Cash,
            Items = new[]
            {
                new CreateSaleItemRequest
                {
                    ProductId = product.Id,
                    Quantity = 1
                }
            }
        };

        var firstSaleTask = firstClient.PostAsJsonAsync("/api/sales", request);
        var secondSaleTask = secondClient.PostAsJsonAsync("/api/sales", request);

        await Task.WhenAll(firstSaleTask, secondSaleTask);

        var responses = new[] { await firstSaleTask, await secondSaleTask };
        Assert.Equal(1, responses.Count(response => response.StatusCode == HttpStatusCode.Created));
        Assert.Equal(1, responses.Count(response => response.StatusCode == HttpStatusCode.Conflict));

        var updatedProduct = await _client.GetFromJsonAsync<ProductResponse>($"/api/products/{product.Id}");
        var sales = await _client.GetFromJsonAsync<PagedResponse<SaleResponse>>("/api/sales");
        var history = await _client.GetFromJsonAsync<PagedResponse<InventoryMovementResponse>>($"/api/inventory/products/{product.Id}/history");

        Assert.Equal(0, updatedProduct!.CurrentStock);
        Assert.Single(sales!.Items);
        Assert.Single(history!.Items, movement => movement.MovementType == StockFlow.Domain.Enums.InventoryMovementType.Sale && movement.Quantity == 1);
    }

    [Fact]
    public async Task ProductListing_ReturnsPagedEnvelope()
    {
        await RegisterAndAuthenticateAsync($"products-page-{Guid.NewGuid():N}@example.com", "Products Page Store");
        var category = await CreateCategoryAsync("Office");

        for (var index = 1; index <= 5; index++)
        {
            await CreateProductAsync(category.Id, $"Notebook {index}", $"NOTE-{index}", 10 + index);
        }

        var response = await _client.GetFromJsonAsync<PagedResponse<ProductResponse>>("/api/products?page=2&pageSize=2");

        Assert.NotNull(response);
        Assert.Equal(2, response!.Page);
        Assert.Equal(2, response.PageSize);
        Assert.Equal(5, response.TotalItems);
        Assert.Equal(3, response.TotalPages);
        Assert.Equal(2, response.Items.Count);
        Assert.Equal(new[] { "Notebook 3", "Notebook 4" }, response.Items.Select(product => product.Name).ToArray());
    }

    [Fact]
    public async Task SalesListing_ReturnsPagedEnvelope()
    {
        await RegisterAndAuthenticateAsync($"sales-page-{Guid.NewGuid():N}@example.com", "Sales Page Store");
        var category = await CreateCategoryAsync("Household");
        var firstProduct = await CreateProductAsync(category.Id, "Soap", "SOAP", 10);
        var secondProduct = await CreateProductAsync(category.Id, "Detergent", "DETERGENT", 10);
        var thirdProduct = await CreateProductAsync(category.Id, "Bleach", "BLEACH", 10);

        foreach (var productId in new[] { firstProduct.Id, secondProduct.Id, thirdProduct.Id })
        {
            var saleResponse = await _client.PostAsJsonAsync("/api/sales", new CreateSaleRequest
            {
                PaymentMethod = StockFlow.Domain.Enums.PaymentMethod.Cash,
                Items = new[]
                {
                    new CreateSaleItemRequest
                    {
                        ProductId = productId,
                        Quantity = 1
                    }
                }
            });

            saleResponse.EnsureSuccessStatusCode();
        }

        var response = await _client.GetFromJsonAsync<PagedResponse<SaleResponse>>("/api/sales?page=2&pageSize=1");

        Assert.NotNull(response);
        Assert.Equal(2, response!.Page);
        Assert.Equal(1, response.PageSize);
        Assert.Equal(3, response.TotalItems);
        Assert.Equal(3, response.TotalPages);
        Assert.Single(response.Items);
    }

    [Theory]
    [InlineData("/api/products?page=0&pageSize=10")]
    [InlineData("/api/products?page=1&pageSize=101")]
    [InlineData("/api/sales?page=0&pageSize=10")]
    [InlineData("/api/inventory/movements?page=1&pageSize=0")]
    public async Task PaginationQuery_InvalidValues_ReturnBadRequest(string url)
    {
        await RegisterAndAuthenticateAsync($"pagination-validation-{Guid.NewGuid():N}@example.com", "Pagination Validation Store");

        var response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<AuthResponse> RegisterAndAuthenticateAsync(string email, string businessName)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new RegisterRequest
        {
            FullName = businessName + " Owner",
            Email = email,
            Password = "Password123!",
            BusinessName = businessName
        });

        response.EnsureSuccessStatusCode();
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        return auth!;
    }

    private async Task<CategoryResponse> CreateCategoryAsync(string name)
    {
        var response = await _client.PostAsJsonAsync("/api/categories", new CreateCategoryRequest
        {
            Name = name,
            Description = $"{name} shelf"
        });

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CategoryResponse>())!;
    }

    private async Task<ProductResponse> CreateProductAsync(Guid categoryId, string name, string internalCodePrefix, int currentStock)
    {
        var response = await _client.PostAsJsonAsync("/api/products", new CreateProductRequest
        {
            CategoryId = categoryId,
            Name = name,
            InternalCode = $"{internalCodePrefix}-{Guid.NewGuid():N}",
            PurchasePrice = 2m,
            SalePrice = 3.5m,
            CurrentStock = currentStock,
            MinimumStock = 1,
            ExpirationDate = DateTime.UtcNow.Date.AddDays(20)
        });

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ProductResponse>())!;
    }

    private sealed record CategoryResponse(Guid Id, string Name);
}
