using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

public class RecipeService
{
    private readonly HttpClient _httpClient;
    private readonly string _openAiKey;
    private readonly FirestoreDb _firestoreDb;

    public RecipeService(HttpClient httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
        _openAiKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY");

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

        // Valmistellaan ainesosat suomeksi, käännetään englanniksi
        var ingredientsFi = products.Select(p => p.Name.Trim().ToLower()).OrderBy(x => x).ToList();
        var ingredientsEn = await TranslateToEnglishAsync(string.Join(", ", ingredientsFi));
        var cacheKey = string.Join(",", ingredientsEn.Split(",").Select(s => s.Trim().ToLower()).OrderBy(x => x));

        // Tarkistetaan välimuisti Firebasesta (24h voimassa)
        var cacheDoc = await _firestoreDb.Collection("recipes_cache").Document(cacheKey).GetSnapshotAsync();
        if (cacheDoc.Exists && cacheDoc.ContainsField("recipes") && cacheDoc.ContainsField("cachedAt"))
        {
            var cachedAt = cacheDoc.GetValue<DateTime>("cachedAt");
            if (DateTime.UtcNow - cachedAt < TimeSpan.FromHours(24))
            {
                return cacheDoc.GetValue<List<RecipeDto>>("recipes");
            }
        }

        // Haetaan reseptit OpenAI:lta
        var recipes = await FetchRecipesFromOpenAIAsync(ingredientsEn);

        // Tallennetaan välimuistiin Firebasessa
        var cacheData = new Dictionary<string, object>
        {
            { "recipes", recipes },
            { "cachedAt", DateTime.UtcNow }
        };
        await _firestoreDb.Collection("recipes_cache").Document(cacheKey).SetAsync(cacheData);

        return recipes;
    }

    private async Task<List<RecipeDto>> FetchRecipesFromOpenAIAsync(string ingredientsEn)
    {
        var prompt =
            $"You are a helpful assistant. Given these ingredients: {ingredientsEn}, " +
            "please provide 3 recipe suggestions. Each recipe should include a title and a short description. " +
            "Return the response as a JSON array with objects containing 'title' and 'description'.";

        var url = "https://api.openai.com/v1/chat/completions";
        var requestBody = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new { role = "system", content = "You are a helpful cooking assistant." },
                new { role = "user", content = prompt }
            },
            temperature = 0.7,
            max_tokens = 600
        };

        var jsonBody = JsonSerializer.Serialize(requestBody);
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Authorization", $"Bearer {_openAiKey}");
        request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            // Rate limit - voit halutessasi käsitellä toisin
            return new List<RecipeDto>();
        }
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);

        var content = doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        // Yritetään parsia OpenAI:n vastaus JSON-listaksi
        try
        {
            var recipes = JsonSerializer.Deserialize<List<RecipeDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (recipes != null)
                return recipes;
        }
        catch
        {
            // Jos JSON-parsaus epäonnistuu, voit halutessasi logittaa tai käsitellä virheen
        }

        // Palautetaan tyhjä lista jos parsinta epäonnistuu
        return new List<RecipeDto>();
    }

    private async Task<string> TranslateToEnglishAsync(string text)
    {
        var url = "https://api.openai.com/v1/chat/completions";
        var requestBody = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new { role = "user", content = $"Translate this text from Finnish to English: \"{text}\"" }
            },
            temperature = 0.3,
            max_tokens = 150
        };

        var jsonBody = JsonSerializer.Serialize(requestBody);
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Authorization", $"Bearer {_openAiKey}");
        request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            return text; // fallback: palauta suomi jos liian monta pyyntöä
        }
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