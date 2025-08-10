using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using System.Text;

public class RecipeService
{
    private readonly HttpClient _httpClient;
    private readonly string _spoonacularApiKey;
    private readonly string _openAiKey;
    private readonly FirestoreDb _firestoreDb;
    public RecipeService(HttpClient httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
        _spoonacularApiKey = Environment.GetEnvironmentVariable("SPOONOCULAR_API_KEY");
        _openAiKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY");

        if (string.IsNullOrEmpty(_spoonacularApiKey))
        {
            throw new InvalidOperationException("SPOONOCULAR_API_KEY environment variable is not set.");
        }
        if (string.IsNullOrEmpty(_openAiKey))
        {
            throw new InvalidOperationException("OPEN_AI_KEY environment variable is not set.");
        }

        var projectId = Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID");
        var credentialsPath = "/etc/secrets/firebase-key.json";
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);

        _firestoreDb = FirestoreDb.Create(projectId);
    }

    public async Task<List<RecipeDto>> GetRecipesForUserAsync(string userId)
    {
        var firebaseService = new FirebaseService();
        var products = await firebaseService.GetAllProductsAsync(userId);

        if (products.Count == 0)
        {
            return new List<RecipeDto>();
        }
        // Prepare ingredients for the API call
        var ingredientsFi = products.Select(p => p.Name.Trim().ToLower()).OrderBy(x => x).ToList();
        var ingredientsEn = await TranslateToEnglishAsync(string.Join(", ", ingredientsFi));
        var cacheKey = string.Join(",", ingredientsEn.Split(",").Select(s => s.Trim().ToLower()).OrderBy(x => x));

        // Check cache first from the Firestore
        var cacheDoc = await _firestoreDb.Collection("recipes_cache").Document(cacheKey).GetSnapshotAsync();
        if (cacheDoc.Exists && cacheDoc.ContainsField("recipes") && cacheDoc.ContainsField("cachedAt"))
        {
            var cachedAt = cacheDoc.GetValue<DateTime>("cachedAt");
            if (DateTime.UtcNow - cachedAt < TimeSpan.FromHours(24))
            {
                return cacheDoc.GetValue<List<RecipeDto>>("recipes");
            }
        }
        var recipes = await FetchFromSpoonacularAsync(ingredientsEn);

        var cacheData = new Dictionary<string, object>
        {
            { "recipes", recipes },
            { "cachedAt", DateTime.UtcNow }
        };
        await _firestoreDb.Collection("recipes_cache").Document(cacheKey).SetAsync(cacheData);
        return recipes;
    }

    private async Task<List<RecipeDto>> FetchFromSpoonacularAsync(string ingredientsEn)
    {
        var url = $"https://api.spoonacular.com/recipes/findByIngredients?ingredients={Uri.EscapeDataString(ingredientsEn)}&number=5&apiKey={_spoonacularApiKey}";
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var recipes = JsonSerializer.Deserialize<List<RecipeDto>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return recipes ?? new List<RecipeDto>();
    }

    private async Task<string> TranslateToEnglishAsync(string text)
    {
        var url = "https://api.openai.com/v1/chat/completions";
        var requestBody = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new {
                    role = "user",
                    content = $"Translate this text from Finnish to English: \"{text}\""
                }
            },
            temperature = 0.3,
            max_tokens = 150
        };
        var jsonBody = JsonSerializer.Serialize(requestBody);
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Authorization", $"Bearer {_openAiKey}");
        request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);

        var translation = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return translation.Trim();
    }
}