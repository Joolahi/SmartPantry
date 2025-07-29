using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly FirebaseService _firebaseService;
    public ProductsController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
        _firebaseService = new FirebaseService();
    }

    [HttpGet("barcode/{code}")]
    public async Task<IActionResult> GetProductByBarcode(string code)
    {
        var url = $"https://world.openfoodfacts.org/api/v0/product/{code}.json";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, "Error fetching product data.");
        }

        var json = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(json);
        var product = jsonDoc.RootElement.GetProperty("product");

        var result = new
        {
            Name = product.GetProperty("product_name").GetString(),
            Brand = product.GetProperty("brands").GetString(),
            Energy = product.GetProperty("nutriments").GetProperty("energy-kcal").GetDouble(),
            Fat = product.GetProperty("nutriments").TryGetProperty("fat", out var fat) ? fat.GetDouble() : 0,
            Sugars = product.GetProperty("nutriments").TryGetProperty("sugars", out var sugars) ? sugars.GetDouble() : 0,
            Image = product.TryGetProperty("image_url", out var image) ? image.GetString() : null,
        };

        return Ok(result);

    }

    [HttpGet("/{userId}/firebase-products")]
    public async Task<IActionResult> GetFirebaseProducts( string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("User ID cannot be null or empty.");
        }
        var products = await _firebaseService.GetAllProductsAsync(userId);
        return Ok(products);
    }
}