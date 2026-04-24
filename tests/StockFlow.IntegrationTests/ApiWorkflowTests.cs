using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using StockFlow.Application.DTOs.Auth;
using StockFlow.Application.DTOs.Categories;
using StockFlow.Application.DTOs.Inventory;
using StockFlow.Application.DTOs.Products;
using StockFlow.Application.DTOs.Sales;

namespace StockFlow.IntegrationTests;

public sealed class ApiWorkflowTests : IClassFixture<StockFlowApiFactory>
{
    private readonly HttpClient _client;

    public ApiWorkflowTests(StockFlowApiFactory factory)
    {
        _client = factory.CreateClient();
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
        var auth = await RegisterAndAuthenticateAsync("sales@example.com", "Sales Store");

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

        var history = await _client.GetFromJsonAsync<List<InventoryMovementResponse>>($"/api/inventory/products/{product.Id}/history");
        Assert.NotNull(history);
        Assert.Contains(history!, movement => movement.MovementType == StockFlow.Domain.Enums.InventoryMovementType.Sale && movement.Quantity == 3);
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

    private sealed record CategoryResponse(Guid Id, string Name);
}
